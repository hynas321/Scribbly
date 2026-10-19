using Scribbly.Application.Features.Chat;
using Scribbly.Application.Features.Games;
using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Features.Rounds;
using Scribbly.Application.Features.Settings;
using Scribbly.Application.Security;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.UnitTests.Rounds;

public class GameRoundServiceTests
{
    private static readonly GamePrompt WalkingTheDog = new("walking the dog", "en", "activities", PromptKinds.Phrase);
    private static readonly GameSettings TwoRounds = new() { DrawingTimeSeconds = 75, RoundsCount = 2, PromptLanguage = "en" };

    private class Fixture
    {
        public required InMemoryGameStateStore Games { get; init; }
        public required RoomCredentialService Credentials { get; init; }
        public required GameLifecycleService Lifecycle { get; init; }
        public required GameRoundService Rounds { get; init; }
        public required GameChatService Chat { get; init; }
        public required CreateGameResult Room { get; init; }
        public required string HostHash { get; set; }
        public List<string> GuestTokens { get; } = new();

        public static async Task<Fixture> CreateAsync(int guestCount = 1)
        {
            var games = new InMemoryGameStateStore();
            var canvas = new InMemoryCanvasStateStore();
            var credentials = new RoomCredentialService();
            var lifecycle = new GameLifecycleService(games, games, canvas, credentials,
                new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 4), GameSettingsTestOptions.Create());
            var fixture = new Fixture
            {
                Games = games,
                Credentials = credentials,
                Lifecycle = lifecycle,
                Rounds = new GameRoundService(games, canvas, credentials, GameSettingsTestOptions.Create()),
                Chat = new GameChatService(games, credentials, ChatTestOptions.Create()),
                Room = await lifecycle.CreateAsync("host"),
                HostHash = string.Empty
            };
            await lifecycle.JoinAsync(fixture.Room.GameHash, fixture.Room.RoomAccessToken, "host-connection");
            fixture.HostHash = credentials.ComputeCredentialHash(fixture.Room.RoomAccessToken);
            for (int i = 0; i < guestCount; i++)
            {
                JoinCredentialResult credential = await lifecycle.IssueJoinCredentialAsync(fixture.Room.GameHash, $"guest{i + 1}");
                await lifecycle.JoinAsync(fixture.Room.GameHash, credential.RoomAccessToken!, $"guest{i + 1}-connection");
                fixture.GuestTokens.Add(credential.RoomAccessToken!);
            }
            return fixture;
        }

        public async Task StartUpToTurnAsync()
        {
            Assert.Equal(RoundCommandStatus.Applied,
                (await Rounds.StartAuthorizedSessionAsync(Room.GameHash, Room.RoomAccessToken, TwoRounds)).Status);
            await Rounds.PrepareRoundAsync(Room.GameHash);
            Assert.Equal(RoundCommandStatus.Applied,
                (await Rounds.StartTurnAsync(Room.GameHash, HostHash)).Status);
        }

