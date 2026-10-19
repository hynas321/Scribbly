using Scribbly.Application.Features.Games;
using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Features.Rounds;
using Scribbly.Application.Features.Settings;
using Scribbly.Application.Security;
using Scribbly.Application.Services;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.IntegrationTests.Critical;

public class KickAndCategoryFlowIntegrationTests
{
    private class Session
    {
        public required InMemoryGameStateStore Games { get; init; }
        public required RoomCredentialService Credentials { get; init; }
        public required GameLifecycleService Lifecycle { get; init; }
        public required GameRoundService Rounds { get; init; }
        public required GameSettingsService Settings { get; init; }
        public required CreateGameResult Room { get; init; }
        public required string HostHash { get; init; }
        public required string GuestHash { get; init; }
        public required string GuestToken { get; init; }

        public static async Task<Session> CreateAsync()
        {
            var games = new InMemoryGameStateStore();
            var canvas = new InMemoryCanvasStateStore();
            var credentials = new RoomCredentialService();
            var lifecycle = new GameLifecycleService(games, games, canvas, credentials,
                new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 10), GameSettingsTestOptions.Create());
            CreateGameResult room = await lifecycle.CreateAsync("host");
            await lifecycle.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
            JoinCredentialResult guest = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "alice");
            await lifecycle.JoinAsync(room.GameHash, guest.RoomAccessToken!, "alice-connection");
            JoinCredentialResult second = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "bob");
            await lifecycle.JoinAsync(room.GameHash, second.RoomAccessToken!, "bob-connection");
            return new Session
            {
                Games = games,
                Credentials = credentials,
                Lifecycle = lifecycle,
                Rounds = new GameRoundService(games, canvas, credentials, GameSettingsTestOptions.Create()),
                Settings = new GameSettingsService(games, credentials, new FakePromptRepository(("en", new[] { "food", "animals" })), GameSettingsTestOptions.Create()),
                Room = room,
                HostHash = credentials.ComputeCredentialHash(room.RoomAccessToken),
                GuestHash = credentials.ComputeCredentialHash(guest.RoomAccessToken!),
                GuestToken = guest.RoomAccessToken!
            };
        }

        public async Task StartAndActivateAsync(GamePrompt prompt, string drawerHash)
        {
            Assert.Equal(RoundCommandStatus.Applied,
                (await Rounds.StartAuthorizedSessionAsync(Room.GameHash, Room.RoomAccessToken,
                    new GameSettings { DrawingTimeSeconds = 75, RoundsCount = 2, PromptLanguage = "en" })).Status);
            await Rounds.PrepareRoundAsync(Room.GameHash);
            Assert.Equal(RoundCommandStatus.Applied, (await Rounds.StartTurnAsync(Room.GameHash, drawerHash)).Status);
            Assert.Equal(RoundCommandStatus.Applied, await Rounds.ActivatePromptAsync(Room.GameHash, drawerHash, prompt));
        }
    }

    [Fact]
    public async Task KickingCurrentDrawer_MidTurn_AllowsNextTurnToProceed()
    {
        Session session = await Session.CreateAsync();
        var prompt = new GamePrompt("apple", "en", "food", PromptKinds.Word);
        await session.StartAndActivateAsync(prompt, session.GuestHash);

        KickPlayerResult kicked = await session.Lifecycle.KickAsync(session.Room.GameHash, session.Room.RoomAccessToken, "alice");

        TimerTickResult tick = await session.Rounds.TickTimerAsync(session.Room.GameHash);
        CompleteTurnResult abandoned = await session.Rounds.CompleteTurnAsync(session.Room.GameHash, session.GuestHash);
        StartTurnResult next = await session.Rounds.StartTurnAsync(session.Room.GameHash, session.HostHash);
        Game snapshot = (await session.Games.GetSnapshotAsync(session.Room.GameHash))!;

        Assert.Equal(KickPlayerStatus.Applied, kicked.Status);
        Assert.Equal(RoundCommandStatus.Applied, tick.Status);
        Assert.Equal(RoundCommandStatus.InvalidDrawingPlayer, abandoned.Status);
        Assert.Equal(RoundCommandStatus.Applied, next.Status);
        Assert.Equal("host", next.DrawingPlayerUsername);
        Assert.Equal(2, snapshot.GameState.Players.Count);
        Assert.DoesNotContain(session.GuestHash, snapshot.GameState.DrawingPlayerCredentialHashes);
    }

    [Fact]
    public async Task PromptFlow_PassesSelectedCategoriesFromGameSettingsToProvider()
    {
        Session session = await Session.CreateAsync();
        var prompt = new GamePrompt("apple", "en", "food", PromptKinds.Word);
        var provider = new FakeRandomPromptProvider(prompt);
        var prompts = new SharedStateRandomPromptService(session.Lifecycle, provider);

        GameSettingsCommandResult selection = await session.Settings.SetPromptCategoriesAsync(
            session.Room.GameHash, session.Room.RoomAccessToken, new[] { "food" });
        Assert.Equal(RoundCommandStatus.Applied,
            (await session.Rounds.StartAuthorizedSessionAsync(session.Room.GameHash, session.Room.RoomAccessToken,
                new GameSettings { DrawingTimeSeconds = 75, RoundsCount = 2, PromptLanguage = "en" })).Status);
        await session.Rounds.PrepareRoundAsync(session.Room.GameHash);
        StartTurnResult turn = await session.Rounds.StartTurnAsync(
            session.Room.GameHash,
            session.HostHash);
        Assert.Equal(RoundCommandStatus.Applied, turn.Status);

        GamePrompt? fetched = await prompts.FetchPromptAsync(session.Room.GameHash, CancellationToken.None);
        RoundCommandStatus activated = await session.Rounds.ActivatePromptAsync(
            session.Room.GameHash,
            session.HostHash,
            fetched!);

        Assert.Equal(GameSettingsCommandStatus.Applied, selection.Status);
        Assert.NotNull(fetched);
        Assert.Equal(new[] { "food" }, provider.LastRequestedCategories);
        Assert.Equal(RoundCommandStatus.Applied, activated);
    }
}
