using Scribbly.Application.Features.Canvas;
using Scribbly.Application.Features.Games;
using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Features.Rounds;
using Scribbly.Application.Security;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.UnitTests.Lifecycle;

public class GameLifecycleKickTests
{
    private static readonly GameSettings SessionSettings = new() { DrawingTimeSeconds = 75, RoundsCount = 2, PromptLanguage = "en" };
    private static readonly GamePrompt TestPrompt = new("apple", "en", "food", PromptKinds.Word);

    private class Lobby
    {
        public required InMemoryGameStateStore Games { get; init; }
        public required InMemoryCanvasStateStore Canvas { get; init; }
        public required RoomCredentialService Credentials { get; init; }
        public required GameLifecycleService Lifecycle { get; init; }
        public required GameRoundService Rounds { get; init; }
        public required CreateGameResult Room { get; init; }
        public required string HostHash { get; init; }
        public required string GuestHash { get; init; }
        public required string GuestToken { get; init; }

        public static async Task<Lobby> CreateAsync()
        {
            var games = new InMemoryGameStateStore();
            var canvas = new InMemoryCanvasStateStore();
            var credentials = new RoomCredentialService();
            var lifecycle = new GameLifecycleService(games, games, canvas, credentials,
                new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 10), GameSettingsTestOptions.Create());
            var rounds = new GameRoundService(games, canvas, credentials, GameSettingsTestOptions.Create());
            CreateGameResult room = await lifecycle.CreateAsync("host");
            await lifecycle.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
            JoinCredentialResult guest = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "alice");
            await lifecycle.JoinAsync(room.GameHash, guest.RoomAccessToken!, "alice-connection");
            JoinCredentialResult second = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "bob");
            await lifecycle.JoinAsync(room.GameHash, second.RoomAccessToken!, "bob-connection");
            return new Lobby
            {
                Games = games,
                Canvas = canvas,
                Credentials = credentials,
                Lifecycle = lifecycle,
                Rounds = rounds,
                Room = room,
                HostHash = credentials.ComputeCredentialHash(room.RoomAccessToken),
                GuestHash = credentials.ComputeCredentialHash(guest.RoomAccessToken!),
                GuestToken = guest.RoomAccessToken!
            };
        }

        public async Task StartGameAsync()
        {
            Assert.Equal(RoundCommandStatus.Applied,
                (await Rounds.StartAuthorizedSessionAsync(Room.GameHash, Room.RoomAccessToken, SessionSettings)).Status);
            await Rounds.PrepareRoundAsync(Room.GameHash);
        }
    }

    [Fact]
    public async Task KickAsync_NonHostToken_ReturnsUnauthorized()
    {
        Lobby lobby = await Lobby.CreateAsync();

        KickPlayerResult result = await lobby.Lifecycle.KickAsync(lobby.Room.GameHash, lobby.GuestToken, "bob");

        Assert.Equal(KickPlayerStatus.Unauthorized, result.Status);
        Game snapshot = (await lobby.Games.GetSnapshotAsync(lobby.Room.GameHash))!;
        Assert.Contains(snapshot.GameState.Players, p => p.Username == "bob");
    }

    [Fact]
    public async Task KickAsync_KickingHost_ReturnsCannotKickHost()
    {
        Lobby lobby = await Lobby.CreateAsync();

        KickPlayerResult result = await lobby.Lifecycle.KickAsync(lobby.Room.GameHash, lobby.Room.RoomAccessToken, "host");

        Assert.Equal(KickPlayerStatus.CannotKickHost, result.Status);
    }

    [Fact]
    public async Task KickAsync_UnknownPlayer_ReturnsPlayerNotFound()
    {
        Lobby lobby = await Lobby.CreateAsync();

        KickPlayerResult result = await lobby.Lifecycle.KickAsync(lobby.Room.GameHash, lobby.Room.RoomAccessToken, "carol");

        Assert.Equal(KickPlayerStatus.PlayerNotFound, result.Status);
    }

    [Fact]
    public async Task KickAsync_RemovesPlayerAndFlagsHash()
    {
        Lobby lobby = await Lobby.CreateAsync();

        KickPlayerResult result = await lobby.Lifecycle.KickAsync(lobby.Room.GameHash, lobby.Room.RoomAccessToken, "ALICE");
        Game snapshot = (await lobby.Games.GetSnapshotAsync(lobby.Room.GameHash))!;

        Assert.Equal(KickPlayerStatus.Applied, result.Status);
        Assert.Equal("alice", result.Username);
        Assert.DoesNotContain(snapshot.GameState.Players, p => p.Username == "alice");
        Assert.Contains(lobby.GuestHash, snapshot.GameState.KickedCredentialHashes);
        Assert.Equal(2, snapshot.GameState.Players.Count);
    }

    [Fact]
    public async Task KickedPlayer_CannotReconnectOrJoinWithSameCredential()
    {
        Lobby lobby = await Lobby.CreateAsync();
        await lobby.Lifecycle.KickAsync(lobby.Room.GameHash, lobby.Room.RoomAccessToken, "alice");

        JoinGameResult rejoin = await lobby.Lifecycle.JoinAsync(lobby.Room.GameHash, lobby.GuestToken, "alice-2");
        JoinGameResult reconnect = await lobby.Lifecycle.ReconnectAsync(lobby.Room.GameHash, lobby.GuestToken, "alice-2");

        Assert.Equal(JoinGameStatus.InvalidCredential, rejoin.Status);
        Assert.Equal(JoinGameStatus.InvalidCredential, reconnect.Status);
    }

    [Fact]
    public async Task KickCurrentDrawer_RotatesTurnAndBumpsCanvasEpoch()
    {
        Lobby lobby = await Lobby.CreateAsync();
        await lobby.StartGameAsync();
        Assert.Equal(RoundCommandStatus.Applied,
            (await lobby.Rounds.StartTurnAsync(lobby.Room.GameHash, lobby.GuestHash)).Status);
        Assert.Equal(RoundCommandStatus.Applied,
            await lobby.Rounds.ActivatePromptAsync(lobby.Room.GameHash, lobby.GuestHash, TestPrompt));
        Game before = (await lobby.Games.GetSnapshotAsync(lobby.Room.GameHash))!;
        long canvasEpochBefore = before.GameState.CanvasEpoch;

        KickPlayerResult result = await lobby.Lifecycle.KickAsync(lobby.Room.GameHash, lobby.Room.RoomAccessToken, "alice");
        Game snapshot = (await lobby.Games.GetSnapshotAsync(lobby.Room.GameHash))!;

        Assert.Equal(KickPlayerStatus.Applied, result.Status);
        Assert.True(snapshot.GameState.CanvasEpoch > canvasEpochBefore);
        Assert.True(snapshot.Version > before.Version);
        Assert.Equal(string.Empty, snapshot.GameState.DrawingCredentialHash);
        Assert.Equal(string.Empty, snapshot.GameState.PendingDrawingCredentialHash);
        Assert.Equal(string.Empty, snapshot.GameState.DrawingPlayerUsername);
        Assert.DoesNotContain(lobby.GuestHash, snapshot.GameState.DrawingPlayerCredentialHashes);
        CanvasSnapshot canvas = await lobby.Canvas.GetSnapshotAsync(lobby.Room.GameHash, snapshot.GameState.CanvasEpoch);
        Assert.Equal(snapshot.GameState.CanvasEpoch, canvas.Epoch);

        StartTurnResult next = await lobby.Rounds.StartTurnAsync(lobby.Room.GameHash, lobby.HostHash);
        Assert.Equal(RoundCommandStatus.Applied, next.Status);
    }

    [Fact]
    public async Task KickPendingDrawer_ClearsPendingHashOnly()
    {
        Lobby lobby = await Lobby.CreateAsync();
        await lobby.StartGameAsync();
        Assert.Equal(RoundCommandStatus.Applied,
            (await lobby.Rounds.StartTurnAsync(lobby.Room.GameHash, lobby.GuestHash)).Status);
        Game before = (await lobby.Games.GetSnapshotAsync(lobby.Room.GameHash))!;

        KickPlayerResult result = await lobby.Lifecycle.KickAsync(lobby.Room.GameHash, lobby.Room.RoomAccessToken, "alice");
        Game snapshot = (await lobby.Games.GetSnapshotAsync(lobby.Room.GameHash))!;

        Assert.Equal(KickPlayerStatus.Applied, result.Status);
        Assert.Equal(0, result.CanvasEpoch);
        Assert.Equal(before.GameState.CanvasEpoch, snapshot.GameState.CanvasEpoch);
        Assert.Equal(string.Empty, snapshot.GameState.PendingDrawingCredentialHash);

        StartTurnResult next = await lobby.Rounds.StartTurnAsync(lobby.Room.GameHash, lobby.HostHash);
        Assert.Equal(RoundCommandStatus.Applied, next.Status);
    }

    [Fact]
    public async Task KickDisconnectedBlocker_UnblocksTimerTicks()
    {
        Lobby lobby = await Lobby.CreateAsync();
        await lobby.StartGameAsync();
        Assert.Equal(RoundCommandStatus.Applied,
            (await lobby.Rounds.StartTurnAsync(lobby.Room.GameHash, lobby.HostHash)).Status);
        Assert.Equal(RoundCommandStatus.Applied,
            await lobby.Rounds.ActivatePromptAsync(lobby.Room.GameHash, lobby.HostHash, TestPrompt));
        await lobby.Lifecycle.RemoveByConnectionIdAsync("alice-connection");
        Assert.Equal(RoundCommandStatus.WaitingForPlayers, (await lobby.Rounds.TickTimerAsync(lobby.Room.GameHash)).Status);

        KickPlayerResult result = await lobby.Lifecycle.KickAsync(lobby.Room.GameHash, lobby.Room.RoomAccessToken, "alice");
        TimerTickResult tick = await lobby.Rounds.TickTimerAsync(lobby.Room.GameHash);
        Game snapshot = (await lobby.Games.GetSnapshotAsync(lobby.Room.GameHash))!;

        Assert.Equal(KickPlayerStatus.Applied, result.Status);
        Assert.Empty(snapshot.GameState.DisconnectedPlayers);
        Assert.Equal(RoundCommandStatus.Applied, tick.Status);
    }

    [Fact]
    public async Task KickUntilOnePlayerRemains_FinishesGameAndPreservesFinalScore()
    {
        Lobby lobby = await Lobby.CreateAsync();
        await lobby.StartGameAsync();
        await lobby.Games.UpdateAsync(lobby.Room.GameHash, game =>
        {
            game.GameState.Players.Single(player => player.Username == "host").Score = 35;
            return 0;
        });

        KickPlayerResult first = await lobby.Lifecycle.KickAsync(lobby.Room.GameHash, lobby.Room.RoomAccessToken, "alice");
        KickPlayerResult second = await lobby.Lifecycle.KickAsync(lobby.Room.GameHash, lobby.Room.RoomAccessToken, "bob");
        Game snapshot = (await lobby.Games.GetSnapshotAsync(lobby.Room.GameHash))!;

        Assert.False(first.GameFinished);
        Assert.True(second.GameFinished);
        Assert.False(snapshot.GameState.IsGameStarted);
        Assert.Equal(snapshot.GameSettings.RoundsCount + 1, snapshot.GameState.CurrentRound);
        Assert.Single(snapshot.FinalScores);
        Assert.Equal("host", snapshot.FinalScores[0].Username);
        Assert.Equal(35, snapshot.FinalScores[0].Score);
    }
}
