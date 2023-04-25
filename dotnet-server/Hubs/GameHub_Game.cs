using System.Text.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class GameHub : Hub
{
    [HubMethodName("JoinGame")]
    public async Task JoinGame(string hash, string username)
    {
        try
        {
            Player player = new Player()
            {
                Username = username,
                Score = 0,
                gameHash = hash
            };

            gamesManager.AddPlayer(hash, player);
            
            List<Player> playerList = gamesManager.GetPlayers(hash);
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string playerListSerialized = JsonSerializer.Serialize(playerList, jsonSerializerOptions);
            await Clients.All.SendAsync("PlayerJoinedGame", playerListSerialized);

            logger.LogInformation($"Game #{hash}: Player {username} joined the game.");
        }
        catch (Exception ex)
        {
            logger.LogError($"Game #{hash}: Player {username} could not join the game. {ex}");
        }
    }

    [HubMethodName("LeaveGame")]
    public async Task LeaveGame(string hash, string username)
    {
        try
        {
            gamesManager.RemovePlayer(hash, username);

            List<Player> playerList = gamesManager.GetPlayers(hash);
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string playerListSerialized = JsonSerializer.Serialize(playerList, jsonSerializerOptions);
            await Clients.All.SendAsync("PlayerLeftGame", playerListSerialized);

            logger.LogInformation($"Game #{hash}: Player {username} left the game.");
        }
        catch (Exception ex)
        {
            logger.LogError($"Game #{hash}: Player {username} could not leave the game. {ex}");
        }
    }
}