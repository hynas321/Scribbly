namespace Scribbly.Application.Features.Games;

public enum JoinCredentialStatus
{
    Issued,
    GameNotFound,
    GameAlreadyStarted,
    RoomFull,
    DuplicateUsername
}
