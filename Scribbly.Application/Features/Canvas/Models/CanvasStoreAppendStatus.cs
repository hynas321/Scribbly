namespace Scribbly.Application.Features.Canvas;

public enum CanvasStoreAppendStatus
{
    Applied,
    Duplicate,
    WrongEpoch,
    OutOfOrder,
    LimitExceeded,
    InvalidStroke
}
