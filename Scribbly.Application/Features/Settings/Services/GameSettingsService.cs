using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Abstractions.Security;
using Scribbly.Application.Features.Settings.Configuration;
using Scribbly.Application.Persistence;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Settings;

public class GameSettingsService : IGameSettingsService
{
    private readonly IGameStateStore _store;
    private readonly IRoomCredentialService _credentials;
    private readonly IPromptRepository _prompts;
    private readonly GameSettingsOptions _options;

    public GameSettingsService(
        IGameStateStore store,
        IRoomCredentialService credentials,
        IPromptRepository prompts,
        GameSettingsOptions options)
    {
        _store = store;
        _credentials = credentials;
        _prompts = prompts;
        _options = options;
    }

    public Task<GameSettingsCommandResult> SetDrawingTimeAsync(
        string gameHash, string token, int seconds, CancellationToken cancellationToken = default) =>
        GameSettingsPolicy.IsDrawingTimeValid(seconds, _options)
            ? UpdateAsync(gameHash, token, s => s.DrawingTimeSeconds = seconds, cancellationToken)
            : Task.FromResult(new GameSettingsCommandResult(GameSettingsCommandStatus.InvalidSetting));
    public Task<GameSettingsCommandResult> SetRoundsCountAsync(
        string gameHash, string token, int rounds, CancellationToken cancellationToken = default) =>
        GameSettingsPolicy.IsRoundsCountValid(rounds, _options)
            ? UpdateAsync(gameHash, token, s => s.RoundsCount = rounds, cancellationToken)
            : Task.FromResult(new GameSettingsCommandResult(GameSettingsCommandStatus.InvalidSetting));
    public Task<GameSettingsCommandResult> SetPromptLanguageAsync(
        string gameHash, string token, string language, CancellationToken cancellationToken = default) =>
        GameSettingsPolicy.IsPromptLanguageValid(language)
            ? UpdateAsync(gameHash, token, s => s.PromptLanguage = language, cancellationToken)
            : Task.FromResult(new GameSettingsCommandResult(GameSettingsCommandStatus.InvalidSetting));
    public Task<GameSettingsCommandResult> SetMaxPlayersAsync(
        string gameHash, string token, int maxPlayers, CancellationToken cancellationToken = default) =>
        GameSettingsPolicy.IsMaxPlayersValid(maxPlayers, _options)
            ? UpdateAsync(gameHash, token, s => s.MaxPlayers = maxPlayers, cancellationToken)
            : Task.FromResult(new GameSettingsCommandResult(GameSettingsCommandStatus.InvalidSetting));
    public Task<GameSettingsCommandResult> SetHintSpeedAsync(
        string gameHash, string token, string speed, CancellationToken cancellationToken = default) =>
        GameSettingsPolicy.IsHintSpeedValid(speed)
            ? UpdateAsync(gameHash, token, s => s.HintSpeed = speed, cancellationToken)
            : Task.FromResult(new GameSettingsCommandResult(GameSettingsCommandStatus.InvalidSetting));
    public Task<GameSettingsCommandResult> SetHintPercentageAsync(
        string gameHash, string token, int percentage, CancellationToken cancellationToken = default) =>
        GameSettingsPolicy.IsHintPercentageValid(percentage, _options)
            ? UpdateAsync(gameHash, token, s => s.HintPercentage = percentage, cancellationToken)
            : Task.FromResult(new GameSettingsCommandResult(GameSettingsCommandStatus.InvalidSetting));

    public async Task<GameSettingsCommandResult> SetPromptCategoriesAsync(
        string gameHash, string token, string[] categories, CancellationToken cancellationToken = default)
    {
        List<string> normalized = (categories ?? Array.Empty<string>())
            .Select(c => (c ?? string.Empty).Trim().ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (!GameSettingsPolicy.IsPromptCategorySelectionValid(normalized, _options))
        {
            return new(GameSettingsCommandStatus.InvalidSetting);
        }
        Game? snapshot = await _store.GetSnapshotAsync(gameHash, cancellationToken);

        if (snapshot is null)
        {
            return new(GameSettingsCommandStatus.GameNotFound);
        }
        IReadOnlyCollection<string> known = await _prompts.GetCategoriesAsync(snapshot.GameSettings.PromptLanguage, cancellationToken);

        if (normalized.Any(c => !known.Contains(c, StringComparer.Ordinal)))
        {
            return new(GameSettingsCommandStatus.InvalidCategories);
        }
        return await UpdateAsync(gameHash, token, s => s.PromptCategories = normalized, cancellationToken);
    }

    public async Task<GameSettings?> GetAsync(
        string gameHash,
        string roomAccessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roomAccessToken))
        {
            return null;
        }
        string hash = _credentials.ComputeCredentialHash(roomAccessToken);

        Game? game = await _store.GetSnapshotAsync(gameHash, cancellationToken);

        if (game is null || game.GameState.Players.All(p => p.CredentialHash != hash))
        {
            return null;
        }
        return Copy(game.GameSettings);
    }

    private async Task<GameSettingsCommandResult> UpdateAsync(
        string gameHash, string roomAccessToken, Action<GameSettings> update, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(roomAccessToken))
        {
            return new(GameSettingsCommandStatus.Unauthorized);
        }
        string hash = _credentials.ComputeCredentialHash(roomAccessToken);

        GameStoreResult<GameSettingsCommandResult> result = await _store.MutateAsync(gameHash, game =>
        {
            if (game.HostCredentialHash != hash || game.GameState.Players.All(p => p.CredentialHash != hash))
            {
                return GameMutation<GameSettingsCommandResult>.NoChange(new(GameSettingsCommandStatus.Unauthorized));
            }

            if (game.GameState.IsGameStarted)
            {
                return GameMutation<GameSettingsCommandResult>.NoChange(new(GameSettingsCommandStatus.GameAlreadyStarted));
            }
            update(game.GameSettings);
            return GameMutation<GameSettingsCommandResult>.Persist(new(GameSettingsCommandStatus.Applied, Copy(game.GameSettings)));
        }, cancellationToken);
        return result.Found ? result.Value! : new(GameSettingsCommandStatus.GameNotFound);
    }

    private GameSettings Copy(GameSettings s) => new()
    {
        DrawingTimeSeconds = s.DrawingTimeSeconds,
        RoundsCount = s.RoundsCount,
        PromptLanguage = s.PromptLanguage,
        MaxPlayers = s.MaxPlayers,
        PromptCategories = new List<string>(s.PromptCategories),
        HintSpeed = s.HintSpeed,
        HintPercentage = GameSettingsPolicy.NormalizeHintPercentage(s.HintPercentage, _options)
    };
}
