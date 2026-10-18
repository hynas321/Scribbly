using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Features.Accounts;
using Scribbly.Application.Features.Games;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.GameSessions;

public class CompletedGameResultRecoveryService : BackgroundService
{
    private readonly IGameStateStore _games;
    private readonly IGameResultService _results;
    private readonly IGameLifecycleService _lifecycle;
    private readonly IGameSessionEvents _events;
    private readonly IGameSessionLeaseExecutor _leases;
    private readonly ILogger<CompletedGameResultRecoveryService> _logger;

    public CompletedGameResultRecoveryService(
        IGameStateStore games,
        IGameResultService results,
        IGameLifecycleService lifecycle,
        IGameSessionEvents events,
        IGameSessionLeaseExecutor leases,
        ILogger<CompletedGameResultRecoveryService> logger)
    {
        _games = games;
        _results = results;
        _lifecycle = lifecycle;
        _events = events;
        _leases = leases;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        try
        {
            do
            {
                await RecoverOnceAsync(stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }

    private async Task RecoverOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyCollection<Game> games = await _games.GetAllSnapshotsAsync(cancellationToken);
            foreach (Game candidate in games.Where(game =>
                         game.GameState.CurrentRound > game.GameSettings.RoundsCount))
            {
                try
                {
                    await _leases.ExecuteAsync(candidate.GameHash, async leaseToken =>
                    {
                        Game? current = await _games.GetSnapshotAsync(candidate.GameHash, leaseToken);
                        if (current is null || current.GameState.CurrentRound <= current.GameSettings.RoundsCount)
                        {
                            return;
                        }
                        if (!current.ResultsCaptured && !await _results.CaptureAsync(candidate.GameHash, leaseToken))
                        {
                            return;
                        }

                        await _events.GameEndedAsync(candidate.GameHash, leaseToken);
                        await _lifecycle.DeleteAsync(candidate.GameHash, leaseToken);
                    }, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Could not finalize completed room {GameHash}", candidate.GameHash);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Could not inspect pending completed games");
        }
    }
}
