using System.Text.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class LobbyHub : Hub
{
    [HubMethodName("ChangeAbstractNounsSetting")]
    public async Task ChangeAbstractNounsSettingAsync(string lobbyHash, bool on)
    {
        try
        {
            GameSettings settings = lobbiesManager.GetGameSettings(lobbyHash);

            settings.NonAbstractNounsOnly = on;
            lobbiesManager.ChangeGameSettings(lobbyHash, settings);

            await Clients.All.SendAsync("ApplyAbstractNounsSetting", on);

            logger.LogInformation($"Lobby #{lobbyHash}: Abstract nouns setting set to {Convert.ToString(settings.NonAbstractNounsOnly)}.");
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Lobby #{lobbyHash}: Abstract nouns setting could not be set. {ex}");
        }
    }

    [HubMethodName("ChangeDrawingTimeSetting")]
    public async Task ChangeDrawingTimeSetting(string lobbyHash, int time)
    {
        try
        {
            GameSettings settings = lobbiesManager.GetGameSettings(lobbyHash);

            settings.DrawingTimeSeconds = time;
            lobbiesManager.ChangeGameSettings(lobbyHash, settings);

            await Clients.All.SendAsync("ApplyDrawingTimeSetting", time);

            logger.LogInformation($"Lobby #{lobbyHash}: Drawing time set to {Convert.ToString(settings.DrawingTimeSeconds)}.");
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Lobby #{lobbyHash}: Drawing time could not be set. {ex}");
        }
    }

    [HubMethodName("ChangeRoundsCountSetting")]
    public async Task ChangeRoundsCountSetting(string lobbyHash, int count)
    {
        try
        {
            GameSettings settings = lobbiesManager.GetGameSettings(lobbyHash);

            settings.RoundsCount = count;
            lobbiesManager.ChangeGameSettings(lobbyHash, settings);

            await Clients.All.SendAsync("ApplyRoundsCountSetting", count);

            logger.LogInformation($"Lobby #{lobbyHash}: Rounds count set to {Convert.ToString(settings.RoundsCount)}.");
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Lobby #{lobbyHash}: Rounds count could not be set. {ex}");
        }
    }

    [HubMethodName("ChangeWordLanguageSetting")]
    public async Task ChangeWordLanguageSetting(string lobbyHash, string language)
    {
        try
        {
            GameSettings settings = lobbiesManager.GetGameSettings(lobbyHash);

            settings.WordLanguage = language;
            lobbiesManager.ChangeGameSettings(lobbyHash, settings);

            await Clients.All.SendAsync("ApplyWordLanguageSetting", language);

            logger.LogInformation($"Lobby #{lobbyHash}: Word language set to {Convert.ToString(settings.WordLanguage)}.");
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Lobby #{lobbyHash}: Word language could not be set. {ex}");
        }
    }

    [HubMethodName("GetGameSettings")]
    public async Task GetGameSettings(string lobbyHash, string username)
    {
        try
        {
            GameSettings settings = lobbiesManager.GetGameSettings(lobbyHash);
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string gameSettingsSerialized = JsonSerializer.Serialize(settings, jsonSerializerOptions);
            await Clients.All.SendAsync("ApplyGameSettings", gameSettingsSerialized);
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Lobby #{lobbyHash}: Player {username} could not load the game settings. {ex}");
        }
    }
}