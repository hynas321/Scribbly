using Scribbly.Application.Persistence;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.Abstractions.Persistence;

public interface IGameStateStore
{
    Task<bool> CreateAsync(string gameHash, Game game, CancellationToken cancellationToken = default);
    Task<Game?> GetSnapshotAsync(string gameHash, CancellationToken cancellationToken = default);
    Task<GameStoreResult<T>> UpdateAsync<T>(
        string gameHash,
        Func<Game, T> update,
        CancellationToken cancellationToken = default);
    Task<GameStoreResult<T>> MutateAsync<T>(
        string gameHash,
        Func<Game, GameMutation<T>> mutation,
        CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string gameHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Game>> GetAllSnapshotsAsync(CancellationToken cancellationToken = default);
}
