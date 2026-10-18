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
private bool AdvanceHintMask(Game game, int displayedSeconds)
    {
        if (game.GameSettings.HintSpeed == "off"
            || string.IsNullOrEmpty(game.GameState.ActualPromptText))
        {
            return false;
        }
        int revealCount = PromptHintSchedule.RevealCount(
            game.GameSettings,
            displayedSeconds,
            game.GameState.ActualPromptText.Count(char.IsLetter),
            _settingsOptions);
        string updated = PromptHintMask.WithRevealedLetters(game.GameState.ActualPromptText, game.GameState.PromptLetterRevealOrder, revealCount);
        if (updated == game.GameState.HiddenPromptText)
        {
            return false;
        }
        game.GameState.HiddenPromptText = updated;
        return true;
    }

    public async Task<CompleteTurnResult> CompleteTurnAsync(
        string gameHash, string drawingCredentialHash, CancellationToken cancellationToken = default)
    {
        GameStoreResult<CompleteTurnResult> result = await _store.MutateAsync(gameHash,
            game => CompleteTurn(game, drawingCredentialHash), cancellationToken);
        return result.Found ? result.Value! : new(RoundCommandStatus.GameNotFound);
    }

    public Task<RoundCommandStatus> RevealAnswerAsync(string gameHash, CancellationToken cancellationToken = default) =>
        SetHiddenPromptAsync(gameHash, game => game.GameState.ActualPromptText, cancellationToken);
    public Task<RoundCommandStatus> HideAnswerAsync(string gameHash, CancellationToken cancellationToken = default) =>
        SetHiddenPromptAsync(gameHash,
            game => string.IsNullOrWhiteSpace(game.GameState.PromptCategory)
                ? "? ? ?"
                : $"{game.GameState.PromptKind} • {game.GameState.PromptCategory}",
            cancellationToken);

    public async Task<AdvanceRoundResult> AdvanceRoundAsync(
        string gameHash,
        CancellationToken cancellationToken = default)
    {
        GameStoreResult<AdvanceRoundResult> result = await _store.MutateAsync(gameHash, game =>
        {
            if (game.GameState.CurrentRound > game.GameSettings.RoundsCount)
            {
                return GameMutation<AdvanceRoundResult>.NoChange(new(RoundCommandStatus.Applied, game.GameState.CurrentRound, true));
            }
            game.GameState.CurrentRound++;
            if (game.GameState.CurrentRound > game.GameSettings.RoundsCount)
            {
                game.FinalScores = game.GameState.Players.Concat(game.GameState.DisconnectedPlayers)
                    .Select(p => new FinalPlayerScore(p.CredentialHash, p.Score, p.Username)).ToList();
            }
            return GameMutation<AdvanceRoundResult>.Persist(new(RoundCommandStatus.Applied,
                game.GameState.CurrentRound, game.GameState.CurrentRound > game.GameSettings.RoundsCount));
        }, cancellationToken);
        return result.Found ? result.Value! : new(RoundCommandStatus.GameNotFound);
    }

    public async Task ResetSessionAsync(string gameHash, CancellationToken cancellationToken = default)
    {
        GameStoreResult<long> result = await _store.MutateAsync(gameHash, game =>
        {
            if (!game.GameState.IsGameStarted)
            {
                return GameMutation<long>.NoChange(0);
            }
            game.GameState.IsGameStarted = false;
            game.GameState.IsTimerVisible = false;
            game.GameState.CurrentDrawingTimeSeconds = game.GameSettings.DrawingTimeSeconds;
            game.GameState.DrawingCredentialHash = string.Empty;
            game.GameState.PendingDrawingCredentialHash = string.Empty;
            game.GameState.DrawingPlayerUsername = string.Empty;
            game.GameState.DrawingPlayerCredentialHashes.Clear();
            game.GameState.NoChatPermissionCredentialHashes.Clear();
            game.GameState.CorrectGuessPlayerUsernames.Clear();
            game.GameState.CorrectAnswerCount = 0;
            game.GameState.ActualPromptText = string.Empty;
            game.GameState.PromptCategory = string.Empty;
            game.GameState.PromptKind = string.Empty;
            game.GameState.HiddenPromptText = "? ? ?";
            game.GameState.CanvasEpoch++;
            return GameMutation<long>.Persist(game.GameState.CanvasEpoch);
        }, cancellationToken);
        if (result.Found && result.Value > 0)
        {
            await _canvas.ClearAsync(gameHash, result.Value, cancellationToken);
        }
    }

    private static GameMutation<StartTurnResult> StartTurn(Game game, string drawingHash)
    {
        if (game.GameState.DisconnectedPlayers.Count > 0)
        {
            return GameMutation<StartTurnResult>.NoChange(new(RoundCommandStatus.WaitingForPlayers,
                RemainingDrawingCredentialHashes: game.GameState.DrawingPlayerCredentialHashes.ToArray()));
        }

        Player? resumed = game.GameState.Players.FirstOrDefault(p => p.CredentialHash == drawingHash);
        if (resumed is not null && (game.GameState.DrawingCredentialHash == drawingHash
            || game.GameState.PendingDrawingCredentialHash == drawingHash))
        {
            return GameMutation<StartTurnResult>.NoChange(new(RoundCommandStatus.Applied, drawingHash, resumed.Username,
                game.GameSettings.PromptLanguage, game.GameState.DrawingPlayerCredentialHashes.ToArray(), true, game.GameState.CanvasEpoch));
        }

        string[] online = game.GameState.Players.Select(p => p.CredentialHash).ToArray();
        game.GameState.DrawingPlayerCredentialHashes = game.GameState.DrawingPlayerCredentialHashes.Where(online.Contains).ToList();
        if (game.GameState.DrawingPlayerCredentialHashes.Count == 0)
        {
            return GameMutation<StartTurnResult>.Persist(new(RoundCommandStatus.NoDrawingPlayers,
                RemainingDrawingCredentialHashes: Array.Empty<string>()));
        }
        Player? player = game.GameState.Players.FirstOrDefault(p => p.CredentialHash == drawingHash);
        if (player is null || !game.GameState.DrawingPlayerCredentialHashes.Contains(drawingHash))
        {
            return GameMutation<StartTurnResult>.NoChange(new(RoundCommandStatus.InvalidDrawingPlayer,
                RemainingDrawingCredentialHashes: game.GameState.DrawingPlayerCredentialHashes.ToArray()));
        }

        game.GameState.DrawingPlayerUsername = player.Username;
        game.GameState.DrawingCredentialHash = string.Empty;
        game.GameState.PendingDrawingCredentialHash = drawingHash;
        game.GameState.NoChatPermissionCredentialHashes.Clear();
        game.GameState.CorrectAnswerCount = 0;
        game.GameState.CorrectGuessPlayerUsernames.Clear();
        game.GameState.DrawingPlayerCredentialHashes.Remove(drawingHash);
        game.GameState.IsTimerVisible = true;
        game.GameState.CurrentDrawingTimeSeconds = game.GameSettings.DrawingTimeSeconds;
        game.GameState.HiddenPromptText = "? ? ?";
        game.GameState.PromptCategory = string.Empty;
        game.GameState.PromptKind = string.Empty;
        game.GameState.CanvasEpoch++;
        return GameMutation<StartTurnResult>.Persist(new(RoundCommandStatus.Applied, drawingHash, player.Username,
            game.GameSettings.PromptLanguage, game.GameState.DrawingPlayerCredentialHashes.ToArray(), false, game.GameState.CanvasEpoch));
    }

    private static GameMutation<CompleteTurnResult> CompleteTurn(Game game, string drawingHash)
    {
        if (game.GameState.DisconnectedPlayers.Count > 0)
        {
            return GameMutation<CompleteTurnResult>.NoChange(new(RoundCommandStatus.WaitingForPlayers));
        }
        if (game.GameState.DrawingCredentialHash != drawingHash)
        {
            return GameMutation<CompleteTurnResult>.NoChange(new(RoundCommandStatus.InvalidDrawingPlayer));
        }
        Player? drawing = game.GameState.Players.FirstOrDefault(p => p.CredentialHash == drawingHash);
        if (drawing is null)
        {
            return GameMutation<CompleteTurnResult>.NoChange(new(RoundCommandStatus.InvalidDrawingPlayer));
        }
        int expectedAnswers = Math.Max(0, game.GameState.Players.Count - 1);
        int bonus = game.GameState.CorrectAnswerCount == expectedAnswers && expectedAnswers > 0 ? 10 : game.GameState.CorrectAnswerCount > 0 ? 5 : 0;
        drawing.Score += bonus;
        game.GameState.Players = game.GameState.Players.OrderByDescending(p => p.Score).ToList();
        game.GameState.DrawingCredentialHash = string.Empty;
        game.GameState.PendingDrawingCredentialHash = string.Empty;
        game.GameState.IsTimerVisible = false;
        return GameMutation<CompleteTurnResult>.Persist(new(RoundCommandStatus.Applied, drawing.Username, game.GameState.ActualPromptText, bonus,
            game.GameState.Players.Select(p => new PlayerScore { Username = p.Username, Score = p.Score }).ToArray()));
    }

    private async Task<RoundCommandStatus> SetHiddenPromptAsync(
        string gameHash,
        Func<Game, string> value,
        CancellationToken cancellationToken)
    {
        GameStoreResult<RoundCommandStatus> result = await _store.UpdateAsync(gameHash, game =>
        {
            game.GameState.HiddenPromptText = value(game);
            return RoundCommandStatus.Applied;
        }, cancellationToken);
        return result.Found ? result.Value! : RoundCommandStatus.GameNotFound;
    }
}
