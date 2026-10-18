namespace Scribbly.Application.Features.Canvas;

public record CanvasStrokeChunk(
    long Epoch,
    string StrokeId,
    int Sequence,
    string Color,
    int Thickness,
    ushort[] Coordinates);
