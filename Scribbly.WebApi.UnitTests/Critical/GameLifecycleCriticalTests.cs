using Scribbly.Application.Features.Games;
using Scribbly.Application.Security;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.UnitTests.Critical;

public class GameLifecycleCriticalTests
{
    private static (GameLifecycleService Service, InMemoryGameStateStore Games) Create(int maxPlayers = 3)
    {
        var games = new InMemoryGameStateStore();
        var canvas = new InMemoryCanvasStateStore();
        var credentials = new RoomCredentialService();
        var options = new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), maxPlayers);
        return (new GameLifecycleService(games, games, canvas, credentials, options, GameSettingsTestOptions.Create()), games);
    }

    [Fact]
    public async Task PendingCredentials_ReserveUsernameAndCapacityAtomically()
    {
        var (service, _) = Create(maxPlayers: 3);
        CreateGameResult room = await service.CreateAsync("host");
        JoinCredentialResult first = await service.IssueJoinCredentialAsync(room.GameHash, "alice");
        JoinCredentialResult duplicate = await service.IssueJoinCredentialAsync(room.GameHash, "Alice");
        JoinCredentialResult second = await service.IssueJoinCredentialAsync(room.GameHash, "bob");
        JoinCredentialResult full = await service.IssueJoinCredentialAsync(room.GameHash, "carol");
        Assert.Equal(JoinCredentialStatus.Issued, first.Status);
        Assert.Equal(JoinCredentialStatus.DuplicateUsername, duplicate.Status);
        Assert.Equal(JoinCredentialStatus.Issued, second.Status);
        Assert.Equal(JoinCredentialStatus.RoomFull, full.Status);
    }

    [Fact]
    public async Task DisconnectThenReconnect_RestoresSamePlayerWithoutCreatingDuplicate()
    {
        var (service, _) = Create();
        CreateGameResult room = await service.CreateAsync("host");
        JoinGameResult host = await service.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
        JoinCredentialResult credential = await service.IssueJoinCredentialAsync(room.GameHash, "alice");
        JoinGameResult joined = await service.JoinAsync(room.GameHash, credential.RoomAccessToken!, "alice-1");
        await service.RemoveByConnectionIdAsync("alice-1");
        JoinGameResult resumed = await service.ReconnectAsync(room.GameHash, credential.RoomAccessToken!, "alice-2");
        Assert.Equal(JoinGameStatus.Joined, host.Status);
        Assert.Equal(JoinGameStatus.Joined, joined.Status);
        Assert.Equal(JoinGameStatus.Resumed, resumed.Status);
        Assert.Equal("alice", resumed.Snapshot!.Username);
        Assert.Equal(2, resumed.Snapshot.PlayerCount);
    }
}
