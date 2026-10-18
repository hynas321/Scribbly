namespace Scribbly.Application.Features.Games;

public enum JoinGameStatus
{
    Joined,
    Resumed,
    GameNotFound,
    InvalidCredential,
    ResumeExpired,
    GameAlreadyStarted
}
