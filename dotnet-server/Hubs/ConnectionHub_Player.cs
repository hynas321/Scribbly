using Dotnet.Server.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{

    [HubMethodName(HubEvents.JoinGame)]
    public async Task JoinGame(string token, string username)
    {
        try 
        {
            if (username.Length < 1)
            {
                logger.LogError($"JoinGame: Username is too short {username}");
            }

            Game game = gameManager.GetGame();

            if (game == null)
            {
                logger.LogError($"JoinGame: Game does not exist");
            }

            Player player;

            if (token == game.HostToken)
            {
                player = new Player()
                {
                    Username = username,
                    Score = 0,
                    Token = game.HostToken,
                };
            }
            else
            {
                player = new Player()
                {
                    Username = username,
                    Score = 0,
                    Token = Guid.NewGuid().ToString().Replace("-", ""),
                };
            }

            PlayerScore playerScore = new PlayerScore()
            {
                Username = player.Username,
                Score = player.Score
            };

            gameManager.AddPlayer(player);
            gameManager.AddPlayerScore(playerScore);

            List<PlayerScore> playerScores = gameManager.GetPlayersWithoutToken();
            bool gameIsStarted = game.GameState.IsStarted;

            await Clients.All.SendAsync(HubEvents.OnPlayerJoinedGame, JsonHelper.Serialize(playerScore));

            logger.LogInformation($"JoinGame: Player joined the game");    
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.LeaveGame)]
    public async Task LeaveGame(string token)
    {
        try
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                return;
            }

            Player player = gameManager.GetPlayerByToken(token);

            if (player == null)
            {
                return;
            }

            if (game.GameState.IsStarted)
            {
                gameManager.RemovePlayerScore(player.Username);
            }
            else
            {
                gameManager.RemovePlayerScore(player.Username);
                gameManager.RemovePlayer(token);
            }

            List<PlayerScore> playerScores = game.GameState.PlayerScores;
            string playerListSerialized = JsonHelper.Serialize(playerScores);

            await Clients.All.SendAsync(HubEvents.OnPlayerLeftGame, playerListSerialized);
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }
}