using System.Text.Json;
using Dotnet.Server.Managers;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class LobbyHub : Hub
{

    [HubMethodName("JoinLobby")]
    public async Task JoinLobby(string hash, string username)
    {   
        Player player = new Player()
        {
            Username = username,
            Score = 0,
            GameHash = "lobbyUrl"
        };

        lobbyManager.AddPlayerToLobby(hash, player);
        
        List<Player> playerList = lobbyManager.GetPlayers(hash);
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string playerListSerialized = JsonSerializer.Serialize(playerList, jsonSerializerOptions);
        await Clients.All.SendAsync("PlayerJoinedLobby", playerListSerialized);

        logger.LogInformation($"Player {username} joined the lobby");
    }

    [HubMethodName("LeaveLobby")]
    public async Task LeaveLobby(string hash, string username)
    {
        lobbyManager.RemovePlayerFromLobby(hash, username);

        List<Player> playerList = lobbyManager.GetPlayers(hash);
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string playerListSerialized = JsonSerializer.Serialize(playerList, jsonSerializerOptions);
        await Clients.All.SendAsync("PlayerLeftLobby", playerListSerialized);

        logger.LogInformation($"Player {username} joined the lobby");
    }
}
