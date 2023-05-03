using Dotnet.Server.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{

    [HubMethodName(HubEvents.JoinGame)]
    public async Task JoinGame(
        string token,
        string gameHash,
        string username
    )
    {
        try 
        {
            if (username.Length < 1)
            {
                logger.LogError($"JoinGame: Username is too short {username}");
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                logger.LogError($"JoinGame: Game with the hash {gameHash} does not exist");
            }

            Player player;

            if (token == game.HostToken)
            {
                player = new Player()
                {
                    Username = username,
                    Score = 0,
                    Token = game.HostToken,
                    GameHash = gameHash
                };
            }
            else
            {
                player = new Player()
                {
                    Username = username,
                    Score = 0,
                    Token = Guid.NewGuid().ToString().Replace("-", ""),
                    GameHash = gameHash
                };
            }

            PlayerScore playerScore = new PlayerScore()
            {
                Username = player.Username,
                Score = player.Score
            };

            gamesManager.AddPlayer(game, player);
            gamesManager.AddPlayerScore(game, playerScore);

            List<PlayerScore> playerScores = gamesManager.GetPlayersWithoutToken(gameHash);
            bool gameIsStarted = game.GameState.IsStarted;

            await Groups.AddToGroupAsync(Context.ConnectionId, gameHash);

            await Clients
                .GroupExcept(gameHash, Context.ConnectionId)
                .SendAsync(HubEvents.OnPlayerJoinedGame, JsonHelper.Serialize(playerScore));

            logger.LogInformation($"JoinGame: Player joined the game with the hash {gameHash}");    
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.LeaveGame)]
    public async Task LeaveGame(
        string token,
        string gameHash
    )
    {
        try
        {
            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                return;
            }

            Player player = gamesManager.GetPlayerByToken(game, token);

            if (player == null)
            {
                return;
            }

            if (game.GameState.IsStarted)
            {
                gamesManager.RemovePlayerScore(game, player.Username);
            }
            else
            {
                gamesManager.RemovePlayerScore(game, player.Username);
                gamesManager.RemovePlayer(game, token);
            }

            List<PlayerScore> playerScores = game.GameState.PlayerScores;
            string playerListSerialized = JsonHelper.Serialize(playerScores);

            await Clients
                .GroupExcept(gameHash, Context.ConnectionId)
                .SendAsync(HubEvents.OnPlayerLeftGame, playerListSerialized);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameHash);
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }
}