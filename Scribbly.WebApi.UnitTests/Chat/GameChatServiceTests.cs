using Scribbly.Application.Features.Chat;
using Scribbly.Application.Features.Games;
using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Features.Rounds;
using Scribbly.Application.Security;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.UnitTests.Chat;

public class GameChatServiceTests
{
    private static async Task<(GameChatService Chat, InMemoryGameStateStore Games, RoomCredentialService Credentials,
        CreateGameResult Room, string GuestToken, string HostHash)> CreateStartedGameWithActivePrompt(
        GamePrompt? prompt = null, int guestCount = 1)
    {
        var games = new InMemoryGameStateStore();
        var canvas = new InMemoryCanvasStateStore();
        var credentials = new RoomCredentialService();
        var lifecycle = new GameLifecycleService(games, games, canvas, credentials,
            new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 4), GameSettingsTestOptions.Create());
        var rounds = new GameRoundService(games, canvas, credentials, GameSettingsTestOptions.Create());
        var chat = new GameChatService(games, credentials, ChatTestOptions.Create());

        CreateGameResult room = await lifecycle.CreateAsync("host");
        await lifecycle.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
        string guestToken = string.Empty;
        for (int i = 0; i < guestCount; i++)
        {
            JoinCredentialResult credential = await lifecycle.IssueJoinCredentialAsync(room.GameHash, $"guest{i + 1}");
            await lifecycle.JoinAsync(room.GameHash, credential.RoomAccessToken!, $"guest{i + 1}-connection");
            if (i == 0)
            {
                guestToken = credential.RoomAccessToken!;
            }
        }

