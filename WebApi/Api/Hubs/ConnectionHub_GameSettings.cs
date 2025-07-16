using WebApi.Api.Hubs.Static;
using WebApi.Api.Utilities;
using WebApi.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.Hubs;

public partial class HubConnection
{
    [HubMethodName(HubMessages.SetDrawingTimeSeconds)]
    public async Task SetDrawingTimeSeconds(string gameHash, string token, int setting)
    {
        try
        {
            Game game = _gameManager.GetGame(gameHash);

            if (game is null)
            {
                _logger.LogError($"Game #{gameHash} SetDrawingTimeSeconds: Game does not exist");
                return;
            }

            if (token != game.HostToken)
            {
                _logger.LogError($"Game #{gameHash} SetDrawingTimeSeconds: Player with the token {token} is not a host");
                return;
            }

            GameSettings settings = game.GameSettings;

            settings.DrawingTimeSeconds = setting;
            game.GameSettings = settings;

            await Clients.Group(gameHash).SendAsync(HubMessages.OnSetDrawingTimeSeconds, setting);

            _logger.LogInformation($"Game #{gameHash} SetDrawingTimeSeconds: Setting set to {setting}");
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubMessages.SetRoundsCount)]
    public async Task SetRoundsCount(string gameHash, string token, int setting)
    {
        try
        {
            Game game = _gameManager.GetGame(gameHash);

            if (game is null)
            {
                _logger.LogError($"Game #{gameHash} SetRoundsCount: Game does not exist");
                return;
            }

            if (token != game.HostToken)
            {
                _logger.LogError($"Game #{gameHash} SetRoundsCount: Player with the token {token} is not a host");
                return;
            }

            GameSettings settings = game.GameSettings;

            settings.RoundsCount = setting;
            game.GameSettings = settings;

            await Clients.Group(gameHash).SendAsync(HubMessages.OnSetRoundsCount, setting);

            _logger.LogInformation($"Game #{gameHash} SetRoundsCount: Setting set to {setting}");
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubMessages.SetWordLanguage)]
    public async Task SetWordLanguageSetting(string gameHash, string token, string setting)
    {
        Game game = _gameManager.GetGame(gameHash);

        if (game is null)
        {
            _logger.LogError($"Game #{gameHash} SetWordLanguageSetting: Game does not exist");
            return;
        }

        if (token != game.HostToken)
        {
            _logger.LogError($"Game #{gameHash} SetWordLanguageSetting: Player with the token {token} is not a host");
            return;
        }

        GameSettings settings = game.GameSettings;

        settings.WordLanguage = setting;
        game.GameSettings = settings;

        await Clients.Group(gameHash).SendAsync(HubMessages.OnSetWordLanguage, setting);

        _logger.LogInformation($"Game #{gameHash} SetWordLanguageSetting: Setting set to {setting}");
    }

    [HubMethodName(HubMessages.LoadGameSettings)]
    public async Task LoadGameSettings(string gameHash, string token)
    {
        Game game = _gameManager.GetGame(gameHash);

        if (game is null)
        {
            _logger.LogError($"Game #{gameHash} LoadGameSettings: Game does not exist");
            return;
        }

        bool playerExists = _playerManager.CheckIfPlayerExistsByToken(gameHash, token);

        if (!playerExists)
        {
            _logger.LogError($"Game #{gameHash} LoadGameSettings: Player with the token {token} does not exist");
            return;
        }

        GameSettings settings = game.GameSettings;

        await Clients
            .Client(Context.ConnectionId)
            .SendAsync(HubMessages.OnLoadGameSettings, JsonHelper.Serialize(settings));

        _logger.LogInformation($"Game #{gameHash} LoadGameSettings: Settings loaded");
    }
}