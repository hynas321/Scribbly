using Scribbly.Application.Abstractions.Canvas;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Abstractions.Security;
using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Features.Settings;
using Scribbly.Application.Features.Settings.Configuration;
using Scribbly.Application.Persistence;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Rounds;

public partial class GameRoundService
{
private async Task<StartSessionResult> StartSessionCoreAsync(
        string gameHash, GameSettings settings, string? hostHash, bool allowAlreadyRunning, CancellationToken cancellationToken)
    {
        if (!GameSettingsPolicy.IsValid(settings, _settingsOptions))
        {
            return new(RoundCommandStatus.InvalidSettings);
        }
        GameStoreResult<StartSessionResult> result = await _store.MutateAsync(gameHash, game =>
        {
            if (hostHash is not null && (game.HostCredentialHash != hostHash
                || game.GameState.Players.All(p => p.CredentialHash != hostHash)))
            {
                return GameMutation<StartSessionResult>.NoChange(new(RoundCommandStatus.InvalidHostCredential));
            }

            if (game.GameState.CurrentRound > game.GameSettings.RoundsCount)
            {
                return GameMutation<StartSessionResult>.NoChange(new(RoundCommandStatus.GameFinished));
            }
            if (game.GameState.IsGameStarted)
            {
                if (!allowAlreadyRunning)
                {
                    return GameMutation<StartSessionResult>.NoChange(new(RoundCommandStatus.AlreadyRunning));
                }
                int reconnectable = game.GameState.Players.Count + game.GameState.DisconnectedPlayers.Count;
                RoundCommandStatus status = reconnectable >= 2 ? RoundCommandStatus.Applied : RoundCommandStatus.TooFewPlayers;
                return GameMutation<StartSessionResult>.NoChange(new(status));
            }
            if (game.GameState.Players.Count < 2)
            {
                return GameMutation<StartSessionResult>.NoChange(new(RoundCommandStatus.TooFewPlayers));
            }
            game.ChatMessages.Clear();
            game.GameState.IsGameStarted = true;
            game.GameSettings.DrawingTimeSeconds = settings.DrawingTimeSeconds;
            game.GameSettings.RoundsCount = settings.RoundsCount;
            game.GameSettings.PromptLanguage = settings.PromptLanguage;
            return GameMutation<StartSessionResult>.Persist(new(RoundCommandStatus.Applied));
        }, cancellationToken);
        return result.Found ? result.Value! : new(RoundCommandStatus.GameNotFound);
    }

    public async Task<PrepareRoundResult> PrepareRoundAsync(
        string gameHash,
        CancellationToken cancellationToken = default)
    {
        GameStoreResult<PrepareRoundResult> result = await _store.UpdateAsync<PrepareRoundResult>(gameHash, game =>
        {
            if (game.GameState.DrawingPlayerCredentialHashes.Count == 0
                && string.IsNullOrWhiteSpace(game.GameState.DrawingCredentialHash)
                && string.IsNullOrWhiteSpace(game.GameState.PendingDrawingCredentialHash))
            {
                game.GameState.DrawingPlayerCredentialHashes = game.GameState.Players.Concat(game.GameState.DisconnectedPlayers)
                    .Select(p => p.CredentialHash).Distinct(StringComparer.Ordinal).ToList();
            }
            string? current = !string.IsNullOrWhiteSpace(game.GameState.DrawingCredentialHash)
                ? game.GameState.DrawingCredentialHash
                : !string.IsNullOrWhiteSpace(game.GameState.PendingDrawingCredentialHash)
                    ? game.GameState.PendingDrawingCredentialHash
                    : null;
            return new(RoundCommandStatus.Applied, game.GameState.CurrentRound,
                game.GameState.DrawingPlayerCredentialHashes.ToArray(), current);
        }, cancellationToken);
        return result.Found ? result.Value! : new(RoundCommandStatus.GameNotFound);
    }