        await rounds.StartAuthorizedSessionAsync(room.GameHash, room.RoomAccessToken,
            new GameSettings { DrawingTimeSeconds = 75, RoundsCount = 2, PromptLanguage = "en" });
        await rounds.PrepareRoundAsync(room.GameHash);
        string hostHash = credentials.ComputeCredentialHash(room.RoomAccessToken);
        await rounds.StartTurnAsync(room.GameHash, hostHash);
        if (prompt is not null)
        {
            await rounds.ActivatePromptAsync(room.GameHash, hostHash, prompt);
        }
        return (chat, games, credentials, room, guestToken, hostHash);
    }

    [Fact]
    public async Task CorrectGuess_AwardsScoreAndUpdatesSnapshot()
    {
        var (chat, games, _, room, guestToken, _) =
            await CreateStartedGameWithActivePrompt(new GamePrompt("walking the dog", "en", "activities", PromptKinds.Phrase));

        SendChatMessageResult guess = await chat.SendAsync(room.GameHash, guestToken, "  Walking The DOG  ");
        Game snapshot = (await games.GetSnapshotAsync(room.GameHash))!;
        Player guest = snapshot.GameState.Players.Single(p => p.Username == "guest1");

        Assert.Equal(SendChatMessageStatus.CorrectGuess, guess.Status);
        Assert.Equal("guest1", guess.Username);
        Assert.Equal(10, guess.AwardedScore);
        Assert.Equal(10, guest.Score);
        Assert.Equal(1, snapshot.GameState.CorrectAnswerCount);
        Assert.Contains("guest1", snapshot.GameState.CorrectGuessPlayerUsernames);
        Assert.Contains(guess.Scores!, s => s.Username == "guest1" && s.Score == 10);
    }

    [Fact]
    public async Task WrongGuess_IsStoredAsPlainMessageWithoutScore()
    {
        var (chat, games, _, room, guestToken, _) =
            await CreateStartedGameWithActivePrompt(new GamePrompt("walking the dog", "en", "activities", PromptKinds.Phrase));

        SendChatMessageResult result = await chat.SendAsync(room.GameHash, guestToken, "running the cat");
        Game snapshot = (await games.GetSnapshotAsync(room.GameHash))!;

        Assert.Equal(SendChatMessageStatus.Sent, result.Status);
        Assert.Equal("running the cat", result.Message!.Text);
        Assert.Equal(0, snapshot.GameState.CorrectAnswerCount);
        Assert.All(snapshot.GameState.Players, p => Assert.Equal(0, p.Score));
        Assert.Single(snapshot.ChatMessages);
    }

    [Fact]
    public async Task CorrectGuessTwice_SecondAttemptRejectedWithoutDoubleScore()
    {
        var (chat, games, _, room, guestToken, _) =
            await CreateStartedGameWithActivePrompt(new GamePrompt("walking the dog", "en", "activities", PromptKinds.Phrase));

        SendChatMessageResult first = await chat.SendAsync(room.GameHash, guestToken, "walking the dog");
        SendChatMessageResult second = await chat.SendAsync(room.GameHash, guestToken, "walking the dog");
        Game snapshot = (await games.GetSnapshotAsync(room.GameHash))!;

        Assert.Equal(SendChatMessageStatus.CorrectGuess, first.Status);
        Assert.Equal(SendChatMessageStatus.ChatForbidden, second.Status);
        Assert.Equal(1, snapshot.GameState.CorrectAnswerCount);
        Assert.Equal(10, snapshot.GameState.Players.Single(p => p.Username == "guest1").Score);
    }

    [Fact]
    public async Task DrawingPlayerSendingOwnPrompt_IsRejectedAndNeverScored()
    {
        var (chat, games, credentials, room, _, _) =
            await CreateStartedGameWithActivePrompt(new GamePrompt("walking the dog", "en", "activities", PromptKinds.Phrase));

        SendChatMessageResult result = await chat.SendAsync(room.GameHash, room.RoomAccessToken, "walking the dog");
        Game snapshot = (await games.GetSnapshotAsync(room.GameHash))!;
        string hostHash = credentials.ComputeCredentialHash(room.RoomAccessToken);

        Assert.Equal(SendChatMessageStatus.DrawingPlayerCannotChat, result.Status);
        Assert.Equal(0, snapshot.GameState.CorrectAnswerCount);
        Assert.Empty(snapshot.ChatMessages);
        Assert.Equal(0, snapshot.GameState.Players.Single(p => p.CredentialHash == hostHash).Score);
    }

    [Fact]
    public async Task GuessBeforePromptIsActive_IsPlainMessageWithoutScoring()
    {
        var (chat, games, _, room, guestToken, _) = await CreateStartedGameWithActivePrompt(prompt: null);

        SendChatMessageResult result = await chat.SendAsync(room.GameHash, guestToken, "walking the dog");
        Game snapshot = (await games.GetSnapshotAsync(room.GameHash))!;

        Assert.Equal(SendChatMessageStatus.Sent, result.Status);
        Assert.Equal(0, snapshot.GameState.CorrectAnswerCount);
        Assert.All(snapshot.GameState.Players, p => Assert.Equal(0, p.Score));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EmptyMessage_IsRejected(string text)
    {
        var (chat, _, _, room, guestToken, _) = await CreateStartedGameWithActivePrompt();

        SendChatMessageResult result = await chat.SendAsync(room.GameHash, guestToken, text);

        Assert.Equal(SendChatMessageStatus.EmptyText, result.Status);
    }

    [Fact]
    public async Task MessageLengthBoundary_IsEnforced()
    {
        var (chat, _, _, room, guestToken, _) = await CreateStartedGameWithActivePrompt();

        int maxMessageLength = ChatTestOptions.Create().MaxMessageLength;
        SendChatMessageResult atLimit = await chat.SendAsync(room.GameHash, guestToken, new string('a', maxMessageLength));
        SendChatMessageResult overLimit = await chat.SendAsync(room.GameHash, guestToken, new string('a', maxMessageLength + 1));

        Assert.Equal(SendChatMessageStatus.Sent, atLimit.Status);
        Assert.Equal(SendChatMessageStatus.MessageTooLong, overLimit.Status);
    }

    [Fact]
    public async Task SenderNotInRoom_IsRejected()
    {
        var (chat, _, credentials, room, _, _) = await CreateStartedGameWithActivePrompt();

        SendChatMessageResult result = await chat.SendAsync(room.GameHash, credentials.GenerateAccessToken(), "hello");

        Assert.Equal(SendChatMessageStatus.PlayerNotFound, result.Status);
    }

    [Fact]
    public async Task SendToUnknownGame_ReturnsGameNotFound()
    {
        var chat = new GameChatService(
            new InMemoryGameStateStore(),
            new RoomCredentialService(),
            ChatTestOptions.Create());

        SendChatMessageResult result = await chat.SendAsync("UNKNOWNHASH00000", "some-token", "hello");

        Assert.Equal(SendChatMessageStatus.GameNotFound, result.Status);
    }
}
