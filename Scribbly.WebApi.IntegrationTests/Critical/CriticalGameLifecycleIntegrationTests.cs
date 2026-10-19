using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Features.Accounts;
using Scribbly.Application.Features.Games;
using Scribbly.Application.Features.Rounds;
using Scribbly.Application.Security;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.IntegrationTests.Critical;

public class CriticalGameLifecycleIntegrationTests
{
    [Fact]
    public async Task FinishedGame_CapturesFinalScoresBeforeRoomCleanup()
    {
        var games = new InMemoryGameStateStore();
        var canvas = new InMemoryCanvasStateStore();
        var credentials = new RoomCredentialService();
        var lifecycle = new GameLifecycleService(games, games, canvas, credentials,
            new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 4), GameSettingsTestOptions.Create());
        var results = new InMemoryGameResultRepository();
        var rounds = new GameRoundService(games, canvas, credentials, GameSettingsTestOptions.Create());

        CreateGameResult room = await lifecycle.CreateAsync("host");
        await lifecycle.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
        JoinCredentialResult guestCredential = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "guest");
        await lifecycle.JoinAsync(room.GameHash, guestCredential.RoomAccessToken!, "guest-connection");

        Assert.Equal(RoundCommandStatus.Applied,
            (await rounds.StartAuthorizedSessionAsync(room.GameHash, room.RoomAccessToken,
                new GameSettings { DrawingTimeSeconds = 30, RoundsCount = 1, PromptLanguage = "en" })).Status);
        AdvanceRoundResult finished = await rounds.AdvanceRoundAsync(room.GameHash);

        Assert.True(finished.GameFinished);
        Assert.True(await new GameResultService(games, results).CaptureAsync(room.GameHash, default));

        Assert.Equal(new[] { "host", "guest" }, results.Scores.Select(score => score.Username));
        Assert.True((await games.GetSnapshotAsync(room.GameHash))!.ResultsCaptured);

        Assert.True(await lifecycle.DeleteAsync(room.GameHash));
        Assert.Null(await games.GetSnapshotAsync(room.GameHash));
    }

    [Fact]
    public async Task HostLeavesLobby_PromotesGuestAndAllowsTheRoomToContinue()
    {
        var games = new InMemoryGameStateStore();
        var canvas = new InMemoryCanvasStateStore();
        var credentials = new RoomCredentialService();
        var lifecycle = new GameLifecycleService(games, games, canvas, credentials,
            new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 4), GameSettingsTestOptions.Create());
        var rounds = new GameRoundService(games, canvas, credentials, GameSettingsTestOptions.Create());

        CreateGameResult room = await lifecycle.CreateAsync("host");
        await lifecycle.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
        JoinCredentialResult guestCredential = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "guest");
        await lifecycle.JoinAsync(room.GameHash, guestCredential.RoomAccessToken!, "guest-connection");

        LeaveGameResult left = await lifecycle.LeaveAsync(room.GameHash, room.RoomAccessToken);

        Assert.Equal(LeaveGameStatus.Left, left.Status);
        Assert.Equal("guest", left.NewHostUsername);
        Assert.True(await lifecycle.ExistsAsync(room.GameHash));

        JoinCredentialResult thirdCredential = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "third");
        await lifecycle.JoinAsync(room.GameHash, thirdCredential.RoomAccessToken!, "third-connection");
        StartSessionResult started = await rounds.StartAuthorizedSessionAsync(room.GameHash,
            guestCredential.RoomAccessToken!,
            new GameSettings { DrawingTimeSeconds = 30, RoundsCount = 1, PromptLanguage = "en" });

        Assert.Equal(RoundCommandStatus.Applied, started.Status);
    }

    [Fact]
    public async Task RoomCredentialFromAnotherRoom_CannotJoinStartOrKick()
    {
        var games = new InMemoryGameStateStore();
        var canvas = new InMemoryCanvasStateStore();
        var credentials = new RoomCredentialService();
        var lifecycle = new GameLifecycleService(games, games, canvas, credentials,
            new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 4), GameSettingsTestOptions.Create());
        var rounds = new GameRoundService(games, canvas, credentials, GameSettingsTestOptions.Create());

        CreateGameResult firstRoom = await lifecycle.CreateAsync("first-host");
        await lifecycle.JoinAsync(firstRoom.GameHash, firstRoom.RoomAccessToken, "first-host-connection");
        CreateGameResult secondRoom = await lifecycle.CreateAsync("second-host");

        JoinGameResult crossRoomJoin = await lifecycle.JoinAsync(
            firstRoom.GameHash, secondRoom.RoomAccessToken, "intruder-connection");
        StartSessionResult crossRoomStart = await rounds.StartAuthorizedSessionAsync(
            firstRoom.GameHash, secondRoom.RoomAccessToken,
            new GameSettings { DrawingTimeSeconds = 30, RoundsCount = 1, PromptLanguage = "en" });
        KickPlayerResult crossRoomKick = await lifecycle.KickAsync(
            firstRoom.GameHash, secondRoom.RoomAccessToken, "first-host");

        Assert.Equal(JoinGameStatus.InvalidCredential, crossRoomJoin.Status);
        Assert.Equal(RoundCommandStatus.InvalidHostCredential, crossRoomStart.Status);
        Assert.Equal(KickPlayerStatus.Unauthorized, crossRoomKick.Status);
    }

    [Fact]
    public async Task KickedPlayer_CannotReconnectOrJoinAgain()
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

        CreateGameResult room = await lifecycle.CreateAsync("host");
        await lifecycle.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
        JoinCredentialResult guest = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "guest");
        await lifecycle.JoinAsync(room.GameHash, guest.RoomAccessToken!, "guest-connection");

        KickPlayerResult kicked = await lifecycle.KickAsync(
            room.GameHash,
            room.RoomAccessToken,
            "guest");
        JoinGameResult reconnect = await lifecycle.ReconnectAsync(
            room.GameHash,
            guest.RoomAccessToken!,
            "guest-reconnected");
        JoinGameResult rejoin = await lifecycle.JoinAsync(
            room.GameHash,
            guest.RoomAccessToken!,
            "guest-rejoined");

        Assert.Equal(KickPlayerStatus.Applied, kicked.Status);
        Assert.Equal(JoinGameStatus.InvalidCredential, reconnect.Status);
        Assert.Equal(JoinGameStatus.InvalidCredential, rejoin.Status);
    }

    private sealed class InMemoryGameResultRepository : IGameResultRepository
    {
        private readonly Dictionary<string, FinalPlayerScore[]> _scores = new(StringComparer.Ordinal);

        public IReadOnlyList<FinalPlayerScore> Scores => _scores.Values.Single();

        public Task SaveAsync(string gameHash, IReadOnlyCollection<FinalPlayerScore> scores,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _scores[gameHash] = scores.ToArray();
            return Task.CompletedTask;
        }

        public Task<int?> GetScoreAsync(string gameHash, string credentialHash,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int? score = _scores.TryGetValue(gameHash, out FinalPlayerScore[]? scores)
                ? scores.FirstOrDefault(item => item.CredentialHash == credentialHash)?.Score
                : null;
            return Task.FromResult(score);
        }

        public Task<IReadOnlyList<FinalPlayerScore>> GetScoresAsync(string gameHash,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<FinalPlayerScore> scores = _scores.TryGetValue(gameHash, out FinalPlayerScore[]? saved)
                ? saved
                : Array.Empty<FinalPlayerScore>();
            return Task.FromResult(scores);
        }
    }
}
