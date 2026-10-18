using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Persistence;
using Scribbly.Domain.Entities;
using Scribbly.Application.GameSessions;

namespace Scribbly.Infrastructure.Redis;

public class RedisGameStateStore : IGameStateStore
{
    private readonly IDatabase _database;
    private readonly RedisOptions _options;
    private readonly GameSessionExecutionContext? _execution;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        IncludeFields = true
    };

    public RedisGameStateStore(
        IConnectionMultiplexer connection,
        IOptions<RedisOptions> options,
        GameSessionExecutionContext? execution = null)
    {
        _database = connection.GetDatabase();
        _options = options.Value;
        _execution = execution;
    }

    public async Task<bool> CreateAsync(
        string gameHash,
        Game game,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        game.GameHash = gameHash;
        string payload = JsonSerializer.Serialize(game, _jsonOptions);
        RedisResult result = await _database.ScriptEvaluateAsync(
            RedisGameStateScripts.CreateScript,
            new RedisKey[] { GameKey(gameHash), IndexKey, RoomConnectionsKey(gameHash) },
            BuildGameArguments(payload, gameHash, game));

        return (long)result == 1;
    }

    public async Task<Game?> GetSnapshotAsync(
        string gameHash,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RedisValue payload = await _database.StringGetAsync(GameKey(gameHash));
        return payload.IsNullOrEmpty
            ? null
            : JsonSerializer.Deserialize<Game>(payload.ToString(), _jsonOptions);
    }

    public async Task<GameStoreResult<T>> UpdateAsync<T>(
        string gameHash,
        Func<Game, T> update,
        CancellationToken cancellationToken = default) =>
        await MutateAsync(
            gameHash,
            game => GameMutation<T>.Persist(update(game)),
            cancellationToken);

    public async Task<GameStoreResult<T>> MutateAsync<T>(
        string gameHash,
        Func<Game, GameMutation<T>> mutation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mutation);
        string lockToken = Guid.NewGuid().ToString("N");
        RedisKey lockKey = LockKey(gameHash);
        await AcquireLockAsync(lockKey, lockToken, cancellationToken);

        try
        {
            RedisValue payload = await _database.StringGetAsync(GameKey(gameHash));
            if (payload.IsNullOrEmpty)
            {
                return GameStoreResult<T>.Missing();
            }

            Game game = JsonSerializer.Deserialize<Game>(payload.ToString(), _jsonOptions)
                ?? throw new InvalidOperationException($"Game '{gameHash}' contains invalid JSON.");
            GameMutation<T> result = mutation(game);

            if (result.Action == GameMutationAction.Delete)
            {
                cancellationToken.ThrowIfCancellationRequested();
                RedisResult deleted = await _database.ScriptEvaluateAsync(
                    RedisGameStateScripts.DeleteScript,
                    new RedisKey[] { GameKey(gameHash), IndexKey, RoomConnectionsKey(gameHash), lockKey, SessionLeaseKey(gameHash) },
                    new RedisValue[] { gameHash, _options.KeyPrefix, lockToken, payload, SessionOwner(gameHash) });
                EnsureOwnership(deleted, gameHash);
                return GameStoreResult<T>.Success(result.Value);
            }

            if (result.Action == GameMutationAction.NoChange)
            {
                return GameStoreResult<T>.Success(result.Value);
            }

            game.Version++;
            string updatedPayload = JsonSerializer.Serialize(game, _jsonOptions);
            cancellationToken.ThrowIfCancellationRequested();
            RedisResult persisted = await _database.ScriptEvaluateAsync(
                RedisGameStateScripts.PersistScript,
                new RedisKey[] { GameKey(gameHash), RoomConnectionsKey(gameHash), lockKey, SessionLeaseKey(gameHash) },
                BuildGameArguments(updatedPayload, gameHash, game, lockToken, payload, SessionOwner(gameHash)));
            EnsureOwnership(persisted, gameHash);
            return GameStoreResult<T>.Success(result.Value);
        }
        finally
        {
            await _database.LockReleaseAsync(lockKey, lockToken);
        }
    }

    public async Task<bool> DeleteAsync(
        string gameHash,
        CancellationToken cancellationToken = default)
    {
        string lockToken = Guid.NewGuid().ToString("N");
        RedisKey lockKey = LockKey(gameHash);
        await AcquireLockAsync(lockKey, lockToken, cancellationToken);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            RedisResult result = await _database.ScriptEvaluateAsync(
                RedisGameStateScripts.DeleteScript,
                new RedisKey[] { GameKey(gameHash), IndexKey, RoomConnectionsKey(gameHash), lockKey, SessionLeaseKey(gameHash) },
                new RedisValue[] { gameHash, _options.KeyPrefix, lockToken, "", SessionOwner(gameHash) });
            EnsureOwnership(result, gameHash);
            return (long)result == 1;
        }
        finally
        {
            await _database.LockReleaseAsync(lockKey, lockToken);
        }
    }

    public async Task<IReadOnlyCollection<Game>> GetAllSnapshotsAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RedisValue[] hashes = await _database.SetMembersAsync(IndexKey);
        var games = new List<Game>(hashes.Length);

        foreach (RedisValue hash in hashes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Game? game = await GetSnapshotAsync(hash.ToString(), cancellationToken);
            if (game is null)
            {
                await _database.ScriptEvaluateAsync(
                    "if redis.call('exists', KEYS[1]) == 0 then return redis.call('srem', KEYS[2], ARGV[1]) end return 0",
                    new RedisKey[] { GameKey(hash.ToString()), IndexKey },
                    new RedisValue[] { hash });
                continue;
            }

            games.Add(game);
        }

        return games;
    }

    private async Task AcquireLockAsync(
        RedisKey lockKey,
        RedisValue lockToken,
        CancellationToken cancellationToken)
    {
        DateTimeOffset deadline = DateTimeOffset.UtcNow.AddSeconds(_options.LockTimeoutSeconds);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await _database.LockTakeAsync(lockKey, lockToken, LockLease))
            {
                return;
            }
            if (DateTimeOffset.UtcNow >= deadline)
            {
                throw new TimeoutException($"Could not acquire distributed lock '{lockKey}'.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(25), cancellationToken);
        }
    }

    private RedisKey SessionLeaseKey(string gameHash) => $"{_options.KeyPrefix}:game:{gameHash}:session-lease";
    private string SessionOwner(string gameHash)
    {
        var ownership = _execution?.Current;
        if (ownership is null)
        {
            return string.Empty;
        }
        if (ownership.GameHash != gameHash)
        {
            throw new InvalidOperationException("A runner cannot mutate another room.");
        }
        return ownership.OwnerToken;
    }

    private TimeSpan GameTtl => TimeSpan.FromHours(_options.GameTtlHours);
    private TimeSpan LockLease => TimeSpan.FromSeconds(_options.LockLeaseSeconds);
    private RedisKey IndexKey => $"{_options.KeyPrefix}:games";
    private RedisKey GameKey(string gameHash) => $"{_options.KeyPrefix}:game:{gameHash}";
    private RedisKey LockKey(string gameHash) => $"{_options.KeyPrefix}:game:{gameHash}:lock";
    private RedisKey RoomConnectionsKey(string gameHash) =>
        $"{_options.KeyPrefix}:game:{gameHash}:connections";

    private static void EnsureOwnership(RedisResult result, string gameHash)
    {
        if ((long)result < 0)
        {
            throw new GameStateConflictException(gameHash);
        }
    }

    private RedisValue[] BuildGameArguments(string payload, string gameHash, Game game,
        params RedisValue[] mutationGuard)
    {
        var arguments = new List<RedisValue>
        {
            payload,
            (long)GameTtl.TotalMilliseconds,
            gameHash,
            _options.KeyPrefix
        };
        arguments.AddRange(mutationGuard);
        arguments.AddRange(game.GameState.Players
            .Select(player => player.ConnectionId)
            .Where(connectionId => !string.IsNullOrWhiteSpace(connectionId))
            .Distinct(StringComparer.Ordinal)
            .Select(connectionId => (RedisValue)connectionId));
        return arguments.ToArray();
    }
}