    public async Task<StartTurnResult> StartTurnAsync(
        string gameHash, string drawingCredentialHash, CancellationToken cancellationToken = default)
    {
        GameStoreResult<StartTurnResult> result = await _store.MutateAsync(gameHash,
            game => StartTurn(game, drawingCredentialHash), cancellationToken);
        if (!result.Found)
        {
            return new(RoundCommandStatus.GameNotFound);
        }
        StartTurnResult turn = result.Value!;
        if (turn.Status == RoundCommandStatus.Applied && !turn.Resumed)
        {
            await _canvas.ClearAsync(gameHash, turn.CanvasEpoch, cancellationToken);
        }
        return turn;
    }

    public async Task<RoundCommandStatus> ActivatePromptAsync(
        string gameHash, string drawingCredentialHash, GamePrompt prompt, CancellationToken cancellationToken = default)
    {
        GameStoreResult<RoundCommandStatus> result = await _store.MutateAsync(gameHash, game =>
        {
            if (game.GameState.DisconnectedPlayers.Count > 0)
            {
                return GameMutation<RoundCommandStatus>.NoChange(RoundCommandStatus.WaitingForPlayers);
            }
            if (game.GameState.DrawingCredentialHash == drawingCredentialHash)
            {
                return GameMutation<RoundCommandStatus>.NoChange(RoundCommandStatus.Applied);
            }
            if (game.GameState.PendingDrawingCredentialHash != drawingCredentialHash
                || game.GameState.Players.All(p => p.CredentialHash != drawingCredentialHash))
            {
                return GameMutation<RoundCommandStatus>.NoChange(RoundCommandStatus.InvalidDrawingPlayer);
            }

            game.GameState.ActualPromptText = prompt.Text;
            game.GameState.PromptCategory = prompt.Category;
            game.GameState.PromptKind = prompt.Kind;
            game.GameState.HiddenPromptText = prompt.HiddenHint;
            game.GameState.PromptLetterRevealOrder = PromptHintMask.ShuffledRevealOrder(prompt.HiddenHint, Random.Shared);
            game.GameState.DrawingCredentialHash = drawingCredentialHash;
            game.GameState.PendingDrawingCredentialHash = string.Empty;
            game.GameState.CurrentDrawingTimeSeconds = game.GameSettings.DrawingTimeSeconds;
            return GameMutation<RoundCommandStatus>.Persist(RoundCommandStatus.Applied);
        }, cancellationToken);
        return result.Found ? result.Value! : RoundCommandStatus.GameNotFound;
    }

    public async Task<TimerTickResult> InitializeTimerAsync(
        string gameHash,
        CancellationToken cancellationToken = default)
    {
        GameStoreResult<TimerTickResult> result = await _store.UpdateAsync<TimerTickResult>(gameHash, game =>
        {
            if (!game.GameState.IsTimerVisible || string.IsNullOrWhiteSpace(game.GameState.DrawingCredentialHash))
            {
                game.GameState.CurrentDrawingTimeSeconds = game.GameSettings.DrawingTimeSeconds;
            }
            return new(RoundCommandStatus.Applied, game.GameState.CurrentDrawingTimeSeconds);
        }, cancellationToken);
        return result.Found ? result.Value! : new(RoundCommandStatus.GameNotFound);
    }

    public async Task<TimerTickResult> TickTimerAsync(string gameHash, CancellationToken cancellationToken = default)
    {
        GameStoreResult<TimerTickResult> result = await _store.MutateAsync(gameHash, game =>
        {
            if (game.GameState.DisconnectedPlayers.Count > 0)
            {
                return GameMutation<TimerTickResult>.NoChange(new(RoundCommandStatus.WaitingForPlayers, game.GameState.CurrentDrawingTimeSeconds));
            }
            bool completed = game.GameState.CorrectAnswerCount >= Math.Max(0, game.GameState.Players.Count - 1)
                || game.GameState.CurrentDrawingTimeSeconds <= 0;
            if (completed)
            {
                return GameMutation<TimerTickResult>.NoChange(new(RoundCommandStatus.Applied, game.GameState.CurrentDrawingTimeSeconds, true));
            }
            int current = game.GameState.CurrentDrawingTimeSeconds;
            game.GameState.CurrentDrawingTimeSeconds--;
            return GameMutation<TimerTickResult>.Persist(new(RoundCommandStatus.Applied, current, false, AdvanceHintMask(game, current)));
        }, cancellationToken);
        return result.Found ? result.Value! : new(RoundCommandStatus.GameNotFound);
    }
}
