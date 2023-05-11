using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubEvents.GetSecretWord)]
    public async Task GetSecretWord(string token)
    {
        try
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                logger.LogError($"GetSecretWord: Game does not exist");
                return;
            }

            Player player = gameManager.GetPlayerByToken(token);

            if (player == null)
            {
                logger.LogError($"GetSecretWord: Player with the token {token} does not exist");
                return;
            }

            string secretWord = "";

            if (player.Token == game.GameState.DrawingToken)
            {
                secretWord = game.GameState.ActualSecretWord;
            }
            else
            {
                secretWord = game.GameState.HiddenSecretWord;
            }

            await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnGetSecretWord, secretWord);
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    public async Task AddPlayerScoreAndAnnouncement(string token)
    {
        try 
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                logger.LogError($"AddPlayerScoreAndAnnouncement: Game does not exist");
                return;
            }

            Player player = gameManager.GetPlayerByToken(token);

            logger.LogInformation(Convert.ToString(player.Score));

            if (player == null)
            {
                logger.LogError($"AddPlayerScoreAndAnnouncement: Player with the token {token} does not exist");
                return;
            }

            double timeLeftPercentage =
                ((double)game.GameState.CurrentDrawingTimeSeconds / game.GameSettings.DrawingTimeSeconds) * 100;

            int score = 0;

            logger.LogInformation(Convert.ToString(timeLeftPercentage));

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

            gameManager.UpdatePlayerScore(player.Token, score);
            game.GameState.CorrectAnswerCount++;

            await SendAnnouncement($"{player.Username} guessed the word (+{score} points)", BootstrapColors.Green);
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }
}