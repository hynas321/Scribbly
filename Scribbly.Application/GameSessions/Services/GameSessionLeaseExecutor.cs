using Scribbly.Application.Abstractions.Time;

namespace Scribbly.Application.GameSessions;

public class GameSessionLeaseExecutor : IGameSessionLeaseExecutor
{
    private readonly IGameSessionLeaseProvider _leaseProvider;
    private readonly IGameSessionCancellationBus _cancellationBus;
    private readonly IAsyncDelay _delay;
    private readonly TimeSpan _renewalInterval;
    private readonly GameSessionExecutionContext? _execution;

    public GameSessionLeaseExecutor(
        IGameSessionLeaseProvider leaseProvider,
        IGameSessionCancellationBus cancellationBus,
        IAsyncDelay delay,
        GameSessionLeaseExecutionOptions options,
        GameSessionExecutionContext? execution = null)
    {
        ArgumentNullException.ThrowIfNull(leaseProvider);
        ArgumentNullException.ThrowIfNull(cancellationBus);
        ArgumentNullException.ThrowIfNull(delay);
        ArgumentNullException.ThrowIfNull(options);

        if (options.RenewalInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "The session lease renewal interval must be greater than zero.");
        }

        _leaseProvider = leaseProvider;
        _cancellationBus = cancellationBus;
        _delay = delay;
        _renewalInterval = options.RenewalInterval;
        _execution = execution;
    }

    public async Task<GameSessionLeaseExecutionResult> ExecuteAsync(
        string gameHash,
        Func<CancellationToken, Task> runSession,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameHash);
        ArgumentNullException.ThrowIfNull(runSession);

        string ownerToken = Guid.NewGuid().ToString("N");
        using CancellationTokenSource sessionCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        long cancellationVersion = await _cancellationBus.GetCancellationVersionAsync(
            gameHash,
            cancellationToken);
        IAsyncDisposable cancellationSubscription = await _cancellationBus.SubscribeAsync(
            gameHash,
            () =>
            {
                sessionCancellation.Cancel();
                return Task.CompletedTask;
            },
            cancellationToken);
        bool leaseAcquired = false;
        try
        {
            sessionCancellation.Token.ThrowIfCancellationRequested();
            leaseAcquired = await _leaseProvider.TryAcquireAsync(
                gameHash,
                ownerToken,
                cancellationToken);

            if (!leaseAcquired)
            {
                return GameSessionLeaseExecutionResult.AlreadyOwned;
            }

            long currentCancellationVersion = await _cancellationBus.GetCancellationVersionAsync(
                gameHash,
                cancellationToken);
            if (currentCancellationVersion != cancellationVersion)
            {
                sessionCancellation.Cancel();
            }
            sessionCancellation.Token.ThrowIfCancellationRequested();

            using CancellationTokenSource stopRenewal = new();
            Task<bool> renewalTask = MonitorOwnershipAsync(
                gameHash,
                ownerToken,
                cancellationVersion,
                sessionCancellation,
                stopRenewal.Token);

            try
            {
                using IDisposable? ownership = _execution?.Enter(gameHash, ownerToken);
                Task sessionTask = runSession(sessionCancellation.Token);
                await Task.WhenAny(sessionTask, renewalTask);

                if (renewalTask.IsCompleted)
                {
                    bool ownershipLost;

                    try
                    {
                        ownershipLost = await renewalTask;
                    }
                    catch
                    {
                        await ObserveLeaseCancellationAsync(sessionTask, sessionCancellation.Token);
                        throw;
                    }

                    if (ownershipLost)
                    {
                        await ObserveLeaseCancellationAsync(sessionTask, sessionCancellation.Token);
                        return GameSessionLeaseExecutionResult.OwnershipLost;
                    }
                }

                await sessionTask;
                return GameSessionLeaseExecutionResult.Completed;
            }
            finally
            {
                stopRenewal.Cancel();

                try
                {
                    await renewalTask;
                }
                catch (OperationCanceledException) when (stopRenewal.IsCancellationRequested)
                {
                }
                catch when (renewalTask.IsFaulted)
                {
                }
            }
        }
        finally
        {
            try
            {
                await cancellationSubscription.DisposeAsync();
            }
            finally
            {
                if (leaseAcquired)
                {
                    await _leaseProvider.ReleaseAsync(gameHash, ownerToken, CancellationToken.None);
                }
            }
        }
    }

    private async Task<bool> MonitorOwnershipAsync(
        string gameHash,
        string ownerToken,
        long cancellationVersion,
        CancellationTokenSource sessionCancellation,
        CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                await _delay.DelayAsync(_renewalInterval, cancellationToken);
                long currentVersion = await _cancellationBus.GetCancellationVersionAsync(
                    gameHash, cancellationToken);
                if (currentVersion != cancellationVersion)
                {
                    sessionCancellation.Cancel();
                    return false;
                }

                bool renewed = await _leaseProvider.RenewAsync(
                    gameHash,
                    ownerToken,
                    cancellationToken);

                if (!renewed)
                {
                    sessionCancellation.Cancel();
                    return true;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            sessionCancellation.Cancel();
            throw;
        }
    }

    private static async Task ObserveLeaseCancellationAsync(
        Task sessionTask,
        CancellationToken sessionCancellationToken)
    {
        try
        {
            await sessionTask;
        }
        catch (OperationCanceledException) when (sessionCancellationToken.IsCancellationRequested)
        {
        }
    }
        }
