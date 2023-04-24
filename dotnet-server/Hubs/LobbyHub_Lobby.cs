using System.Text.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class LobbyHub : Hub
{
    [HubMethodName("JoinLobby")]
    public async Task JoinLobby(string lobbyHash, string username)
    {
        try
        {
            Player player = new Player()
            {
                Username = username,
                Score = 0,
                GameHash = lobbyHash
            };

            lobbiesManager.AddPlayer(lobbyHash, player);
            
            List<Player> playerList = lobbiesManager.GetPlayers(lobbyHash);
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string playerListSerialized = JsonSerializer.Serialize(playerList, jsonSerializerOptions);
            await Clients.All.SendAsync("PlayerJoinedLobby", playerListSerialized);

            logger.LogInformation($"Lobby #{lobbyHash}: Player {username} joined the lobby.");
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Lobby #{lobbyHash}: Player {username} could not join the lobby. {ex}");
        }
    }

    [HubMethodName("LeaveLobby")]
    public async Task LeaveLobby(string lobbyHash, string username)
    {
        try 
        {
            lobbiesManager.RemovePlayer(lobbyHash, username);

            List<Player> playerList = lobbiesManager.GetPlayers(lobbyHash);
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string playerListSerialized = JsonSerializer.Serialize(playerList, jsonSerializerOptions);
            await Clients.All.SendAsync("PlayerLeftLobby", playerListSerialized);

            logger.LogInformation($"Lobby {lobbyHash}: Player {username} left the lobby.");
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Lobby {lobbyHash}: Player {username} could not leave the lobby. {ex}");
        }
    }
}
