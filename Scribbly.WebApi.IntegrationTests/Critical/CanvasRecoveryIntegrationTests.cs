using Scribbly.Application.Features.Canvas;
using Scribbly.Application.Features.Canvas.Configuration;
using Scribbly.Application.Features.Games;
using Scribbly.Application.Security;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.IntegrationTests.Critical;

public class CanvasRecoveryIntegrationTests
{
    [Fact]
    public async Task ClearCanvas_StartsNewEpochAndRejectsStaleChunks()
    {
        (GameCanvasService service, CreateGameResult room) = await CreateDrawingSession();
        var stroke = new CanvasStrokeChunk(
            1,
            "stroke",
            0,
            "#112233",
            4,
            new ushort[] { 10, 20, 30, 40 });

        Assert.Equal(
            CanvasCommandStatus.Applied,
            (await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, stroke)).Status);

        CanvasCommandResult cleared = await service.ClearAsync(room.GameHash, room.RoomAccessToken);
        CanvasCommandResult stale = await service.AppendChunkAsync(
            room.GameHash,
            room.RoomAccessToken,
            stroke);
        CanvasSnapshot snapshot = (await service.GetSnapshotAsync(
            room.GameHash,
            room.RoomAccessToken))!;

        Assert.Equal(CanvasCommandStatus.Applied, cleared.Status);
        Assert.Equal(2, cleared.Epoch);
        Assert.Equal(CanvasCommandStatus.WrongEpoch, stale.Status);
        Assert.Equal(2, snapshot.Epoch);
        Assert.Empty(snapshot.Strokes);
    }

    [Fact]
    public async Task Undo_RemovesOnlyTheLatestStroke()
    {
        (GameCanvasService service, CreateGameResult room) = await CreateDrawingSession();
        var firstStroke = new CanvasStrokeChunk(
            1,
            "first",
            0,
            "#112233",
            4,
            new ushort[] { 10, 20 });
        var secondStroke = new CanvasStrokeChunk(
            1,
            "second",
            0,
            "#445566",
            4,
            new ushort[] { 30, 40 });

        await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, firstStroke);
        await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, secondStroke);

        CanvasCommandResult undone = await service.UndoAsync(room.GameHash, room.RoomAccessToken);
        CanvasSnapshot snapshot = (await service.GetSnapshotAsync(
            room.GameHash,
            room.RoomAccessToken))!;

        Assert.Equal(CanvasCommandStatus.Applied, undone.Status);
        Assert.Equal("second", undone.RemovedStrokeId);
        Assert.Single(snapshot.Strokes);
        Assert.Equal("first", snapshot.Strokes.Single().StrokeId);
    }

    [Fact]
    public async Task RetryThenSnapshot_PreservesSingleStrokeAndRevision()
    {
        (GameCanvasService service, CreateGameResult room) = await CreateDrawingSession();
        var first = new CanvasStrokeChunk(1, "stroke", 0, "#112233", 4, new ushort[] { 10, 20, 30, 40 });
        var second = new CanvasStrokeChunk(1, "stroke", 1, "#112233", 4, new ushort[] { 50, 60 });
        await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, first);
        CanvasCommandResult duplicate = await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, first);
        await service.AppendChunkAsync(room.GameHash, room.RoomAccessToken, second);
        CanvasSnapshot snapshot = (await service.GetSnapshotAsync(room.GameHash, room.RoomAccessToken))!;
        Assert.Equal(CanvasCommandStatus.Duplicate, duplicate.Status);
        Assert.Equal(new ushort[] { 10, 20, 30, 40, 50, 60 }, snapshot.Strokes.Single().Coordinates);
        Assert.Equal(2, snapshot.Revision);
    }

    private static async Task<(GameCanvasService Service, CreateGameResult Room)> CreateDrawingSession()
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
                12), GameSettingsTestOptions.Create());

        CreateGameResult room = await lifecycle.CreateAsync("host");
        await lifecycle.JoinAsync(room.GameHash, room.RoomAccessToken, "host");
        string hash = credentials.ComputeCredentialHash(room.RoomAccessToken);

        await games.UpdateAsync(room.GameHash, game =>
        {
            game.GameState.IsGameStarted = true;
            game.GameState.DrawingCredentialHash = hash;
            return 0;
        });

        return (new GameCanvasService(games, canvas, credentials, CanvasTestOptions.Create()), room);
    }
}
