using Dotnet.Server.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubEvents.SetAbstractNouns)]
    public async Task SetAbstractNouns(string token, object setting)
    {
        try
        {
            if (setting is not bool)
            {
                return;
            }

            Game game = gameManager.GetGame();

            if (game == null)
            {
                return;
            }

            if (token != game.HostToken)
            {
                return;
            }

            GameSettings settings = game.GameSettings;

            settings.NonAbstractNounsOnly = (bool)setting;
            game.GameSettings = settings;

            await Clients.All.SendAsync(HubEvents.OnSetAbstractNouns, (bool)setting);
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.SetDrawingTimeSeconds)]
    public async Task SetDrawingTimeSeconds(string token, object setting)
    {
        try
        {
            if (setting is not int)
            {
                return;
            }

            Game game = gameManager.GetGame();

            if (game == null)
            {
                return;
            }

            if (token != game.HostToken)
            {
                return;
            }

            GameSettings settings = game.GameSettings;

            settings.DrawingTimeSeconds = (int)setting;
            game.GameSettings = settings;

            await Clients.All.SendAsync(HubEvents.OnSetDrawingTimeSeconds, (int)setting);
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.SetRoundsCount)]
    public async Task SetRoundsCount(string token, object setting)
    {
        try
        {
            if (setting is not int)
            {
                return;
            }

            Game game = gameManager.GetGame();

            if (game == null)
            {
                return;
            }

            if (token != game.HostToken)
            {
                return;
            }

            GameSettings settings = game.GameSettings;

            settings.RoundsCount = (int)setting;
            game.GameSettings = settings;

            await Clients.All.SendAsync(HubEvents.OnSetRoundsCount, (int)setting);
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.SetWordLanguage)]
    public async Task ChangeWordLanguageSetting(string token, object setting)
    {
        try
        {
            if (setting is not string)
            {
                return;
            }

            Game game = gameManager.GetGame();

            if (game == null)
            {
                return;
            }

            if (token != game.HostToken)
            {
                return;
            }

            GameSettings settings = game.GameSettings;

            settings.WordLanguage = (string)setting;
            game.GameSettings = settings;

            await Clients.All.SendAsync(HubEvents.OnSetWordLanguage, (string)setting);
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.LoadGameSettings)]
    public async Task LoadGameSettings(string token)
    {
        try
        {
            Game game = gameManager.GetGame();
            if (game == null)
            {
                return;
            }

            GameSettings settings = game.GameSettings;

            await Clients
                .Client(Context.ConnectionId)
                .SendAsync(HubEvents.OnSetRoundsCount, JsonHelper.Serialize(settings));
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }
}