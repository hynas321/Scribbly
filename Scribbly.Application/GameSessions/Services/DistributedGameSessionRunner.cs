using Scribbly.Application.Abstractions.Time;
using Scribbly.Application.Features.Accounts;
using Scribbly.Application.Features.Games;
using Scribbly.Application.Features.Rounds;
using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Services.Interfaces;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.GameSessions;

public class DistributedGameSessionRunner : IGameSessionRunner
{
    private readonly IGameRoundService _rounds;
    private readonly IRandomPromptService _prompts;
    private readonly IDrawingPlayerSelector _drawingPlayerSelector;
    private readonly IGameSessionEvents _events;
    private readonly IAsyncDelay _delay;
    private readonly IGameLifecycleService _games;
    private readonly IGameResultService _results;
    private readonly GameSessionTimingOptions _timing;

    public DistributedGameSessionRunner(
        IGameRoundService rounds,
        IRandomPromptService prompts,
        IDrawingPlayerSelector drawingPlayerSelector,
        IGameSessionEvents events,
        IAsyncDelay delay,
        IGameLifecycleService games,
        IGameResultService results,
        GameSessionTimingOptions timing)
    {
        _rounds = rounds;
        _prompts = prompts;
        _drawingPlayerSelector = drawingPlayerSelector;
        _events = events;
        _delay = delay;
        _games = games;
        _results = results;
        _timing = timing;
    }

    public async Task RunAsync(string gameHash, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameHash);
        await _events.GameStartedAsync(gameHash, cancellationToken);

