using Scribbly.Domain.Entities;
using Scribbly.Application.Features.Settings.Configuration;

namespace Scribbly.Application.Features.Settings;

public static class GameSettingsPolicy
{
    public static bool IsDrawingTimeValid(int seconds, GameSettingsOptions options) =>
        seconds >= options.MinDrawingTimeSeconds &&
        seconds <= options.MaxDrawingTimeSeconds &&
        (seconds - options.MinDrawingTimeSeconds) % options.DrawingTimeStepSeconds == 0;

    public static bool IsRoundsCountValid(int rounds, GameSettingsOptions options) =>
        rounds >= options.MinRounds && rounds <= options.MaxRounds;

    public static bool IsPromptLanguageValid(string? language) => language is "en" or "pl";

    public static bool IsMaxPlayersValid(int maxPlayers, GameSettingsOptions options) =>
        maxPlayers >= options.MinPlayers && maxPlayers <= options.MaxPlayers;

    public static bool IsHintSpeedValid(string? speed) => speed is "off" or "slow" or "normal" or "fast";

    public static bool IsHintPercentageValid(int percentage, GameSettingsOptions options) =>
        percentage >= options.MinHintPercentage &&
        percentage <= options.MaxHintPercentage &&
        (percentage - options.MinHintPercentage) % options.HintPercentageStep == 0;

    public static int NormalizeHintPercentage(int percentage, GameSettingsOptions options) =>
        Math.Clamp(
            (percentage - options.MinHintPercentage) / options.HintPercentageStep *
                options.HintPercentageStep + options.MinHintPercentage,
            options.MinHintPercentage,
            options.MaxHintPercentage);

    public static bool IsPromptCategorySelectionValid(
        IReadOnlyCollection<string>? categories,
        GameSettingsOptions options) =>
        categories is null || categories.All(category => IsPromptCategorySlugValid(category, options));

    public static bool IsPromptCategorySlugValid(string? category, GameSettingsOptions options) =>
        !string.IsNullOrEmpty(category) && category.Length <= options.MaxPromptCategoryLength &&
        category.All(c => char.IsLetter(c) || c is '-' or ' ');

    public static bool IsValid(GameSettings? settings, GameSettingsOptions options) => settings is not null &&
        IsDrawingTimeValid(settings.DrawingTimeSeconds, options) &&
        IsRoundsCountValid(settings.RoundsCount, options) &&
        IsPromptLanguageValid(settings.PromptLanguage) &&
        IsMaxPlayersValid(settings.MaxPlayers, options) &&
        IsPromptCategorySelectionValid(settings.PromptCategories, options) &&
        IsHintSpeedValid(settings.HintSpeed) &&
        IsHintPercentageValid(settings.HintPercentage, options);
}
