namespace Scribbly.Application.GameSessions;

public interface IDrawingPlayerSelector
{
    string Select(IReadOnlyCollection<string> availableTokens);
}
