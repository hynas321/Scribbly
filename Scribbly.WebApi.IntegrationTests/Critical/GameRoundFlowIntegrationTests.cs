using Scribbly.Application.Features.Chat;
using Scribbly.Application.Features.Games;
using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Features.Rounds;
using Scribbly.Application.Security;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.IntegrationTests.Critical;

public class GameRoundFlowIntegrationTests
{
    private static readonly GamePrompt LuckyCat = new("lucky cat", "en", "animals", "phrase");
    private static readonly GameSettings DefaultSettings = new() { DrawingTimeSeconds = 75, RoundsCount = 2, PromptLanguage = "en" };

    private class Session
    {
        public required InMemoryGameStateStore Games { get; init; }
        public required RoomCredentialService Credentials { get; init; }
        public required GameLifecycleService Lifecycle { get; init; }
        public required GameRoundService Rounds { get; init; }
        public required GameChatService Chat { get; init; }
        public required CreateGameResult Room { get; init; }
        public required string HostHash { get; set; }
        public List<string> GuestTokens { get; } = new();

        public static async Task<Session> CreateAsync(int guestCount = 1, int maxPlayers = 4)
        {
            var games = new InMemoryGameStateStore();
            var canvas = new InMemoryCanvasStateStore();
            var credentials = new RoomCredentialService();
            var lifecycle = new GameLifecycleService(games, games, canvas, credentials,
                new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), maxPlayers), GameSettingsTestOptions.Create());
            var session = new Session
            {
                Games = games,
                Credentials = credentials,
                Lifecycle = lifecycle,
                Rounds = new GameRoundService(games, canvas, credentials, GameSettingsTestOptions.Create()),
                Chat = new GameChatService(games, credentials, ChatTestOptions.Create()),
                Room = await lifecycle.CreateAsync("host"),
                HostHash = string.Empty
            };
            await lifecycle.JoinAsync(session.Room.GameHash, session.Room.RoomAccessToken, "host-connection");
            session.HostHash = credentials.ComputeCredentialHash(session.Room.RoomAccessToken);
            for (int i = 0; i < guestCount; i++)
            {
                JoinCredentialResult credential = await lifecycle.IssueJoinCredentialAsync(session.Room.GameHash, $"guest{i + 1}");
                await lifecycle.JoinAsync(session.Room.GameHash, credential.RoomAccessToken!, $"guest{i + 1}-connection");
                session.GuestTokens.Add(credential.RoomAccessToken!);
            }
            return session;
        }

        public async Task StartSessionAndActivateAsync(GamePrompt prompt)
        {
            Assert.Equal(RoundCommandStatus.Applied,
                (await Rounds.StartAuthorizedSessionAsync(Room.GameHash, Room.RoomAccessToken, DefaultSettings)).Status);
            await Rounds.PrepareRoundAsync(Room.GameHash);
            Assert.Equal(RoundCommandStatus.Applied,
                (await Rounds.StartTurnAsync(Room.GameHash, HostHash)).Status);
            Assert.Equal(RoundCommandStatus.Applied,
                await Rounds.ActivatePromptAsync(Room.GameHash, HostHash, prompt));
        }
    }

    [Fact]
    public async Task FullRound_FromCreateToCorrectGuess_UpdatesScoresAndHint()
    {
        Session session = await Session.CreateAsync();
        await session.StartSessionAndActivateAsync(LuckyCat);

        SendChatMessageResult wrong = await session.Chat.SendAsync(session.Room.GameHash, session.GuestTokens[0], "lucky dog");
        SendChatMessageResult correct = await session.Chat.SendAsync(session.Room.GameHash, session.GuestTokens[0], "  LUCKY cat  ");
        SendChatMessageResult afterCorrectGuess = await session.Chat.SendAsync(
            session.Room.GameHash,
            session.GuestTokens[0],
            "another guess");
        Game snapshot = (await session.Games.GetSnapshotAsync(session.Room.GameHash))!;

        Assert.Equal(SendChatMessageStatus.Sent, wrong.Status);
        Assert.Equal(SendChatMessageStatus.CorrectGuess, correct.Status);
        Assert.Equal(SendChatMessageStatus.ChatForbidden, afterCorrectGuess.Status);
        Assert.Contains(snapshot.GameState.Players, p => p.Username == "guest1" && p.Score == 10);
        Assert.True(snapshot.GameState.CorrectAnswerCount >= 1);
        Assert.Contains("guest1", snapshot.GameState.CorrectGuessPlayerUsernames);
        Assert.Contains(correct.Scores!, s => s.Username == "guest1" && s.Score == 10);
        Assert.Contains("guest1", correct.CorrectGuessPlayerUsernames!);
        Assert.Equal("_____ ___", snapshot.GameState.HiddenPromptText);
        Assert.DoesNotContain("lucky cat", snapshot.GameState.HiddenPromptText);
    }

    [Fact]
    public async Task EveryoneGuessed_RoundCompletesOnNextTick()
    {
        Session session = await Session.CreateAsync(guestCount: 2);
        await session.StartSessionAndActivateAsync(LuckyCat);

        foreach (string guestToken in session.GuestTokens)
        {
            Assert.Equal(SendChatMessageStatus.CorrectGuess,
                (await session.Chat.SendAsync(session.Room.GameHash, guestToken, "lucky cat")).Status);
        }

        TimerTickResult tick = await session.Rounds.TickTimerAsync(session.Room.GameHash);

        Assert.Equal(RoundCommandStatus.Applied, tick.Status);
        Assert.True(tick.Completed);
    }

    [Fact]
    public async Task StartSession_ByNonHostToken_IsRejected()
    {
        Session session = await Session.CreateAsync();

        StartSessionResult result = await session.Rounds.StartAuthorizedSessionAsync(
            session.Room.GameHash, session.GuestTokens[0], DefaultSettings);

        Assert.Equal(RoundCommandStatus.InvalidHostCredential, result.Status);
        Game snapshot = (await session.Games.GetSnapshotAsync(session.Room.GameHash))!;
        Assert.False(snapshot.GameState.IsGameStarted);
    }

    [Fact]
    public async Task ActivatePrompt_ByNonDrawingPlayer_IsRejected()
    {
        Session session = await Session.CreateAsync();
        StartSessionResult started = await session.Rounds.StartAuthorizedSessionAsync(
            session.Room.GameHash,
            session.Room.RoomAccessToken,
            DefaultSettings);
        Assert.Equal(RoundCommandStatus.Applied, started.Status);
        await session.Rounds.PrepareRoundAsync(session.Room.GameHash);
        await session.Rounds.StartTurnAsync(session.Room.GameHash, session.HostHash);

        RoundCommandStatus result = await session.Rounds.ActivatePromptAsync(
            session.Room.GameHash, session.Credentials.ComputeCredentialHash(session.GuestTokens[0]), LuckyCat);

        Assert.Equal(RoundCommandStatus.InvalidDrawingPlayer, result);
    }

    [Fact]
    public async Task JoinBeyondCapacity_IsRejected()
    {
        Session session = await Session.CreateAsync(maxPlayers: 2);
        var credentials = new RoomCredentialService();

        JoinCredentialResult third = await session.Lifecycle.IssueJoinCredentialAsync(session.Room.GameHash, "third");
        JoinGameResult fabricated = await session.Lifecycle.JoinAsync(session.Room.GameHash, credentials.GenerateAccessToken(), "intruder");

        Assert.Equal(JoinCredentialStatus.RoomFull, third.Status);
        Assert.Equal(JoinGameStatus.InvalidCredential, fabricated.Status);
    }

    [Fact]
    public async Task DisconnectMidRound_PausesTimerUntilReconnect()
    {
        Session session = await Session.CreateAsync();
        await session.StartSessionAndActivateAsync(LuckyCat);

        await session.Lifecycle.RemoveByConnectionIdAsync("guest1-connection");
        TimerTickResult paused = await session.Rounds.TickTimerAsync(session.Room.GameHash);
        JoinGameResult resumed = await session.Lifecycle.ReconnectAsync(
            session.Room.GameHash,
            session.GuestTokens[0],
            "guest1-reconnected");
        TimerTickResult resumedTick = await session.Rounds.TickTimerAsync(session.Room.GameHash);

        Assert.Equal(RoundCommandStatus.WaitingForPlayers, paused.Status);
        Assert.Equal(75, paused.CurrentDrawingTimeSeconds);
        Assert.Equal(JoinGameStatus.Resumed, resumed.Status);
        Assert.Equal(RoundCommandStatus.Applied, resumedTick.Status);
        Assert.Equal(75, resumedTick.CurrentDrawingTimeSeconds);
        Game snapshot = (await session.Games.GetSnapshotAsync(session.Room.GameHash))!;
        Assert.Equal(74, snapshot.GameState.CurrentDrawingTimeSeconds);
    }
}
