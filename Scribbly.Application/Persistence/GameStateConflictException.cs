namespace Scribbly.Application.Persistence;

public class GameStateConflictException : Exception
{
    public GameStateConflictException(string gameHash)
        : base($"Game '{gameHash}' changed before the operation could commit.") { }
}
