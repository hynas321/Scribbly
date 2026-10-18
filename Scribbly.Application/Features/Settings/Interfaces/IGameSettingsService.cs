using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Settings;

public interface IGameSettingsService
{
    Task<GameSettingsCommandResult> SetDrawingTimeAsync(
        string gameHash, string roomAccessToken, int seconds, CancellationToken cancellationToken = default);
    Task<GameSettingsCommandResult> SetRoundsCountAsync(
        string gameHash, string roomAccessToken, int rounds, CancellationToken cancellationToken = default);
    Task<GameSettingsCommandResult> SetPromptLanguageAsync(
        string gameHash, string roomAccessToken, string language, CancellationToken cancellationToken = default);
    Task<GameSettingsCommandResult> SetMaxPlayersAsync(
        string gameHash, string roomAccessToken, int maxPlayers, CancellationToken cancellationToken = default);
    Task<GameSettingsCommandResult> SetHintSpeedAsync(
        string gameHash, string roomAccessToken, string speed, CancellationToken cancellationToken = default);
    Task<GameSettingsCommandResult> SetHintPercentageAsync(
        string gameHash, string roomAccessToken, int percentage, CancellationToken cancellationToken = default);
    Task<GameSettingsCommandResult> SetPromptCategoriesAsync(
        string gameHash, string roomAccessToken, string[] categories, CancellationToken cancellationToken = default);
    Task<GameSettings?> GetAsync(string gameHash, string roomAccessToken, CancellationToken cancellationToken = default);
}
