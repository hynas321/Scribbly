using Dotnet.Server.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubEvents.JoinGame)]
    public async Task JoinGame(string gameHash, string username)
    {
        try
        {
            Player player = new Player()
            {
                Username = username,
                Score = 0,
            };

            gamesManager.AddPlayer(gameHash, player);
            
            List<Player> playerList = gamesManager.GetPlayers(gameHash);
            string playerListSerialized = JsonHelper.Serialize(playerList);

            await Clients.All.SendAsync(HubEvents.OnPlayerJoinedGame, playerListSerialized);

            logger.LogInformation($"Game #{gameHash}: Player {username} joined the game.");
        }
        catch (Exception ex)
        {
            logger.LogError($"Game #{gameHash}: Player {username} could not join the game. {ex}");
        }
    }

    [HubMethodName(HubEvents.LeaveGame)]
    public async Task LeaveGame(string hash, string username)
    {
        try
        {
            gamesManager.RemovePlayer(hash, username);

            List<Player> playerList = gamesManager.GetPlayers(hash);
            string playerListSerialized = JsonHelper.Serialize(playerList);

            await Clients.All.SendAsync(HubEvents.OnPlayerLeftGame, playerListSerialized);

            logger.LogInformation($"Game #{hash}: Player {username} left the game.");
        }
        catch (Exception ex)
        {
            logger.LogError($"Game #{hash}: Player {username} could not leave the game. {ex}");
        }
    }
}