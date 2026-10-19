using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Scribbly.Application.GameSessions;

namespace Scribbly.Infrastructure.Redis;

public class RedisGameSessionCancellationBus : IGameSessionCancellationBus
{
    private const string ReadVersionScript = """
        local version = redis.call('GET', KEYS[1])
        if version then
            redis.call('PEXPIRE', KEYS[1], ARGV[1])
            return version
        end
        return '0'
        """;

    private const string PublishScript = """
        local version = redis.call('INCR', KEYS[1])
        redis.call('PEXPIRE', KEYS[1], ARGV[1])
        return redis.call('PUBLISH', ARGV[2], tostring(version))
        """;

    private readonly IDatabase _database;
    private readonly ISubscriber _subscriber;
    private readonly string _keyPrefix;
    private readonly TimeSpan _versionTtl;

    public RedisGameSessionCancellationBus(
        IConnectionMultiplexer connection,
        IOptions<RedisOptions> options)
    {
        _database = connection.GetDatabase();
        _subscriber = connection.GetSubscriber();
        _keyPrefix = options.Value.KeyPrefix;
        _versionTtl = TimeSpan.FromHours(Math.Max(1, options.Value.GameTtlHours));
    }

    public async Task<long> GetCancellationVersionAsync(
        string gameHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameHash);
        cancellationToken.ThrowIfCancellationRequested();
        RedisResult value = await _database.ScriptEvaluateAsync(ReadVersionScript,
            new RedisKey[] { CancellationVersionKey(gameHash) },
            new RedisValue[] { (long)_versionTtl.TotalMilliseconds });
        return (long)value;
    }

    public async Task PublishAsync(
        string gameHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameHash);
        cancellationToken.ThrowIfCancellationRequested();
        await _database.ScriptEvaluateAsync(PublishScript,
            new RedisKey[] { CancellationVersionKey(gameHash) },
            new RedisValue[] { (long)_versionTtl.TotalMilliseconds, CancellationChannel(gameHash).ToString() });
    }

    public async Task<IAsyncDisposable> SubscribeAsync(
        string gameHash,
        Func<Task> onCancellation,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameHash);
        ArgumentNullException.ThrowIfNull(onCancellation);
        cancellationToken.ThrowIfCancellationRequested();
        ChannelMessageQueue queue = await _subscriber.SubscribeAsync(CancellationChannel(gameHash));
        Func<ChannelMessage, Task> handler = _ => onCancellation();
        queue.OnMessage(handler);
        return new RedisCancellationSubscription(queue);
    }

    private RedisChannel CancellationChannel(string gameHash) =>
        RedisChannel.Literal($"{_keyPrefix}:session-cancel:{gameHash}");

    private RedisKey CancellationVersionKey(string gameHash) =>
        $"{_keyPrefix}:game:{gameHash}:session-cancel-version";

    private class RedisCancellationSubscription : IAsyncDisposable
    {
        private ChannelMessageQueue? _queue;

        public RedisCancellationSubscription(ChannelMessageQueue queue)
        {
            _queue = queue;
        }

        public async ValueTask DisposeAsync()
        {
            ChannelMessageQueue? queue = Interlocked.Exchange(ref _queue, null);
            if (queue is not null)
            {
                await queue.UnsubscribeAsync();
            }
        }
    }
}