        public async Task StartRoundAsync()
        {
            await StartUpToTurnAsync();
            Assert.Equal(RoundCommandStatus.Applied,
                await Rounds.ActivatePromptAsync(Room.GameHash, HostHash, WalkingTheDog));
        }
    }

    [Fact]
    public async Task StartAuthorizedSession_TooFewPlayers_IsRejected()
    {
        Fixture fixture = await Fixture.CreateAsync(guestCount: 0);

        StartSessionResult result = await fixture.Rounds.StartAuthorizedSessionAsync(
            fixture.Room.GameHash, fixture.Room.RoomAccessToken, TwoRounds);

        Assert.Equal(RoundCommandStatus.TooFewPlayers, result.Status);
    }

    [Fact]
    public async Task StartAuthorizedSession_AppliesThenReportsAlreadyRunning()
    {
        Fixture fixture = await Fixture.CreateAsync();

        StartSessionResult first = await fixture.Rounds.StartAuthorizedSessionAsync(
            fixture.Room.GameHash, fixture.Room.RoomAccessToken, TwoRounds);
        StartSessionResult second = await fixture.Rounds.StartAuthorizedSessionAsync(
            fixture.Room.GameHash, fixture.Room.RoomAccessToken, TwoRounds);

        Assert.Equal(RoundCommandStatus.Applied, first.Status);
        Assert.Equal(RoundCommandStatus.AlreadyRunning, second.Status);
    }

    [Fact]
    public async Task StartAuthorizedSession_WithNonHostToken_IsRejected()
    {
        Fixture fixture = await Fixture.CreateAsync();

        StartSessionResult result = await fixture.Rounds.StartAuthorizedSessionAsync(
            fixture.Room.GameHash, fixture.GuestTokens[0], TwoRounds);

        Assert.Equal(RoundCommandStatus.InvalidHostCredential, result.Status);
    }

    [Fact]
    public async Task StartAuthorizedSession_WithInvalidSettings_IsRejected()
    {
        Fixture fixture = await Fixture.CreateAsync();

        StartSessionResult result = await fixture.Rounds.StartAuthorizedSessionAsync(
            fixture.Room.GameHash, fixture.Room.RoomAccessToken,
            new GameSettings { DrawingTimeSeconds = 10, RoundsCount = 2, PromptLanguage = "en" });

        Assert.Equal(RoundCommandStatus.InvalidSettings, result.Status);
    }

    [Fact]
    public async Task StartAuthorizedSession_UnknownGame_ReturnsGameNotFound()
    {
        Fixture fixture = await Fixture.CreateAsync();

        StartSessionResult result = await fixture.Rounds.StartAuthorizedSessionAsync("UNKNOWNHASH00000", fixture.Room.RoomAccessToken, TwoRounds);

        Assert.Equal(RoundCommandStatus.GameNotFound, result.Status);
    }

    [Fact]
    public async Task StartTurn_WithWrongHash_IsRejected()
    {
        Fixture fixture = await Fixture.CreateAsync();
        Assert.Equal(RoundCommandStatus.Applied,
            (await fixture.Rounds.StartAuthorizedSessionAsync(fixture.Room.GameHash, fixture.Room.RoomAccessToken, TwoRounds)).Status);
        await fixture.Rounds.PrepareRoundAsync(fixture.Room.GameHash);
        string unknownHash = fixture.Credentials.ComputeCredentialHash(fixture.Credentials.GenerateAccessToken());

        StartTurnResult result = await fixture.Rounds.StartTurnAsync(fixture.Room.GameHash, unknownHash);

        Assert.Equal(RoundCommandStatus.InvalidDrawingPlayer, result.Status);
    }

    [Fact]
    public async Task ActivatePrompt_WithWrongHash_IsRejected()
    {
        Fixture fixture = await Fixture.CreateAsync();

        await fixture.StartUpToTurnAsync();
        string unknownHash = fixture.Credentials.ComputeCredentialHash(fixture.Credentials.GenerateAccessToken());

        RoundCommandStatus result = await fixture.Rounds.ActivatePromptAsync(fixture.Room.GameHash, unknownHash, WalkingTheDog);

        Assert.Equal(RoundCommandStatus.InvalidDrawingPlayer, result);
    }

    [Fact]
    public async Task ActivatePrompt_ByOtherPlayer_IsRejected()
    {
        Fixture fixture = await Fixture.CreateAsync();

        await fixture.StartUpToTurnAsync();

        RoundCommandStatus result = await fixture.Rounds.ActivatePromptAsync(
            fixture.Room.GameHash, fixture.Credentials.ComputeCredentialHash(fixture.GuestTokens[0]), WalkingTheDog);

        Assert.Equal(RoundCommandStatus.InvalidDrawingPlayer, result);
    }

    [Fact]
    public async Task ActivatePrompt_CalledTwice_IsIdempotentApplied()
    {
        Fixture fixture = await Fixture.CreateAsync();

        await fixture.StartUpToTurnAsync();

        RoundCommandStatus first = await fixture.Rounds.ActivatePromptAsync(fixture.Room.GameHash, fixture.HostHash, WalkingTheDog);
        long versionAfterFirst = (await fixture.Games.GetSnapshotAsync(fixture.Room.GameHash))!.Version;
        RoundCommandStatus second = await fixture.Rounds.ActivatePromptAsync(fixture.Room.GameHash, fixture.HostHash, WalkingTheDog);
        Game snapshot = (await fixture.Games.GetSnapshotAsync(fixture.Room.GameHash))!;

        Assert.Equal(RoundCommandStatus.Applied, first);
        Assert.Equal(RoundCommandStatus.Applied, second);
        Assert.Equal(versionAfterFirst, snapshot.Version);
        Assert.Equal("walking the dog", snapshot.GameState.ActualPromptText);
    }

    [Fact]
    public async Task TickTimer_DecrementsRemainingTime()
    {
        Fixture fixture = await Fixture.CreateAsync();
        await fixture.StartRoundAsync();

        TimerTickResult tick = await fixture.Rounds.TickTimerAsync(fixture.Room.GameHash);
        Game snapshot = (await fixture.Games.GetSnapshotAsync(fixture.Room.GameHash))!;

        Assert.Equal(RoundCommandStatus.Applied, tick.Status);
        Assert.False(tick.Completed);
        Assert.Equal(75, tick.CurrentDrawingTimeSeconds);
        Assert.Equal(74, snapshot.GameState.CurrentDrawingTimeSeconds);
    }

    [Fact]
    public async Task TickTimer_WhenAllNonDrawersGuessed_ReportsCompleted()
    {
        Fixture fixture = await Fixture.CreateAsync(guestCount: 3);
        await fixture.StartRoundAsync();

        foreach (string guestToken in fixture.GuestTokens)
        {
            SendChatMessageResult guess = await fixture.Chat.SendAsync(fixture.Room.GameHash, guestToken, "walking the dog");
            Assert.Equal(SendChatMessageStatus.CorrectGuess, guess.Status);
        }
        TimerTickResult tick = await fixture.Rounds.TickTimerAsync(fixture.Room.GameHash);

        Assert.Equal(RoundCommandStatus.Applied, tick.Status);
        Assert.True(tick.Completed);
        Assert.Equal(75, tick.CurrentDrawingTimeSeconds);
    }

    [Fact]
    public async Task TickTimer_HintsDisabled_KeepsFullMask()
    {
        Fixture fixture = await Fixture.CreateAsync();
        await fixture.StartRoundAsync();

        TimerTickResult tick = await fixture.Rounds.TickTimerAsync(fixture.Room.GameHash);
        var snapshot = await fixture.Games.GetSnapshotAsync(fixture.Room.GameHash);

        Assert.False(tick.HintChanged);
        Assert.Equal("_______ ___ ___", snapshot!.GameState.HiddenPromptText);
    }

    [Fact]
    public async Task TickTimer_WithNormalHints_UsesTimingAndPercentageCap()
    {
        Fixture fixture = await Fixture.CreateAsync();
        var settingsService = new GameSettingsService(fixture.Games, fixture.Credentials, new FakePromptRepository(), GameSettingsTestOptions.Create());
        Assert.Equal(GameSettingsCommandStatus.Applied,
            (await settingsService.SetDrawingTimeAsync(fixture.Room.GameHash, fixture.Room.RoomAccessToken, 30)).Status);
        Assert.Equal(GameSettingsCommandStatus.Applied,
            (await settingsService.SetHintSpeedAsync(fixture.Room.GameHash, fixture.Room.RoomAccessToken, "normal")).Status);
        Assert.Equal(GameSettingsCommandStatus.Applied,
            (await settingsService.SetHintPercentageAsync(fixture.Room.GameHash, fixture.Room.RoomAccessToken, 25)).Status);
        Assert.Equal(RoundCommandStatus.Applied,
            (await fixture.Rounds.StartAuthorizedSessionAsync(fixture.Room.GameHash, fixture.Room.RoomAccessToken,
                new GameSettings { DrawingTimeSeconds = 30, RoundsCount = 1, PromptLanguage = "en" })).Status);
        await fixture.Rounds.PrepareRoundAsync(fixture.Room.GameHash);
        await fixture.Rounds.StartTurnAsync(fixture.Room.GameHash, fixture.HostHash);
        await fixture.Rounds.ActivatePromptAsync(fixture.Room.GameHash, fixture.HostHash, WalkingTheDog);

        var revealsAt = new List<int>();
        for (int expected = 30; expected > 1; expected--)
        {
            TimerTickResult tick = await fixture.Rounds.TickTimerAsync(fixture.Room.GameHash);
            Assert.Equal(expected, tick.CurrentDrawingTimeSeconds);
            if (tick.HintChanged)
            {
                revealsAt.Add(expected);
            }
        }
        var snapshot = await fixture.Games.GetSnapshotAsync(fixture.Room.GameHash);
        string mask = snapshot!.GameState.HiddenPromptText;

        Assert.Equal(new[] { 16, 10 }, revealsAt);
        Assert.Equal("_______ ___ ___".Length, mask.Length);
        Assert.Equal(' ', mask[7]);
        Assert.Equal(' ', mask[11]);
        Assert.Equal(2, mask.Count(c => c is not ('_' or ' ')));
        foreach (char c in mask.Where(c => c is not ('_' or ' ')))
        {
            Assert.Contains(c, "walking the dog");
        }

        TimerTickResult last = await fixture.Rounds.TickTimerAsync(fixture.Room.GameHash);
        Assert.False(last.HintChanged);
    }
}
