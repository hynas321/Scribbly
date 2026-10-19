using Scribbly.Application.Features.Canvas;

namespace Scribbly.Application.Abstractions.Canvas;

public interface ICanvasStateStore
{
    Task<CanvasStoreAppendResult> AppendChunkAsync(string gameHash, CanvasStrokeChunk chunk, CancellationToken cancellationToken = default);
    Task<CanvasSnapshot> GetSnapshotAsync(string gameHash, long expectedEpoch, CancellationToken cancellationToken = default);
    Task<CanvasStoreUndoResult> UndoAsync(string gameHash, long expectedEpoch, CancellationToken cancellationToken = default);
    Task ClearAsync(string gameHash, long newEpoch, CancellationToken cancellationToken = default);
    Task DeleteAsync(string gameHash, CancellationToken cancellationToken = default);
}
