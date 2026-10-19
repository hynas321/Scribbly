namespace Scribbly.Application.GameSessions;

public enum GameSessionStartResult
{
    Started,
    GameNotFound,
    TooFewPlayers,
    AlreadyRunning,
    GameFinished,
    InvalidHostCredential,
    InvalidSettings
}
