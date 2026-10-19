namespace Scribbly.Application.GameSessions;

public interface IGameSessionRunner
{
    Task RunAsync(string gameHash, CancellationToken cancellationToken);
}
