namespace Scribbly.Application.GameSessions;

public interface IGameSessionCancellationBus
{
    Task<long> GetCancellationVersionAsync(
        string gameHash,
        CancellationToken cancellationToken = default);

    Task PublishAsync(
        string gameHash,
        CancellationToken cancellationToken = default);

    Task<IAsyncDisposable> SubscribeAsync(
        string gameHash,
        Func<Task> onCancellation,
        CancellationToken cancellationToken = default);
}
