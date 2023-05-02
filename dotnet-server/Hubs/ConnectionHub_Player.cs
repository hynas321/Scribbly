using Dotnet.Server.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{

    [HubMethodName(HubEvents.JoinGame)]
    public async Task JoinGame(
        string gameHash,
        string username,
        string token = null
    )
    {
        try 
        {
            if (username.Length < 1)
            {
                return;
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                return;
            }

            if (gamesManager.CheckIfPlayerExistsByUsername(game, username))
            {
                return;
            }

            Player player;
            PlayerScore playerScore;

            if (token != null &&
                gamesManager.CheckIfPlayerExistsByToken(game, token) &&
                token == gamesManager.GetPlayerByToken(game, token).Token
            )
            {
                player = gamesManager.GetPlayerByToken(game, token);

                playerScore = new PlayerScore()
                {
                    Username = player.Username,
                    Score = player.Score
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

                playerScore = new PlayerScore()
                {
                    Username = player.Username,
                    Score = player.Score
                };

                gamesManager.AddPlayer(game, player);
            }

            gamesManager.AddPlayerScore(game, playerScore);
        
            List<PlayerScore> playerList = gamesManager.GetPlayersWithoutToken(gameHash);
            bool gameIsStarted = game.GameState.IsStarted;

            await Groups.AddToGroupAsync(Context.ConnectionId, gameHash);
            await Clients
                .Group(gameHash)
                .SendAsync(HubEvents.OnPlayerJoinedGame, JsonHelper.Serialize(playerList));
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.LeaveGame)]
    public async Task LeaveGame(
        string gameHash,
        string token
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
                .Group(gameHash)
                .SendAsync(HubEvents.OnPlayerLeftGame, playerListSerialized);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameHash);
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }
}