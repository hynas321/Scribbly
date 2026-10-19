namespace Scribbly.Application.Features.Rounds;

public enum RoundCommandStatus
{
    Applied,
    GameNotFound,
    TooFewPlayers,
    AlreadyRunning,
    GameFinished,
    NoDrawingPlayers,
    InvalidDrawingPlayer,
    InvalidHostCredential,
    InvalidSettings,
    WaitingForPlayers
}
