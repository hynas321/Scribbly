using System.Text.Json;
using Dotnet.Server.Managers;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class LobbyHub : Hub
{
    [HubMethodName("ChangeAbstractNounsSetting")]
    public async Task ChangeAbstractNounsSettingAsync(string lobbyHash, bool on)
    {
        GameSettings settings = lobbyManager.GetGameSettings(lobbyHash);

        settings.NonAbstractNounsOnly = on;
        lobbyManager.ChangeGameSettings(lobbyHash, settings);

        logger.LogInformation($"Abstract nouns setting set to {Convert.ToString(settings.NonAbstractNounsOnly)}");
        await Clients.All.SendAsync("ApplyAbstractNounsSetting", on);
    }

    [HubMethodName("ChangeDrawingTimeSetting")]
    public async Task ChangeDrawingTimeSetting(string lobbyHash, int time)
    {
        GameSettings settings = lobbyManager.GetGameSettings(lobbyHash);

        settings.DrawingTimeSeconds = time;
        lobbyManager.ChangeGameSettings(lobbyHash, settings);

        logger.LogInformation($"Drawing time set to {Convert.ToString(settings.DrawingTimeSeconds)}");
        await Clients.All.SendAsync("ApplyDrawingTimeSetting", time);
    }

    [HubMethodName("ChangeRoundsCountSetting")]
    public async Task ChangeRoundsCountSetting(string lobbyHash, int count)
    {
        GameSettings settings = lobbyManager.GetGameSettings(lobbyHash);

        settings.RoundsCount = count;
        lobbyManager.ChangeGameSettings(lobbyHash, settings);

        logger.LogInformation($"Rounds count set to {Convert.ToString(settings.RoundsCount)}");
        await Clients.All.SendAsync("ApplyRoundsCountSetting", count);
    }

    [HubMethodName("ChangeWordLanguageSetting")]
    public async Task ChangeWordLanguageSetting(string lobbyHash, string language)
    {
        GameSettings settings = lobbyManager.GetGameSettings(lobbyHash);

        settings.WordLanguage = language;
        lobbyManager.ChangeGameSettings(lobbyHash, settings);

        logger.LogInformation($"Word language set to {Convert.ToString(settings.WordLanguage)}");
        await Clients.All.SendAsync("ApplyWordLanguageSetting", language);
    }

    [HubMethodName("GetGameSettings")]
    public async Task GetGameSettings(string lobbyHash)
    {
        GameSettings settings = lobbyManager.GetGameSettings(lobbyHash);
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string gameSettingsSerialized = JsonSerializer.Serialize(settings, jsonSerializerOptions);
        await Clients.All.SendAsync("ApplyGameSettings", gameSettingsSerialized);
    }
}