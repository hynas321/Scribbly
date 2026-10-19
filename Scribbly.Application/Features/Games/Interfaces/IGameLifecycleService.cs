using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Games;

public interface IGameLifecycleService
{
    Task<CreateGameResult> CreateAsync(string hostUsername, CancellationToken cancellationToken = default);
    Task<JoinCredentialResult> IssueJoinCredentialAsync(
        string gameHash,
        string username,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string gameHash, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string gameHash, CancellationToken cancellationToken = default);
    Task<Game?> GetSnapshotAsync(string gameHash, CancellationToken cancellationToken = default);
    Task<Player?> GetPlayerAsync(string gameHash, string roomAccessToken, CancellationToken cancellationToken = default);
    Task<bool> PlayerExistsAsync(string gameHash, string roomAccessToken, CancellationToken cancellationToken = default);
    Task<JoinGameResult> JoinAsync(
        string gameHash,
        string roomAccessToken,
        string connectionId,
        CancellationToken cancellationToken = default);
    Task<JoinGameResult> ReconnectAsync(
        string gameHash,
        string roomAccessToken,
        string connectionId,
        CancellationToken cancellationToken = default);
    Task<LeaveGameResult> LeaveAsync(
        string gameHash,
        string roomAccessToken,
        CancellationToken cancellationToken = default);
    Task<KickPlayerResult> KickAsync(
        string gameHash,
        string roomAccessToken,
        string username,
        CancellationToken cancellationToken = default);
    Task<DisconnectedPlayerResult> RemoveByConnectionIdAsync(
        string connectionId,
        CancellationToken cancellationToken = default);
    Task<ExpiredDisconnectedPlayersResult> RemoveExpiredDisconnectedPlayersAsync(
        string gameHash,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);
}
