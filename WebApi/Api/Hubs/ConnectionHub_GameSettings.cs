using WebApi.Api.Hubs.Static;
using WebApi.Api.Utilities;
using WebApi.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using WebApi.Api.Hubs.Attributes;

namespace WebApi.Hubs;

public partial class HubConnection
{
    [HubMethodName(HubMessages.SetDrawingTimeSeconds)]
    [ValidateHubArgument("gameHash", ValidationType.GameHash)]
    [ValidateHubArgument("token", ValidationType.HostToken)]

    public async Task SetDrawingTimeSeconds(string gameHash, string token, int setting)
    {
        Game game = (Game)Context.Items["Game"]!;
        GameSettings settings = game.GameSettings;

        settings.DrawingTimeSeconds = setting;
        game.GameSettings = settings;

        await Clients.Group(gameHash).SendAsync(HubMessages.OnSetDrawingTimeSeconds, setting);

        _logger.LogInformation($"Game #{gameHash} SetDrawingTimeSeconds: Setting set to {setting}");
    }

    [HubMethodName(HubMessages.SetRoundsCount)]
    [ValidateHubArgument("gameHash", ValidationType.GameHash)]
    [ValidateHubArgument("token", ValidationType.HostToken)]
    public async Task SetRoundsCount(string gameHash, string token, int setting)
    {
        Game game = (Game)Context.Items["Game"]!;
        GameSettings settings = game.GameSettings;

        settings.RoundsCount = setting;
        game.GameSettings = settings;

        await Clients.Group(gameHash).SendAsync(HubMessages.OnSetRoundsCount, setting);

        _logger.LogInformation($"Game #{gameHash} SetRoundsCount: Setting set to {setting}");
    }

    [HubMethodName(HubMessages.SetWordLanguage)]
    [ValidateHubArgument("gameHash", ValidationType.GameHash)]
    [ValidateHubArgument("token", ValidationType.HostToken)]
    public async Task SetWordLanguageSetting(string gameHash, string token, string setting)
    {
        Game game = (Game)Context.Items["Game"]!;
        GameSettings settings = game.GameSettings;

        settings.WordLanguage = setting;
        game.GameSettings = settings;

        await Clients.Group(gameHash).SendAsync(HubMessages.OnSetWordLanguage, setting);

        _logger.LogInformation($"Game #{gameHash} SetWordLanguageSetting: Setting set to {setting}");
    }

    [HubMethodName(HubMessages.LoadGameSettings)]
    [ValidateHubArgument("gameHash", ValidationType.GameHash)]
    [ValidateHubArgument("token", ValidationType.PlayerToken)]
    public async Task LoadGameSettings(string gameHash, string token)
    {
        _logger.LogInformation($"Hub Context Hash: {Context.GetHashCode()}");
        Game game = (Game)Context.Items["Game"]!;

        await Clients
            .Client(Context.ConnectionId)
            .SendAsync(HubMessages.OnLoadGameSettings, JsonHelper.Serialize(game.GameSettings));

        _logger.LogInformation($"Game #{gameHash} LoadGameSettings: Settings loaded");
    }
}