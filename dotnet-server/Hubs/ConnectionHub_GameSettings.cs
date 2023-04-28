using System.Text.Json;
using Dotnet.Server.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubEvents.SetAbstractNouns)]
    public async Task SetAbstractNouns(string lobbyHash, bool on)
    {
        try
        {
            GameSettings settings = gamesManager.GetGameSettings(lobbyHash);

            settings.NonAbstractNounsOnly = on;
            gamesManager.ChangeGameSettings(lobbyHash, settings);

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
            GameSettings settings = gamesManager.GetGameSettings(lobbyHash);

            settings.DrawingTimeSeconds = time;
            gamesManager.ChangeGameSettings(lobbyHash, settings);

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
            GameSettings settings = gamesManager.GetGameSettings(lobbyHash);

            settings.RoundsCount = count;
            gamesManager.ChangeGameSettings(lobbyHash, settings);

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
            GameSettings settings = gamesManager.GetGameSettings(lobbyHash);

            settings.WordLanguage = language;
            gamesManager.ChangeGameSettings(lobbyHash, settings);

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
            GameSettings settings = gamesManager.GetGameSettings(lobbyHash);
            string gameSettingsSerialized = JsonHelper.Serialize(settings);

            await Clients.All.SendAsync(HubEvents.OnLoadGameSettings, gameSettingsSerialized);
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Lobby #{lobbyHash}: Player {username} could not load the game settings. {ex}");
        }
    }
}