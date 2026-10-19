using Scribbly.Application.Features.Games;
using Scribbly.Application.Security;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.IntegrationTests.Critical;

public class CapacityReconnectIntegrationTests
{
    [Fact]
    public async Task CapacityReservationAndReconnect_WorkAcrossLifecycleOperations()
    {
        var games = new InMemoryGameStateStore();
        var canvas = new InMemoryCanvasStateStore();
        var credentials = new RoomCredentialService();
        var lifecycle = new GameLifecycleService(games, games, canvas, credentials,
            new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 2), GameSettingsTestOptions.Create());

        CreateGameResult room = await lifecycle.CreateAsync("host");
        await lifecycle.JoinAsync(room.GameHash, room.RoomAccessToken, "host-1");
        JoinCredentialResult guest = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "guest");
        JoinCredentialResult full = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "third");
        await lifecycle.JoinAsync(room.GameHash, guest.RoomAccessToken!, "guest-1");
        await lifecycle.RemoveByConnectionIdAsync("guest-1");
        JoinGameResult resumed = await lifecycle.ReconnectAsync(room.GameHash, guest.RoomAccessToken!, "guest-2");

        Assert.Equal(JoinCredentialStatus.RoomFull, full.Status);
        Assert.Equal(JoinGameStatus.Resumed, resumed.Status);
        Assert.Equal(2, resumed.Snapshot!.PlayerCount);
    }
}
