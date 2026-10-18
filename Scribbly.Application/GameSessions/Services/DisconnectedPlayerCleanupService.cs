using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Features.Games;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.GameSessions;

public class DisconnectedPlayerCleanupService : BackgroundService
{
    private readonly IGameStateStore _store;
    private readonly IGameLifecycleService _games;
    private readonly IDistributedGameSessionCoordinator _sessionCoordinator;
    private readonly GameLifecycleOptions _options;
    private readonly ILogger<DisconnectedPlayerCleanupService> _logger;

    public DisconnectedPlayerCleanupService(
        IGameStateStore store,
        IGameLifecycleService games,
        IDistributedGameSessionCoordinator sessions,
        GameLifecycleOptions options,
        ILogger<DisconnectedPlayerCleanupService> logger)
    {
        _store = store;
        _games = games;
        _sessionCoordinator = sessions;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.DisconnectedPlayerCleanupInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PruneAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task PruneAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Game> games;
        try
        {
            games = await _store.GetAllSnapshotsAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not enumerate games for reconnect cleanup");
            return;
        }

        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        foreach (Game game in games)
        {
            try
            {
                ExpiredDisconnectedPlayersResult result =
                    await _games.RemoveExpiredDisconnectedPlayersAsync(
                        game.GameHash,
                        nowUtc,
                        cancellationToken);

                if (result.GameRemoved && result.GameHash is not null)
                {
                    await _sessionCoordinator.RequestCancellationAsync(result.GameHash, CancellationToken.None);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Could not prune reconnect reservations for game {GameHash}",
                    game.GameHash);
            }
        }
    }
}