        while (true)
        {
            PrepareRoundResult prepared = await _rounds.PrepareRoundAsync(gameHash, cancellationToken);
            if (prepared.Status == RoundCommandStatus.GameNotFound)
            {
                return;
            }

            await SetCanvasTextAsync(gameHash, $"Round {prepared.CurrentRound}", BootstrapColors.Green, cancellationToken);
            await _delay.DelayAsync(_timing.RoundIntroduction, cancellationToken);

            IReadOnlyCollection<string> remaining = prepared.RemainingDrawingCredentialHashes ?? Array.Empty<string>();
            string? resumed = prepared.CurrentDrawingCredentialHash;
            while (remaining.Count > 0 || resumed is not null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string selected = resumed ?? _drawingPlayerSelector.Select(remaining);
                resumed = null;

                StartTurnResult turn = await WaitForTurnAsync(gameHash, selected, cancellationToken);
                if (turn.Status == RoundCommandStatus.GameNotFound)
                {
                    return;
                }
                remaining = turn.RemainingDrawingCredentialHashes ?? Array.Empty<string>();
                if (turn.Status == RoundCommandStatus.NoDrawingPlayers)
                {
                    break;
                }
                if (turn.Status != RoundCommandStatus.Applied)
                {
                    continue;
                }

                string drawingCredential = turn.DrawingCredentialHash
                    ?? throw new InvalidOperationException("Drawing credential is missing.");
                string drawingUsername = turn.DrawingPlayerUsername
                    ?? throw new InvalidOperationException("Drawing player is missing.");
                if (!turn.Resumed)
                {
                    await _events.CanvasClearedAsync(gameHash, turn.CanvasEpoch, cancellationToken);
                }

                GamePrompt prompt = await _prompts.FetchPromptForLanguageAsync(turn.PromptLanguage ?? string.Empty, cancellationToken)
                    ?? throw new InvalidOperationException("No drawing prompt is available.");

                await _events.CorrectGuessPlayersUpdatedAsync(gameHash, Array.Empty<string>(), cancellationToken);
                await _events.DrawingPlayerUpdatedAsync(gameHash, drawingUsername, cancellationToken);
                await SetCanvasTextAsync(gameHash, $"{drawingUsername} is going to draw in 5s", BootstrapColors.Green, cancellationToken);
                if (!turn.Resumed)
                {
                    await _delay.DelayAsync(_timing.DrawingPlayerIntroduction, cancellationToken);
                }

                RoundCommandStatus activation = await RepeatUntilPlayersReadyAsync(
                    () => _rounds.ActivatePromptAsync(gameHash, drawingCredential, prompt, cancellationToken),
                    status => status == RoundCommandStatus.WaitingForPlayers,
                    cancellationToken);
                if (activation == RoundCommandStatus.GameNotFound)
                {
                    return;
                }
                if (activation != RoundCommandStatus.Applied)
                {
                    continue;
                }

                await _events.PromptRefreshRequestedAsync(gameHash, cancellationToken);
                await _events.TimerVisibilityUpdatedAsync(gameHash, true, cancellationToken);
                if (!await RunTimerAsync(gameHash, cancellationToken))
                {
                    return;
                }

                CompleteTurnResult completed = await RepeatUntilPlayersReadyAsync(
                    () => _rounds.CompleteTurnAsync(gameHash, drawingCredential, cancellationToken),
                    result => result.Status == RoundCommandStatus.WaitingForPlayers,
                    cancellationToken);
                if (completed.Status == RoundCommandStatus.GameNotFound)
                {
                    return;
                }
                if (completed.Status != RoundCommandStatus.Applied)
                {
                    continue;
                }

                await SendDrawingBonusAsync(gameHash, completed, cancellationToken);
                await _events.PlayerScoresUpdatedAsync(gameHash, completed.Scores ?? Array.Empty<PlayerScore>(), cancellationToken);
                await _events.TimerVisibilityUpdatedAsync(gameHash, false, cancellationToken);
                await SetCanvasTextAsync(gameHash, "The drawing phase has ended", BootstrapColors.Green, cancellationToken);
                await SendAnnouncementAsync(gameHash, $"The answer was: {completed.ActualPromptText}", BootstrapColors.Yellow, cancellationToken);

                if (await _rounds.RevealAnswerAsync(gameHash, cancellationToken) == RoundCommandStatus.GameNotFound)
                {
                    return;
                }
                await _events.PromptRefreshRequestedAsync(gameHash, cancellationToken);
                await _delay.DelayAsync(_timing.AnswerReveal, cancellationToken);
                if (await _rounds.HideAnswerAsync(gameHash, cancellationToken) == RoundCommandStatus.GameNotFound)
                {
                    return;
                }
                await _events.PromptRefreshRequestedAsync(gameHash, cancellationToken);
            }

            AdvanceRoundResult advanced = await _rounds.AdvanceRoundAsync(gameHash, cancellationToken);
            if (advanced.Status == RoundCommandStatus.GameNotFound)
            {
                return;
            }
            if (advanced.GameFinished)
            {
                if (!await _results.CaptureAsync(gameHash, cancellationToken))
                {
                    return;
                }
                await SetCanvasTextAsync(gameHash, "Thank you for playing!", BootstrapColors.Green, cancellationToken);
                await _events.GameEndedAsync(gameHash, cancellationToken);
                await _delay.DelayAsync(_timing.GameFinished, cancellationToken);
                await _games.DeleteAsync(gameHash, cancellationToken);
                return;
            }
            await _events.CurrentRoundUpdatedAsync(gameHash, advanced.CurrentRound, cancellationToken);
        }
    }

    private Task<StartTurnResult> WaitForTurnAsync(
        string gameHash,
        string credential,
        CancellationToken cancellationToken) =>
        RepeatUntilPlayersReadyAsync(
            () => _rounds.StartTurnAsync(gameHash, credential, cancellationToken),
            result => result.Status == RoundCommandStatus.WaitingForPlayers,
            cancellationToken);

    private async Task<TResult> RepeatUntilPlayersReadyAsync<TResult>(
        Func<Task<TResult>> attempt, Func<TResult, bool> waitingForPlayers, CancellationToken cancellationToken)
    {
        TResult result;
        do
        {
            result = await attempt();
            if (waitingForPlayers(result))
            {
                await _delay.DelayAsync(TimeSpan.FromMilliseconds(500), cancellationToken);
            }
        } while (waitingForPlayers(result));
        return result;
    }

    private async Task<bool> RunTimerAsync(string gameHash, CancellationToken cancellationToken)
    {
        TimerTickResult initialized = await _rounds.InitializeTimerAsync(gameHash, cancellationToken);
        if (initialized.Status == RoundCommandStatus.GameNotFound)
        {
            return false;
        }
        while (true)
        {
            TimerTickResult result = await _rounds.TickTimerAsync(gameHash, cancellationToken);
            if (result.Status == RoundCommandStatus.GameNotFound)
            {
                return false;
            }
            if (result.Status == RoundCommandStatus.WaitingForPlayers)
            {
                await _delay.DelayAsync(TimeSpan.FromMilliseconds(500), cancellationToken);
                continue;
            }
            if (result.Completed)
            {
                return true;
            }
            await _events.TimerUpdatedAsync(gameHash, result.CurrentDrawingTimeSeconds, cancellationToken);
            if (result.HintChanged)
            {
                await _events.PromptRefreshRequestedAsync(gameHash, cancellationToken);
            }
            await _delay.DelayAsync(_timing.TimerTick, cancellationToken);
        }
    }

    private Task SetCanvasTextAsync(string gameHash, string text, string color, CancellationToken cancellationToken) =>
        _events.CanvasTextSetAsync(gameHash, new AnnouncementMessage { Text = text, BootstrapBackgroundColor = color }, cancellationToken);

    private Task SendAnnouncementAsync(string gameHash, string text, string color, CancellationToken cancellationToken) =>
        _events.AnnouncementSentAsync(gameHash, new AnnouncementMessage { Text = text, BootstrapBackgroundColor = color }, cancellationToken);

    private Task SendDrawingBonusAsync(
        string gameHash,
        CompleteTurnResult completed,
        CancellationToken cancellationToken)
    {
        string username = completed.DrawingPlayerUsername ?? "Drawing player";
        return SendAnnouncementAsync(gameHash,
            completed.DrawingBonus > 0
                ? $"{username} received the drawing bonus (+{completed.DrawingBonus} points)"
                : $"{username} received no drawing bonus",
            completed.DrawingBonus > 0 ? BootstrapColors.Green : BootstrapColors.Red,
            cancellationToken);
    }
}
