namespace Scribbly.Application.GameSessions;

public class GameSessionExecutionContext
{
    private readonly AsyncLocal<Ownership?> _current = new();
    public Ownership? Current => _current.Value;

    public IDisposable Enter(string gameHash, string ownerToken)
    {
        var previous = _current.Value;
        _current.Value = new Ownership(gameHash, ownerToken);
        return new Scope(() => _current.Value = previous);
    }

    public record Ownership(string GameHash, string OwnerToken);
    private class Scope(Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }
}
