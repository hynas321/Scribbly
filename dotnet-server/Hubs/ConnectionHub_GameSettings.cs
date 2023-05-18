using Dotnet.Server.JsonConfig;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubEvents.SetAbstractNouns)]
    public async Task SetAbstractNouns(string gameHash, string token, bool setting)
    {
        try
        {
            Game game = gameManager.GetGame(gameHash);

            if (game == null)
            {
                logger.LogError($"Game #{gameHash} SetAbstractNouns: Game does not exist");
                return;
            }

            if (token != game.HostToken)
            {
                logger.LogError($"Game #{gameHash} SetAbstractNouns: Player with the token {token} is not a host");
                return;
            }

            GameSettings settings = game.GameSettings;

            settings.NounsOnly = setting;
            game.GameSettings = settings;

            await Clients.Group(gameHash).SendAsync(HubEvents.OnSetAbstractNouns, setting);

            logger.LogInformation($"Game #{gameHash} SetAbstractNouns: Setting set to {setting}");
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.SetDrawingTimeSeconds)]
    public async Task SetDrawingTimeSeconds(string gameHash, string token, int setting)
    {
        try
        {
            Game game = gameManager.GetGame(gameHash);

            if (game == null)
            {
                logger.LogError($"Game #{gameHash} SetDrawingTimeSeconds: Game does not exist");
                return;
            }

            if (token != game.HostToken)
            {
                logger.LogError($"Game #{gameHash} SetDrawingTimeSeconds: Player with the token {token} is not a host");
                return;
            }

            GameSettings settings = game.GameSettings;

            settings.DrawingTimeSeconds = setting;
            game.GameSettings = settings;

            await Clients.Group(gameHash).SendAsync(HubEvents.OnSetDrawingTimeSeconds, setting);

            logger.LogInformation($"Game #{gameHash} SetDrawingTimeSeconds: Setting set to {setting}");
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.SetRoundsCount)]
    public async Task SetRoundsCount(string gameHash, string token, int setting)
    {
        try
        {
            Game game = gameManager.GetGame(gameHash);

            if (game == null)
            {
                logger.LogError($"Game #{gameHash} SetRoundsCount: Game does not exist");
                return;
            }

            if (token != game.HostToken)
            {
                logger.LogError($"Game #{gameHash} SetRoundsCount: Player with the token {token} is not a host");
                return;
            }

            GameSettings settings = game.GameSettings;

            settings.RoundsCount = setting;
            game.GameSettings = settings;

            await Clients.Group(gameHash).SendAsync(HubEvents.OnSetRoundsCount, setting);

            logger.LogInformation($"Game #{gameHash} SetRoundsCount: Setting set to {setting}");
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.SetWordLanguage)]
    public async Task SetWordLanguageSetting(string gameHash, string token, string setting)
    {
        try
        {
            Game game = gameManager.GetGame(gameHash);

            if (game == null)
            {
                logger.LogError($"Game #{gameHash} SetWordLanguageSetting: Game does not exist");
                return;
            }

            if (token != game.HostToken)
            {
                logger.LogError($"Game #{gameHash} SetWordLanguageSetting: Player with the token {token} is not a host");
                return;
            }

            GameSettings settings = game.GameSettings;

            settings.WordLanguage = setting;
            game.GameSettings = settings;

            await Clients.Group(gameHash).SendAsync(HubEvents.OnSetWordLanguage, setting);

            logger.LogInformation($"Game #{gameHash} SetWordLanguageSetting: Setting set to {setting}");
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.LoadGameSettings)]
    public async Task LoadGameSettings(string gameHash, string token)
    {
        try
        {
            Game game = gameManager.GetGame(gameHash);

            if (game == null)
            {
                logger.LogError($"Game #{gameHash} LoadGameSettings: Game does not exist");
                return;
            }

            bool playerExists = gameManager.CheckIfPlayerExistsByToken(gameHash, token);

            if (!playerExists)
            {
                logger.LogError($"Game #{gameHash} LoadGameSettings: Player with the token {token} does not exist");
                return;
            }

            GameSettings settings = game.GameSettings;

            await Clients
                .Client(Context.ConnectionId)
                .SendAsync(HubEvents.OnLoadGameSettings, JsonHelper.Serialize(settings));

            logger.LogInformation($"Game #{gameHash} LoadGameSettings: Settings loaded");
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }
}