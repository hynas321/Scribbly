using Scribbly.Application.Features.Chat;
using Scribbly.Application.Features.Games;
using Scribbly.Application.Features.Rounds;
using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Security;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.IntegrationTests.Critical;

public class PromptGameFlowIntegrationTests
{
    [Fact]
    public async Task PhrasePrompt_CanFlowFromRoundActivationToCorrectGuess()
    {
        var games = new InMemoryGameStateStore();
        var canvas = new InMemoryCanvasStateStore();
        var credentials = new RoomCredentialService();
        var lifecycle = new GameLifecycleService(games, games, canvas, credentials,
            new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 12), GameSettingsTestOptions.Create());
        var rounds = new GameRoundService(games, canvas, credentials, GameSettingsTestOptions.Create());
        var chat = new GameChatService(games, credentials, ChatTestOptions.Create());

        CreateGameResult room = await lifecycle.CreateAsync("host");
        await lifecycle.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
        JoinCredentialResult guestCredential = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "alice");
        await lifecycle.JoinAsync(room.GameHash, guestCredential.RoomAccessToken!, "guest-connection");

        var settings = new GameSettings { DrawingTimeSeconds = 75, RoundsCount = 2, PromptLanguage = "en" };
        StartSessionResult started = await rounds.StartAuthorizedSessionAsync(
            room.GameHash,
            room.RoomAccessToken,
            settings);
        Assert.Equal(RoundCommandStatus.Applied, started.Status);
        PrepareRoundResult prepared = await rounds.PrepareRoundAsync(room.GameHash);
        string hostHash = credentials.ComputeCredentialHash(room.RoomAccessToken);
        StartTurnResult turn = await rounds.StartTurnAsync(room.GameHash, hostHash);
        var prompt = new GamePrompt("walking the dog", "en", "everyday activities", PromptKinds.Phrase);
        Assert.Equal(RoundCommandStatus.Applied, await rounds.ActivatePromptAsync(room.GameHash, hostHash, prompt));

        SendChatMessageResult guess = await chat.SendAsync(room.GameHash, guestCredential.RoomAccessToken!, "  WALKING THE DOG  ");
        Game snapshot = (await games.GetSnapshotAsync(room.GameHash))!;

        Assert.Equal(RoundCommandStatus.Applied, turn.Status);
        Assert.Equal(SendChatMessageStatus.CorrectGuess, guess.Status);
        Assert.Equal("phrase", snapshot.GameState.PromptKind);
        Assert.Equal("everyday activities", snapshot.GameState.PromptCategory);
        Assert.Equal("_______ ___ ___", snapshot.GameState.HiddenPromptText);
    }

    [Fact]
    public async Task SayingPrompt_ProducesHintWithoutLeakingAnswer()
    {
        var prompt = new GamePrompt("better late than never", "en", "proverbs", PromptKinds.Saying);
        Assert.Equal("______ ____ ____ _____", prompt.HiddenHint);
        Assert.DoesNotContain("saying", prompt.HiddenHint);
        Assert.DoesNotContain("proverbs", prompt.HiddenHint);
        Assert.DoesNotContain(prompt.Text, prompt.HiddenHint);
    }
}
