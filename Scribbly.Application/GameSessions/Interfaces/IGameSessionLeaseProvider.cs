namespace Scribbly.Application.GameSessions;

public interface IGameSessionLeaseProvider
{
    Task<bool> TryAcquireAsync(
        string gameHash,
        string ownerToken,
        CancellationToken cancellationToken = default);
    Task<bool> RenewAsync(
        string gameHash,
        string ownerToken,
        CancellationToken cancellationToken = default);
    Task<bool> ReleaseAsync(
        string gameHash,
        string ownerToken,
        CancellationToken cancellationToken = default);
}
