namespace Scribbly.Application.GameSessions;

public interface IGameSessionLeaseExecutor
{
    Task<GameSessionLeaseExecutionResult> ExecuteAsync(
        string gameHash,
        Func<CancellationToken, Task> runSession,
        CancellationToken cancellationToken = default);
}
