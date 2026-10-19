namespace Scribbly.Application.Features.Canvas;

public interface IGameCanvasService
{
    Task<CanvasCommandResult> AppendChunkAsync(
        string gameHash,
        string roomAccessToken,
        CanvasStrokeChunk chunk,
        CancellationToken cancellationToken = default);

    Task<CanvasCommandResult> ClearAsync(
        string gameHash,
        string roomAccessToken,
        CancellationToken cancellationToken = default);

    Task<CanvasCommandResult> UndoAsync(
        string gameHash,
        string roomAccessToken,
        CancellationToken cancellationToken = default);

    Task<CanvasSnapshot?> GetSnapshotAsync(
        string gameHash,
        string roomAccessToken,
        CancellationToken cancellationToken = default);
}
