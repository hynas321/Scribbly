namespace Scribbly.Application.Features.Canvas;

public record CanvasStrokeSnapshot(string StrokeId, string Color, int Thickness, ushort[] Coordinates);
