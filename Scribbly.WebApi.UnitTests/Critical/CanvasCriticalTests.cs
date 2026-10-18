using Scribbly.Application.Features.Canvas;
using Scribbly.Application.Features.Canvas.Configuration;
using Scribbly.Application.Features.Games;
using Scribbly.Application.Security;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.UnitTests.Critical;

public class CanvasCriticalTests
{
    [Fact]
    public async Task SameChunkRetry_IsIdempotentAndSnapshotContainsOneCopy()
    {
        var games = new InMemoryGameStateStore();
        var canvas = new InMemoryCanvasStateStore();
        var credentials = new RoomCredentialService();
        var lifecycle = new GameLifecycleService(games, games, canvas, credentials,
            new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 12), GameSettingsTestOptions.Create());
        CreateGameResult room = await lifecycle.CreateAsync("host");
        await lifecycle.JoinAsync(room.GameHash, room.RoomAccessToken, "c1");
        string hash = credentials.ComputeCredentialHash(room.RoomAccessToken);
        await games.UpdateAsync(room.GameHash, game =>
        {
            game.GameState.IsGameStarted = true;
            game.GameState.DrawingCredentialHash = hash;
            return 0;
        });
        var service = new GameCanvasService(games, canvas, credentials, CanvasTestOptions.Create());
        var chunk = new CanvasStrokeChunk(1, "stroke-1", 0, "#000000", 5, new ushort[] { 1, 2, 3, 4 });
        CanvasCommandResult first = await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, chunk);
        CanvasCommandResult retry = await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, chunk);
        CanvasSnapshot snapshot = (await service.GetSnapshotAsync(room.GameHash, room.RoomAccessToken))!;
        Assert.Equal(CanvasCommandStatus.Applied, first.Status);
        Assert.Equal(CanvasCommandStatus.Duplicate, retry.Status);
        Assert.Single(snapshot.Strokes);
        Assert.Equal(new ushort[] { 1, 2, 3, 4 }, snapshot.Strokes.Single().Coordinates);
    }

    [Fact]
    public async Task OversizedChunk_IsRejectedBeforeStorage()
    {
        var games = new InMemoryGameStateStore();
        var canvas = new InMemoryCanvasStateStore();
        var credentials = new RoomCredentialService();
        var canvasOptions = CanvasTestOptions.Create();
        var service = new GameCanvasService(games, canvas, credentials, canvasOptions);
        ushort[] coordinates = Enumerable.Repeat((ushort)1, (canvasOptions.MaxPointsPerChunk + 1) * 2).ToArray();
        CanvasCommandResult result = await service.AppendChunkAsync("missing", "token", new CanvasStrokeChunk(1, "s", 0, "#000000", 5, coordinates));
        Assert.Equal(CanvasCommandStatus.InvalidStroke, result.Status);
    }
}
