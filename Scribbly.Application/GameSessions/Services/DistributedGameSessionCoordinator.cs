using Scribbly.Application.Features.Rounds;
using Scribbly.Application.Features.Games;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.GameSessions;

public class DistributedGameSessionCoordinator :
    IDistributedGameSessionCoordinator,
    IHostedService,
    IDisposable
{
    private readonly IGameRoundService _roundService;
    private readonly IGameSessionLeaseExecutor _leaseExecutor;
    private readonly IGameSessionRunner _sessionRunner;
    private readonly IGameSessionCancellationBus _cancellationBus;
    private readonly ILogger<DistributedGameSessionCoordinator> _logger;
    private readonly IGameSessionEvents _events;
    private readonly IGameLifecycleService _games;
    private readonly CancellationTokenSource _shutdown = new();
    private int _stopping;

    public DistributedGameSessionCoordinator(
        IGameRoundService roundService,
        IGameSessionLeaseExecutor leaseExecutor,
        IGameSessionRunner sessionRunner,
        IGameSessionCancellationBus cancellationBus,
        IGameSessionEvents events,
        IGameLifecycleService games,
        ILogger<DistributedGameSessionCoordinator> logger)
    {
        _roundService = roundService;
        _leaseExecutor = leaseExecutor;
        _sessionRunner = sessionRunner;
        _cancellationBus = cancellationBus;
        _events = events;
        _games = games;
        _logger = logger;
    }

    public Task<GameSessionStartResult> TryStartAsync(
        string gameHash,
        GameSettings settings,
        CancellationToken cancellationToken = default) =>
        TryStartCoreAsync(gameHash, settings, null, cancellationToken);

    public Task<GameSessionStartResult> TryStartAuthorizedAsync(
        string gameHash,
        string roomAccessToken,
        GameSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roomAccessToken);
        return TryStartCoreAsync(gameHash, settings, roomAccessToken, cancellationToken);
    }

    private async Task<GameSessionStartResult> TryStartCoreAsync(
        string gameHash,
        GameSettings settings,
        string? roomAccessToken,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameHash);
        ArgumentNullException.ThrowIfNull(settings);

        if (Volatile.Read(ref _stopping) != 0 || _shutdown.IsCancellationRequested)
        {
            throw new InvalidOperationException("The game session coordinator is stopping.");
        }

        TaskCompletionSource<GameSessionStartResult> startOutcome = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _ = ExecuteSessionAsync(
            gameHash,
            settings,
            roomAccessToken,
            startOutcome,
            _shutdown.Token);

        return await startOutcome.Task.WaitAsync(cancellationToken);
    }

    public Task RequestCancellationAsync(
        string gameHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameHash);
        return _cancellationBus.PublishAsync(gameHash, cancellationToken);
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _stopping, 1) == 0)
        {
            _shutdown.Cancel();
        }

        return Task.CompletedTask;
    }

    public void Dispose() => _shutdown.Dispose();

    private async Task ExecuteSessionAsync(
        string gameHash,
        GameSettings settings,
        string? roomAccessToken,
        TaskCompletionSource<GameSessionStartResult> startOutcome,
        CancellationToken shutdownToken)
    {
        try
        {
            GameSessionLeaseExecutionResult leaseResult = await _leaseExecutor.ExecuteAsync(
                gameHash,
                async leaseCancellationToken =>
                {
                    StartSessionResult result = roomAccessToken is null
                        ? await _roundService.StartSessionAsync(gameHash, settings, leaseCancellationToken)
                        : await _roundService.StartAuthorizedSessionAsync(gameHash, roomAccessToken, settings, leaseCancellationToken);

                    if (result.Status == RoundCommandStatus.AlreadyRunning)
                    {
                        StartSessionResult? resumed = roomAccessToken is null
                            ? await _roundService.ResumeSessionAsync(gameHash, settings, leaseCancellationToken)
                            : await _roundService.ResumeAuthorizedSessionAsync(gameHash, roomAccessToken, settings, leaseCancellationToken);
                        if (resumed is not null)
                        {
                            result = resumed;
                        }
                    }

                    GameSessionStartResult mappedResult = MapStartResult(result.Status);
                    startOutcome.TrySetResult(mappedResult);

                    if (mappedResult != GameSessionStartResult.Started)
                    {
                        return;
                    }

                    await _sessionRunner.RunAsync(gameHash, leaseCancellationToken);

                    if (!shutdownToken.IsCancellationRequested && !leaseCancellationToken.IsCancellationRequested)
                    {
                        await _roundService.ResetSessionAsync(gameHash, CancellationToken.None);
                    }
                },
                shutdownToken);

            if (leaseResult == GameSessionLeaseExecutionResult.AlreadyOwned)
            {
                startOutcome.TrySetResult(GameSessionStartResult.AlreadyRunning);
            }
            else if (leaseResult == GameSessionLeaseExecutionResult.OwnershipLost)
            {
                startOutcome.TrySetResult(GameSessionStartResult.AlreadyRunning);
                _logger.LogWarning(
                    "Game #{GameHash}: distributed session ownership was lost",
                    gameHash);
            }
        }
        catch (OperationCanceledException)
        {
            startOutcome.TrySetCanceled();
            _logger.LogInformation("Game #{GameHash}: distributed session cancelled", gameHash);
        }
        catch (Exception exception)
        {
            bool started = startOutcome.Task.IsCompletedSuccessfully &&
                           startOutcome.Task.Result == GameSessionStartResult.Started;
            if (!started)
            {
                startOutcome.TrySetException(exception);
            }
            else
            {
                try
                {
                    var snapshot = await _games.GetSnapshotAsync(gameHash, CancellationToken.None);
                    bool pendingCompletion = snapshot is not null &&
                        snapshot.GameState.CurrentRound > snapshot.GameSettings.RoundsCount;
                    if (!pendingCompletion)
                    {
                        await _roundService.ResetSessionAsync(gameHash, CancellationToken.None);
                    }
                    await _events.GameSessionFailedAsync(
                        gameHash,
                        "The game session stopped unexpectedly. The host can retry.",
                        CancellationToken.None);
                }
                catch (Exception recoveryException)
                {
                    _logger.LogError(recoveryException, "Game #{GameHash}: failure recovery also failed", gameHash);
                }
            }
            _logger.LogError(exception, "Game #{GameHash}: distributed session failed", gameHash);
        }
    }

    private static GameSessionStartResult MapStartResult(RoundCommandStatus status) =>
        status switch
        {
            RoundCommandStatus.Applied => GameSessionStartResult.Started,
            RoundCommandStatus.GameNotFound => GameSessionStartResult.GameNotFound,
            RoundCommandStatus.TooFewPlayers => GameSessionStartResult.TooFewPlayers,
            RoundCommandStatus.GameFinished => GameSessionStartResult.GameFinished,
            RoundCommandStatus.AlreadyRunning => GameSessionStartResult.AlreadyRunning,
            RoundCommandStatus.InvalidHostCredential => GameSessionStartResult.InvalidHostCredential,
            RoundCommandStatus.InvalidSettings => GameSessionStartResult.InvalidSettings,
            _ => throw new InvalidOperationException(
                $"Unexpected round start status '{status}'.")
        };
        }
