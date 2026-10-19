using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Scribbly.Application.Abstractions.Sessions;
using Scribbly.Infrastructure.Redis;
using StackExchange.Redis;

namespace Scribbly.Infrastructure.Sessions;

public class RedisAccountSessionStore : IAccountSessionStore
{
    private const string CreateScript = """
        local previous = redis.call('get', KEYS[2])
        if previous then redis.call('del', ARGV[1] .. ':account-session:' .. previous) end
        redis.call('set', KEYS[1], ARGV[2], 'PX', ARGV[3])
        redis.call('set', KEYS[2], ARGV[4], 'PX', ARGV[3])
        return 1
        """;

    private readonly IDatabase _database;
    private readonly RedisOptions _options;

    public RedisAccountSessionStore(IConnectionMultiplexer connection, IOptions<RedisOptions> options)
    {
        _database = connection.GetDatabase();
        _options = options.Value;
    }

    public async Task<string> CreateAsync(string accountId, TimeSpan lifetime, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        string token = Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        string hash = ComputeCredentialHash(token);
        await _database.ScriptEvaluateAsync(CreateScript,
            new RedisKey[] { SessionKey(hash), LatestKey(accountId) },
            new RedisValue[] { _options.KeyPrefix, accountId, (long)lifetime.TotalMilliseconds, hash });
        return token;
    }

    public async Task<string?> GetAccountIdAsync(string? sessionToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return null;
        }
        cancellationToken.ThrowIfCancellationRequested();
        RedisValue value = await _database.StringGetAsync(SessionKey(ComputeCredentialHash(sessionToken)));
        return value.IsNullOrEmpty ? null : value.ToString();
    }

    public async Task DeleteAsync(string? sessionToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return;
        }
        cancellationToken.ThrowIfCancellationRequested();
        string hash = ComputeCredentialHash(sessionToken);
        RedisValue accountId = await _database.StringGetAsync(SessionKey(hash));
        await _database.KeyDeleteAsync(SessionKey(hash));
        if (!accountId.IsNullOrEmpty)
        {
            RedisKey latest = LatestKey(accountId.ToString());
            RedisValue current = await _database.StringGetAsync(latest);
            if (current == hash)
            {
                await _database.KeyDeleteAsync(latest);
            }
        }
    }

    private RedisKey SessionKey(string hash) => $"{_options.KeyPrefix}:account-session:{hash}";
    private RedisKey LatestKey(string accountId) => $"{_options.KeyPrefix}:account:{accountId}:latest-session";
    private static string ComputeCredentialHash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
