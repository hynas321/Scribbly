using Scribbly.Application.Features.Games;
using Scribbly.Application.Features.Rounds;
using Scribbly.Application.Features.Settings;
using Scribbly.Application.Security;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.IntegrationTests.Critical;

public class GameSettingsFlowIntegrationTests
{
    [Fact]
    public async Task HostSettings_AreVisibleToPlayersAndBecomeImmutableWhenGameStarts()
    {
        var games = new InMemoryGameStateStore();
        var canvas = new InMemoryCanvasStateStore();
        var credentials = new RoomCredentialService();
        var lifecycle = new GameLifecycleService(
            games,
            games,
            canvas,
            credentials,
            new GameLifecycleOptions(
                TimeSpan.FromMinutes(2),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMinutes(1),
                4), GameSettingsTestOptions.Create());
        var settings = new GameSettingsService(
            games,
            credentials,
            new FakePromptRepository(("en", new[] { "animals", "food" })), GameSettingsTestOptions.Create());
        var rounds = new GameRoundService(games, canvas, credentials, GameSettingsTestOptions.Create());

        CreateGameResult room = await lifecycle.CreateAsync("host");
        await lifecycle.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
        JoinCredentialResult guest = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "guest");
        await lifecycle.JoinAsync(room.GameHash, guest.RoomAccessToken!, "guest-connection");

        Assert.Equal(
            GameSettingsCommandStatus.Applied,
            (await settings.SetDrawingTimeAsync(room.GameHash, room.RoomAccessToken, 45)).Status);
        Assert.Equal(
            GameSettingsCommandStatus.Applied,
            (await settings.SetRoundsCountAsync(room.GameHash, room.RoomAccessToken, 3)).Status);
        Assert.Equal(
            GameSettingsCommandStatus.Applied,
            (await settings.SetHintSpeedAsync(room.GameHash, room.RoomAccessToken, "fast")).Status);
        Assert.Equal(
            GameSettingsCommandStatus.Applied,
            (await settings.SetHintPercentageAsync(room.GameHash, room.RoomAccessToken, 75)).Status);
        Assert.Equal(
            GameSettingsCommandStatus.Applied,
            (await settings.SetPromptCategoriesAsync(
                room.GameHash,
                room.RoomAccessToken,
                new[] { "food" })).Status);

        GameSettings visibleSettings = (await settings.GetAsync(
            room.GameHash,
            guest.RoomAccessToken!))!;

        Assert.Equal(45, visibleSettings.DrawingTimeSeconds);
        Assert.Equal(3, visibleSettings.RoundsCount);
        Assert.Equal("fast", visibleSettings.HintSpeed);
        Assert.Equal(75, visibleSettings.HintPercentage);
        Assert.Equal(new[] { "food" }, visibleSettings.PromptCategories);

        StartSessionResult started = await rounds.StartAuthorizedSessionAsync(
            room.GameHash,
            room.RoomAccessToken,
            visibleSettings);
        GameSettingsCommandResult afterStart = await settings.SetHintSpeedAsync(
            room.GameHash,
            room.RoomAccessToken,
            "off");

        Assert.Equal(RoundCommandStatus.Applied, started.Status);
        Assert.Equal(GameSettingsCommandStatus.GameAlreadyStarted, afterStart.Status);
    }
}
