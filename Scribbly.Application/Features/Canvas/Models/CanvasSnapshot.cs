namespace Scribbly.Application.Features.Canvas;

public record CanvasSnapshot(long Epoch, long Revision, IReadOnlyCollection<CanvasStrokeSnapshot> Strokes);
