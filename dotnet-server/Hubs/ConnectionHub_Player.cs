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
            
            Player player = gameManager.GetPlayerByToken(token);

            if (player == null && token == game.HostToken)
            {
                player = new Player()
                {
                    Username = username,
                    Score = 0,
                    Token = game.HostToken,
                };

                gameManager.AddPlayer(player);
            }
            else if (player == null)
            {
                player = new Player()
                {
                    Username = username,
                    Score = 0,
                    Token = Guid.NewGuid().ToString().Replace("-", ""),
                };

                gameManager.AddPlayer(player);
            }
            else if (token == game.HostToken)
            {
                player = new Player()
                {
                    Username = player.Username,
                    Score = player.Score,
                    Token = game.HostToken,
                };
            }
            else
            {
                player = new Player()
                {
                    Username = player.Username,
                    Score = player.Score,
                    Token = player.Token,
                };
            }

            PlayerScore playerScore = new PlayerScore()
            {
                Username = player.Username,
                Score = player.Score
            };

            gameManager.AddPlayerScore(playerScore);

            List<PlayerScore> playerScores = gameManager.GetPlayersWithoutToken();
            bool gameIsStarted = game.GameState.IsStarted;

            await Clients.All.SendAsync(HubEvents.OnPlayerJoinedGame, JsonHelper.Serialize(playerScores));
            await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnJoinGame, JsonHelper.Serialize(player));

            logger.LogInformation($"JoinGame: Player {player.Username} joined the game.");
            logger.LogInformation($"Online players: {game.GameState.PlayerScores.Count}. Total players: {game.GameState.Players.Count}");
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.LeaveGame)]
    public async Task LeaveGame(string token, bool leaveForGood)
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

            if (leaveForGood)
            {
                gameManager.RemovePlayer(token);
                gameManager.RemovePlayerScore(player.Username);
            }
            else
            {
                gameManager.RemovePlayerScore(player.Username);
            }

            if (gameManager.GetGame().GameState.PlayerScores.Count == 0)
            {
                gameManager.SetGame(null);
                logger.LogInformation($"LeaveGame: Game removed - no online players");
                return;
            }

            List<PlayerScore> playerScores = game.GameState.PlayerScores;
            string playerListSerialized = JsonHelper.Serialize(playerScores);

            logger.LogInformation($"JoinGame: Player {player.Username} left the game");
            logger.LogInformation($"Online players: {game.GameState.PlayerScores.Count}. Total players: {game.GameState.Players.Count}");

            await Clients.AllExcept(Context.ConnectionId).SendAsync(HubEvents.OnPlayerLeftGame, playerListSerialized);
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }
}