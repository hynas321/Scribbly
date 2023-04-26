using System.Text.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class LobbyHub : Hub
{
    [HubMethodName(HubEvents.SetAbstractNouns)]
    public async Task SetAbstractNouns(string lobbyHash, bool on)
    {
        try
        {
            GameSettings settings = lobbiesManager.GetGameSettings(lobbyHash);

            settings.NonAbstractNounsOnly = on;
            lobbiesManager.ChangeGameSettings(lobbyHash, settings);

            await Clients.All.SendAsync(HubEvents.OnSetAbstractNouns, on);
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Lobby #{lobbyHash}: Abstract nouns setting could not be set. {ex}");
        }
    }

    [HubMethodName(HubEvents.SetDrawingTimeSeconds)]
    public async Task SetDrawingTimeSeconds(string lobbyHash, int time)
    {
        try
        {
            GameSettings settings = lobbiesManager.GetGameSettings(lobbyHash);

            settings.DrawingTimeSeconds = time;
            lobbiesManager.ChangeGameSettings(lobbyHash, settings);

            await Clients.All.SendAsync(HubEvents.OnSetDrawingTimeSeconds, time);
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Lobby #{lobbyHash}: Drawing time could not be set. {ex}");
        }
    }

    [HubMethodName(HubEvents.SetRoundsCount)]
    public async Task SetRoundsCount(string lobbyHash, int count)
    {
        try
        {
            GameSettings settings = lobbiesManager.GetGameSettings(lobbyHash);

            settings.RoundsCount = count;
            lobbiesManager.ChangeGameSettings(lobbyHash, settings);

            await Clients.All.SendAsync(HubEvents.OnSetRoundsCount, count);
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Lobby #{lobbyHash}: Rounds count could not be set. {ex}");
        }
    }

    [HubMethodName(HubEvents.SetWordLanguage)]
    public async Task ChangeWordLanguageSetting(string lobbyHash, string language)
    {
        try
        {
            GameSettings settings = lobbiesManager.GetGameSettings(lobbyHash);

            settings.WordLanguage = language;
            lobbiesManager.ChangeGameSettings(lobbyHash, settings);

            await Clients.All.SendAsync(HubEvents.OnSetWordLanguage, language);
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Lobby #{lobbyHash}: Word language could not be set. {ex}");
        }
    }

    [HubMethodName(HubEvents.LoadGameSettings)]
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

            await Clients.All.SendAsync(HubEvents.OnLoadGameSettings, gameSettingsSerialized);
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Lobby #{lobbyHash}: Player {username} could not load the game settings. {ex}");
        }
    }
}