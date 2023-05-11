using Dotnet.Server.JsonConfig;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubEvents.SetAbstractNouns)]
    public async Task SetAbstractNouns(string token, bool setting)
    {
        try
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                logger.LogError($"SetAbstractNouns: Game does not exist");
                return;
            }

            if (token != game.HostToken)
            {
                logger.LogError($"SetAbstractNouns: Player with the token {token} is not a host");
                return;
            }

            GameSettings settings = game.GameSettings;

            settings.NonAbstractNounsOnly = setting;
            game.GameSettings = settings;

            await Clients.All.SendAsync(HubEvents.OnSetAbstractNouns, setting);

            logger.LogInformation($"SetAbstractNouns: Setting set to {setting}");
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.SetDrawingTimeSeconds)]
    public async Task SetDrawingTimeSeconds(string token, int setting)
    {
        try
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                logger.LogError($"SetDrawingTimeSeconds: Game does not exist");
                return;
            }

            if (token != game.HostToken)
            {
                logger.LogError($"SetDrawingTimeSeconds: Player with the token {token} is not a host");
                return;
            }

            GameSettings settings = game.GameSettings;

            settings.DrawingTimeSeconds = setting;
            game.GameSettings = settings;

            await Clients.All.SendAsync(HubEvents.OnSetDrawingTimeSeconds, setting);

            logger.LogInformation($"SetDrawingTimeSeconds: Setting set to {setting}");
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.SetRoundsCount)]
    public async Task SetRoundsCount(string token, int setting)
    {
        try
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                logger.LogError($"SetRoundsCount: Game does not exist");
                return;
            }

            if (token != game.HostToken)
            {
                logger.LogError($"SetRoundsCount: Player with the token {token} is not a host");
                return;
            }

            GameSettings settings = game.GameSettings;

            settings.RoundsCount = setting;
            game.GameSettings = settings;

            await Clients.All.SendAsync(HubEvents.OnSetRoundsCount, setting);

            logger.LogInformation($"SetRoundsCount: Setting set to {setting}");
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.SetWordLanguage)]
    public async Task SetWordLanguageSetting(string token, string setting)
    {
        try
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                logger.LogError($"SetWordLanguageSetting: Game does not exist");
                return;
            }

            if (token != game.HostToken)
            {
                logger.LogError($"SetWordLanguageSetting: Player with the token {token} is not a host");
                return;
            }

            GameSettings settings = game.GameSettings;

            settings.WordLanguage = setting;
            game.GameSettings = settings;

            await Clients.All.SendAsync(HubEvents.OnSetWordLanguage, setting);

            logger.LogInformation($"SetWordLanguageSetting: Setting set to {setting}");
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
                logger.LogError($"LoadGameSettings: Game does not exist");
                return;
            }

            bool playerExists = gameManager.CheckIfPlayerExistsByToken(token);

            if (!playerExists)
            {
                logger.LogError($"LoadGameSettings: Player with the token {token} does not exist");
                return;
            }

            GameSettings settings = game.GameSettings;

            await Clients
                .Client(Context.ConnectionId)
                .SendAsync(HubEvents.OnLoadGameSettings, JsonHelper.Serialize(settings));

            logger.LogInformation($"LoadGameSettings: Settings loaded");
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }
}