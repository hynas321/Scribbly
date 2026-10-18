using Scribbly.Domain.Entities;

namespace Scribbly.Application.GameSessions;

public interface IDistributedGameSessionCoordinator
{
    Task<GameSessionStartResult> TryStartAuthorizedAsync(
        string gameHash,
        string roomAccessToken,
        GameSettings settings,
        CancellationToken cancellationToken = default);
    Task<GameSessionStartResult> TryStartAsync(
        string gameHash,
        GameSettings settings,
        CancellationToken cancellationToken = default);
    Task RequestCancellationAsync(string gameHash, CancellationToken cancellationToken = default);
}
