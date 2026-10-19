namespace Scribbly.Application.Features.Canvas;

public enum CanvasCommandStatus
{
    Applied,
    Duplicate,
    GameNotFound,
    NotAuthorized,
    WaitingForPlayers,
    WrongEpoch,
    OutOfOrder,
    LimitExceeded,
    InvalidStroke,
    NothingToUndo
}
