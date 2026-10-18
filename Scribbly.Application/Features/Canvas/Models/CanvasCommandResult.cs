namespace Scribbly.Application.Features.Canvas;

public record CanvasCommandResult(
    CanvasCommandStatus Status,
    string? RemovedStrokeId = null,
    long? Epoch = null,
    long? Revision = null);
