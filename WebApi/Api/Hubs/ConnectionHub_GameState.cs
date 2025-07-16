using WebApi.Api.Hubs.Static;
using WebApi.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubMessages.GetSecretWord)]
    public async Task GetSecretWord(string gameHash, string token)
    {
        Game game = _gameManager.GetGame(gameHash);

        if (game is null)
        {
            _logger.LogError($"Game #{gameHash} GetSecretWord: Game does not exist");
            return;
        }

        Player player = _playerManager.GetPlayerByToken(gameHash, token);

        if (player is null)
        {
            _logger.LogError($"Game #{gameHash} GetSecretWord: Player with the token {token} does not exist");
            return;
        }

        string secretWordMessage;

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

    public async Task AddPlayerScoreAndAnnouncement(string gameHash, string token)
    {
        Game game = _gameManager.GetGame(gameHash);

        if (game is null)
        {
            _logger.LogError($"Game #{gameHash} AddPlayerScoreAndAnnouncement: Game does not exist");
            return;
        }

        Player player = _playerManager.GetPlayerByToken(gameHash, token);

        if (player is null)
        {
            _logger.LogError($"Game #{gameHash} AddPlayerScoreAndAnnouncement: Player with the token {token} does not exist");
            return;
        }

        int score = SetScore(game.GameSettings.DrawingTimeSeconds);

        _playerManager.UpdatePlayerScore(gameHash, player.Token, score);
        game.GameState.CorrectAnswerCount++;

        await SendAnnouncement(gameHash, $"{player.Username} guessed the word (+{score} points)", BootstrapColors.Green);
    }

    private int SetScore(int currentDrawingTimeInSeconds)
    {
        double timeLeftPercentage =
            (double)currentDrawingTimeInSeconds / currentDrawingTimeInSeconds * 100;

        if (timeLeftPercentage > 80)
        {
            return 10;
        }
        else if (timeLeftPercentage > 70)
        {
            return 9;
        }
        else if (timeLeftPercentage > 60)
        {
            return 8;
        }
        else if (timeLeftPercentage > 50)
        {
            return 7;
        }
        else if (timeLeftPercentage > 40)
        {
            return 6;
        }
        else if (timeLeftPercentage > 30)
        {
            return 5;
        }
        else if (timeLeftPercentage > 20)
        {
            return 4;
        }
        else if (timeLeftPercentage > 10)
        {
            return 3;
        }
        else
        {
            return 2;
        }
    }
}