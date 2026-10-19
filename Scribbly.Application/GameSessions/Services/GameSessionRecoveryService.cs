using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.GameSessions;

public class GameSessionRecoveryService : BackgroundService
{
    private readonly IGameStateStore _games;
    private readonly IDistributedGameSessionCoordinator _coordinator;
    private readonly ILogger<GameSessionRecoveryService> _logger;

    public GameSessionRecoveryService(
        IGameStateStore games,
        IDistributedGameSessionCoordinator coordinator,
        ILogger<GameSessionRecoveryService> logger)
    {
        _games = games;
        _coordinator = coordinator;
        _logger = logger;
    }

    public async Task<int> RecoverOnceAsync(CancellationToken cancellationToken = default)
    {
        var recovered = 0;
        foreach (Game game in await _games.GetAllSnapshotsAsync(cancellationToken))
        {
            if (!game.GameState.IsGameStarted || game.ResultsCaptured ||
                game.GameState.CurrentRound > game.GameSettings.RoundsCount)
            {
                continue;
            }

            try
            {
                GameSessionStartResult result = await _coordinator.TryStartAsync(
                    game.GameHash,
                    new GameSettings
                    {
                        DrawingTimeSeconds = game.GameSettings.DrawingTimeSeconds,
                        RoundsCount = game.GameSettings.RoundsCount,
                        PromptLanguage = game.GameSettings.PromptLanguage
                    },
                    cancellationToken);

                if (result is GameSessionStartResult.Started or GameSessionStartResult.AlreadyRunning)
                {
                    recovered++;
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Could not recover game session {GameHash}", game.GameHash);
            }
        }

        return recovered;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        try
        {
            do
            {
                try
                {
                    await RecoverOnceAsync(stoppingToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogError(exception, "Could not scan for recoverable game sessions");
                }
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
