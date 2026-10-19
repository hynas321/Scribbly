using Scribbly.Application.Features.Canvas;
using Scribbly.Application.Features.Canvas.Configuration;
using Scribbly.Application.Features.Games;
using Scribbly.Application.Security;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.UnitTests.Canvas;

public class GameCanvasServiceTests
{
    private static async Task<(GameCanvasService Service, InMemoryGameStateStore Games, CreateGameResult Room)> CreateDrawingRoom()
    {
        var games = new InMemoryGameStateStore();
        var canvasOptions = CanvasTestOptions.Create();
        var canvas = new InMemoryCanvasStateStore(canvasOptions);
        var credentials = new RoomCredentialService();
        var lifecycle = new GameLifecycleService(games, games, canvas, credentials,
            new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 4), GameSettingsTestOptions.Create());
        CreateGameResult room = await lifecycle.CreateAsync("host");
        await lifecycle.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
        await games.UpdateAsync(room.GameHash, game =>
        {
            game.GameState.IsGameStarted = true;
            game.GameState.DrawingCredentialHash = credentials.ComputeCredentialHash(room.RoomAccessToken);
            return 0;
        });
        return (new GameCanvasService(games, canvas, credentials, canvasOptions), games, room);
    }

    private static CanvasStrokeChunk Chunk(string strokeId, int sequence, params ushort[] coordinates) =>
        new(1, strokeId, sequence, "#000000", 5, coordinates);

    [Fact]
    public async Task AppendedBatches_AppearInHistoryInOrder()
    {
        var (service, _, room) = await CreateDrawingRoom();

        await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, Chunk("stroke-a", 0, 1, 2));
        await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, Chunk("stroke-b", 0, 3, 4, 5, 6));
        CanvasSnapshot snapshot = (await service.GetSnapshotAsync(room.GameHash, room.RoomAccessToken))!;

        Assert.Equal(new[] { "stroke-a", "stroke-b" }, snapshot.Strokes.Select(s => s.StrokeId).ToArray());
        Assert.Equal(new ushort[] { 1, 2 }, snapshot.Strokes.First().Coordinates);
        Assert.Equal(new ushort[] { 3, 4, 5, 6 }, snapshot.Strokes.Last().Coordinates);
        Assert.Equal(2, snapshot.Revision);
    }

    [Fact]
    public async Task ClearAsync_EmptiesHistoryAndBumpsEpoch()
    {
        var (service, _, room) = await CreateDrawingRoom();
        await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, Chunk("stroke-a", 0, 1, 2));

        CanvasCommandResult cleared = await service.ClearAsync(room.GameHash, room.RoomAccessToken);
        CanvasSnapshot snapshot = (await service.GetSnapshotAsync(room.GameHash, room.RoomAccessToken))!;
        CanvasCommandResult staleChunk = await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, Chunk("stroke-b", 0, 9, 9));

        Assert.Equal(CanvasCommandStatus.Applied, cleared.Status);
        Assert.Equal(2, cleared.Epoch);
        Assert.Empty(snapshot.Strokes);
        Assert.Equal(2, snapshot.Epoch);
        Assert.Equal(CanvasCommandStatus.WrongEpoch, staleChunk.Status);
    }

    [Fact]
    public async Task OutOfOrderChunk_IsRejectedAndNotStored()
    {
        var (service, _, room) = await CreateDrawingRoom();

        CanvasCommandResult result = await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, Chunk("stroke-a", 1, 1, 2));
        CanvasSnapshot snapshot = (await service.GetSnapshotAsync(room.GameHash, room.RoomAccessToken))!;

        Assert.Equal(CanvasCommandStatus.OutOfOrder, result.Status);
        Assert.Empty(snapshot.Strokes);
    }

    [Fact]
    public async Task NonDrawingPlayer_CannotAppendStrokes()
    {
        var (service, games, room) = await CreateDrawingRoom();
        await games.UpdateAsync(room.GameHash, game =>
        {
            game.GameState.DrawingCredentialHash = "some-other-player";
            return 0;
        });

        CanvasCommandResult result = await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, Chunk("stroke-a", 0, 1, 2));

        Assert.Equal(CanvasCommandStatus.NotAuthorized, result.Status);
    }

    [Fact]
    public async Task StrokeLimit_ExceededBeyondMaxStrokes()
    {
        var (service, _, room) = await CreateDrawingRoom();

        for (int i = 0; i < CanvasTestOptions.Create().MaxStrokes; i++)
        {
            CanvasCommandResult applied = await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, Chunk($"stroke-{i}", 0, 1, 2));
            Assert.Equal(CanvasCommandStatus.Applied, applied.Status);
        }
        CanvasCommandResult overflow = await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, Chunk("stroke-overflow", 0, 1, 2));

        Assert.Equal(CanvasCommandStatus.LimitExceeded, overflow.Status);
    }
}
