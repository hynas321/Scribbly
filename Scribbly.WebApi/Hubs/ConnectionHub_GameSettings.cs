using Microsoft.AspNetCore.SignalR;
using Scribbly.Application.Features.Settings;
using Scribbly.WebApi.Hubs.Static;

namespace Scribbly.WebApi.Hubs;

public partial class HubConnection
{
    [HubMethodName(HubMessages.SetDrawingTimeSeconds)]
    public async Task SetDrawingTimeSeconds(string gameHash, int value)
    {
        GameSettingsCommandResult result = await _settings.SetDrawingTimeAsync(gameHash, RequireRoomAccessToken(), value, Context.ConnectionAborted);
        EnsureSettings(result);
        await Clients.Group(gameHash).SendAsync(HubMessages.OnDrawingTimeChanged, result.Settings!.DrawingTimeSeconds, Context.ConnectionAborted);
    }

    [HubMethodName(HubMessages.SetRoundsCount)]
    public async Task SetRoundsCount(string gameHash, int value)
    {
        GameSettingsCommandResult result = await _settings.SetRoundsCountAsync(gameHash, RequireRoomAccessToken(), value, Context.ConnectionAborted);
        EnsureSettings(result);
        await Clients.Group(gameHash).SendAsync(HubMessages.OnRoundsCountChanged, result.Settings!.RoundsCount, Context.ConnectionAborted);
    }

    [HubMethodName(HubMessages.SetPromptLanguage)]
    public async Task SetPromptLanguageSetting(string gameHash, string value)
    {
        GameSettingsCommandResult result = await _settings.SetPromptLanguageAsync(
            gameHash, RequireRoomAccessToken(), value, Context.ConnectionAborted);
        EnsureSettings(result);
        await Clients.Group(gameHash).SendAsync(HubMessages.OnPromptLanguageChanged, result.Settings!.PromptLanguage, Context.ConnectionAborted);
    }

    [HubMethodName(HubMessages.SetMaxPlayers)]
    public async Task SetMaxPlayers(string gameHash, int value)
    {
        GameSettingsCommandResult result = await _settings.SetMaxPlayersAsync(gameHash, RequireRoomAccessToken(), value, Context.ConnectionAborted);
        EnsureSettings(result);
        await Clients.Group(gameHash).SendAsync(HubMessages.OnMaxPlayersChanged, result.Settings!.MaxPlayers, Context.ConnectionAborted);
    }

    [HubMethodName(HubMessages.SetHintSpeed)]
    public async Task SetHintSpeed(string gameHash, string value)
    {
        GameSettingsCommandResult result = await _settings.SetHintSpeedAsync(
            gameHash, RequireRoomAccessToken(), value, Context.ConnectionAborted);
        EnsureSettings(result);
        await Clients.Group(gameHash).SendAsync(
            HubMessages.OnHintSpeedChanged, result.Settings!.HintSpeed, Context.ConnectionAborted);
    }

    [HubMethodName(HubMessages.SetHintPercentage)]
    public async Task SetHintPercentage(string gameHash, int value)
    {
        GameSettingsCommandResult result = await _settings.SetHintPercentageAsync(
            gameHash, RequireRoomAccessToken(), value, Context.ConnectionAborted);
        EnsureSettings(result);
        await Clients.Group(gameHash).SendAsync(
            HubMessages.OnHintPercentageChanged, result.Settings!.HintPercentage, Context.ConnectionAborted);
    }

    [HubMethodName(HubMessages.SetPromptCategories)]
    public async Task SetPromptCategories(string gameHash, string[] categories)
    {
        GameSettingsCommandResult result = await _settings.SetPromptCategoriesAsync(
            gameHash, RequireRoomAccessToken(), categories, Context.ConnectionAborted);
        EnsureSettings(result);
        await Clients.Group(gameHash).SendAsync(
            HubMessages.OnPromptCategoriesChanged, result.Settings!.PromptCategories.ToArray(), Context.ConnectionAborted);
    }

    private static void EnsureSettings(GameSettingsCommandResult result)
    {
        if (result.Status == GameSettingsCommandStatus.Applied)
        {
            return;
        }
        throw new HubException(result.Status switch
        {
            GameSettingsCommandStatus.InvalidSetting => "Invalid game setting.",
            GameSettingsCommandStatus.InvalidCategories => "Unknown prompt categories.",
            GameSettingsCommandStatus.GameAlreadyStarted => "Settings cannot be changed after the game starts.",
            GameSettingsCommandStatus.GameNotFound => "Game does not exist.",
            _ => "Only the host may change game settings."
        });
    }
}
