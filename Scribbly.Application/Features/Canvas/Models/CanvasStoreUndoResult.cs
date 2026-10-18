namespace Scribbly.Application.Features.Canvas;

public record CanvasStoreUndoResult(bool Applied, string? RemovedStrokeId = null, long Revision = 0);
