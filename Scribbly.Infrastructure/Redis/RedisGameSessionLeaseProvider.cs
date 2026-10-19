using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Scribbly.Application.GameSessions;

namespace Scribbly.Infrastructure.Redis;

public class RedisGameSessionLeaseProvider : IGameSessionLeaseProvider
{
    private readonly IDatabase _database;
    private readonly RedisOptions _options;

    public RedisGameSessionLeaseProvider(
        IConnectionMultiplexer connection,
        IOptions<RedisOptions> options)
    {
        _database = connection.GetDatabase();
        _options = options.Value;
    }

    public async Task<bool> TryAcquireAsync(
        string gameHash,
        string ownerToken,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(gameHash, ownerToken);
        cancellationToken.ThrowIfCancellationRequested();
        return await _database.LockTakeAsync(LeaseKey(gameHash), ownerToken, LeaseTime);
    }

    public async Task<bool> RenewAsync(
        string gameHash,
        string ownerToken,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(gameHash, ownerToken);
        cancellationToken.ThrowIfCancellationRequested();
        return await _database.LockExtendAsync(LeaseKey(gameHash), ownerToken, LeaseTime);
    }

    public async Task<bool> ReleaseAsync(
        string gameHash,
        string ownerToken,
        CancellationToken cancellationToken = default)
    {
        ValidateArguments(gameHash, ownerToken);
        cancellationToken.ThrowIfCancellationRequested();
        return await _database.LockReleaseAsync(LeaseKey(gameHash), ownerToken);
    }

    private static void ValidateArguments(string gameHash, string ownerToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerToken);
    }

    private TimeSpan LeaseTime => TimeSpan.FromSeconds(_options.SessionLeaseSeconds);
    private RedisKey LeaseKey(string gameHash) =>
        $"{_options.KeyPrefix}:game:{gameHash}:session-lease";
}
