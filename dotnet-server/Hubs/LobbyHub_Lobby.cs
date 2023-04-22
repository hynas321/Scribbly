using System.Text.Json;
using Dotnet.Server.Managers;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class LobbyHub : Hub
{

    [HubMethodName("JoinLobby")]
    public async Task JoinLobby(string lobbyUrl, string username)
    {   
        Player player = new Player()
        {
            Username = username,
            Score = 0,
            Host = false,
            GameUrl = "lobbyUrl"
        };

        lobbyManager.AddPlayerToLobby(lobbyUrl, player);
        
        List<Player> playerList = lobbyManager.GetPlayers(lobbyUrl);
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string playerListSerialized = JsonSerializer.Serialize(playerList, jsonSerializerOptions);
        await Clients.All.SendAsync("PlayerJoinedLobby", playerListSerialized);

        logger.LogInformation($"Player {username} joined the lobby");
    }

    [HubMethodName("LeaveLobby")]
    public async Task LeaveLobby(string lobbyUrl, string username)
    {
        lobbyManager.RemovePlayerFromLobby(lobbyUrl, username);

        List<Player> playerList = lobbyManager.GetPlayers(lobbyUrl);
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string playerListSerialized = JsonSerializer.Serialize(playerList, jsonSerializerOptions);
        await Clients.All.SendAsync("PlayerLeftLobby", playerListSerialized);

        logger.LogInformation($"Player {username} joined the lobby");
    }
}
