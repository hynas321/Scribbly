using dotnet_server.Api.Hubs.Static;
using dotnet_server.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubMessages.GetSecretWord)]
    public async Task GetSecretWord(string gameHash, string token)
    {
        try
        {
            Game game = _gameManager.GetGame(gameHash);

            if (game == null)
            {
                _logger.LogError($"Game #{gameHash} GetSecretWord: Game does not exist");
                return;
            }

            Player player = _playerManager.GetPlayerByToken(gameHash, token);

            if (player == null)
            {
                _logger.LogError($"Game #{gameHash} GetSecretWord: Player with the token {token} does not exist");
                return;
            }

            string secretWordMessage = "";

            if (player.Token == game.GameState.DrawingToken)
            {
                secretWordMessage = $"Secret word: {game.GameState.ActualSecretWord}";
            }
            else
            {
                secretWordMessage = game.GameState.HiddenSecretWord;
            }

            await Clients.Client(Context.ConnectionId).SendAsync(HubMessages.OnGetSecretWord, secretWordMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }

    public async Task AddPlayerScoreAndAnnouncement(string gameHash, string token)
    {
        try 
        {
            Game game = _gameManager.GetGame(gameHash);

            if (game == null)
            {
                _logger.LogError($"Game #{gameHash} AddPlayerScoreAndAnnouncement: Game does not exist");
                return;
            }

            Player player = _playerManager.GetPlayerByToken(gameHash, token);

            if (player == null)
            {
                _logger.LogError($"Game #{gameHash} AddPlayerScoreAndAnnouncement: Player with the token {token} does not exist");
                return;
            }

            double timeLeftPercentage =
                (double)game.GameState.CurrentDrawingTimeSeconds / game.GameSettings.DrawingTimeSeconds * 100;

            int score = 0;

            if (timeLeftPercentage > 80)
            {
                score = 10;
            }
            else if (timeLeftPercentage > 70)
            {
                score = 9;
            }
            else if (timeLeftPercentage > 60)
            {
                score = 8;
            }
            else if (timeLeftPercentage > 50)
            {
                score = 7;
            }
            else if (timeLeftPercentage > 40)
            {
                score = 6;
            }
            else if (timeLeftPercentage > 30)
            {
                score = 5;
            }
            else if (timeLeftPercentage > 20)
            {
                score = 4;
            }
            else if (timeLeftPercentage > 10)
            {
                score = 3;
            }
            else
            {
                score = 2;
            }

            _playerManager.UpdatePlayerScore(gameHash, player.Token, score);
            game.GameState.CorrectAnswerCount++;

            await SendAnnouncement(gameHash, $"{player.Username} guessed the word (+{score} points)", BootstrapColors.Green);
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }
}