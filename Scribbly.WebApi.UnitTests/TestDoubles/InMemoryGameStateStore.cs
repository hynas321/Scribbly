using System.Collections.Concurrent;
using System.Text.Json;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Persistence;
using Scribbly.Domain.Entities;

namespace Scribbly.WebApi.UnitTests.TestDoubles;

public class InMemoryGameStateStore : IGameStateStore, IGameConnectionIndex
{
    private readonly ConcurrentDictionary<string, string> _games = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly Dictionary<string, string> _connectionRooms = new();
    private readonly object _connectionIndexSync = new();
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        IncludeFields = true
    };

    public Task<bool> CreateAsync(
        string gameHash,
        Game game,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        game.GameHash = gameHash;
        bool created = _games.TryAdd(gameHash, Serialize(game));

        if (created)
        {
            SynchronizeConnectionIndex(gameHash, game);
        }

        return Task.FromResult(created);
    }

    public Task<Game?> GetSnapshotAsync(
        string gameHash,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(
            _games.TryGetValue(gameHash, out string? payload) ? Deserialize(payload) : null);
    }

    public Task<GameStoreResult<T>> UpdateAsync<T>(
        string gameHash,
        Func<Game, T> update,
        CancellationToken cancellationToken = default) =>
        MutateAsync(gameHash, game => GameMutation<T>.Persist(update(game)), cancellationToken);

    public async Task<GameStoreResult<T>> MutateAsync<T>(
        string gameHash,
        Func<Game, GameMutation<T>> mutation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mutation);
        SemaphoreSlim gate = _locks.GetOrAdd(gameHash, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);

        try
        {
            if (!_games.TryGetValue(gameHash, out string? payload))
            {
                return GameStoreResult<T>.Missing();
            }

            Game game = Deserialize(payload);
            GameMutation<T> result = mutation(game);

            if (result.Action == GameMutationAction.Delete)
            {
                _games.TryRemove(gameHash, out _);
                SynchronizeConnectionIndex(gameHash, null);
            }
            else if (result.Action == GameMutationAction.Persist)
            {
                game.Version++;
                _games[gameHash] = Serialize(game);
                SynchronizeConnectionIndex(gameHash, game);
            }

            return GameStoreResult<T>.Success(result.Value);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<bool> DeleteAsync(
        string gameHash,
        CancellationToken cancellationToken = default)
    {
        SemaphoreSlim gate = _locks.GetOrAdd(gameHash, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            bool removed = _games.TryRemove(gameHash, out _);

            if (removed)
            {
                SynchronizeConnectionIndex(gameHash, null);
            }

            return removed;
        }
        finally
        {
            gate.Release();
        }
    }

    public Task<IReadOnlyCollection<Game>> GetAllSnapshotsAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyCollection<Game> snapshots = _games.Values.Select(Deserialize).ToArray();
        return Task.FromResult(snapshots);
    }

    public Task<string?> FindGameHashAsync(
        string connectionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_connectionIndexSync)
        {
            return Task.FromResult(
                _connectionRooms.TryGetValue(connectionId, out string? gameHash)
                    ? gameHash
                    : null);
        }
    }

    public Task RemoveAsync(
        string connectionId,
        string expectedGameHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedGameHash);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_connectionIndexSync)
        {
            if (_connectionRooms.TryGetValue(connectionId, out string? currentGameHash) &&
                currentGameHash == expectedGameHash)
            {
                _connectionRooms.Remove(connectionId);
            }
        }

        return Task.CompletedTask;
    }

    private void SynchronizeConnectionIndex(string gameHash, Game? game)
    {
        HashSet<string> currentConnections = game?.GameState.Players
            .Select(player => player.ConnectionId)
            .Where(connectionId => !string.IsNullOrWhiteSpace(connectionId))
            .ToHashSet(StringComparer.Ordinal)
            ?? new HashSet<string>(StringComparer.Ordinal);

        lock (_connectionIndexSync)
        {
            string[] removedConnections = _connectionRooms
                .Where(pair => pair.Value == gameHash && !currentConnections.Contains(pair.Key))
                .Select(pair => pair.Key)
                .ToArray();

            foreach (string connectionId in removedConnections)
            {
                _connectionRooms.Remove(connectionId);
            }

            foreach (string connectionId in currentConnections)
            {
                _connectionRooms[connectionId] = gameHash;
            }
        }
    }

    private string Serialize(Game game) => JsonSerializer.Serialize(game, _jsonOptions);

    private Game Deserialize(string payload) =>
        JsonSerializer.Deserialize<Game>(payload, _jsonOptions)
        ?? throw new InvalidOperationException("Stored game contains invalid JSON.");
}
