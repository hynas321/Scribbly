using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Scribbly.Application.Abstractions.Persistence;

namespace Scribbly.Infrastructure.Redis;

public class RedisGameConnectionIndex : IGameConnectionIndex
{
    private const string RemoveScript = """
        if redis.call('get', KEYS[1]) ~= ARGV[1] then
            return 0
        end
        redis.call('del', KEYS[1])
        redis.call('srem', KEYS[2], ARGV[2])
        return 1
        """;

    private readonly IDatabase _database;
    private readonly RedisOptions _options;

    public RedisGameConnectionIndex(
        IConnectionMultiplexer connection,
        IOptions<RedisOptions> options)
    {
        _database = connection.GetDatabase();
        _options = options.Value;
    }

    public async Task<string?> FindGameHashAsync(
        string connectionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        cancellationToken.ThrowIfCancellationRequested();
        RedisValue gameHash = await _database.StringGetAsync(ConnectionKey(connectionId));
        return gameHash.IsNullOrEmpty ? null : gameHash.ToString();
    }

    public async Task RemoveAsync(
        string connectionId,
        string expectedGameHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedGameHash);
        cancellationToken.ThrowIfCancellationRequested();
        await _database.ScriptEvaluateAsync(
            RemoveScript,
            new RedisKey[]
            {
                ConnectionKey(connectionId),
                RoomConnectionsKey(expectedGameHash)
            },
            new RedisValue[] { expectedGameHash, connectionId });
    }

    private RedisKey ConnectionKey(string connectionId) =>
        $"{_options.KeyPrefix}:connection:{connectionId}:game";

    private RedisKey RoomConnectionsKey(string gameHash) =>
        $"{_options.KeyPrefix}:game:{gameHash}:connections";
}
