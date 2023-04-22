using System.Text.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public class LobbyHub : Hub
{
    private readonly LobbyState lobbyState = new LobbyState();
    private readonly ILogger<LobbyHub> logger;

    public LobbyHub(ILogger<LobbyHub> logger)
    {
        this.logger = logger;
    }

    public async Task JoinLobby(string lobbyUrl, string username)
    {   
        Player player = new Player()
        {
            Username = username,
            Score = 0,
            Host = false,
            GameUrl = "lobbyUrl"
        };

        lobbyState.AddPlayerToLobby(lobbyUrl, player);
        
        List<Player> playerList = lobbyState.GetPlayers(lobbyUrl);
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string playerListSerialized = JsonSerializer.Serialize(playerList, jsonSerializerOptions);
        await Clients.All.SendAsync("PlayerJoinedLobby", playerListSerialized);

        logger.LogInformation($"Player {username} joined the lobby");
    }

    public async Task LeaveLobby(string lobbyUrl, string username)
    {
        lobbyState.RemovePlayerFromLobby(lobbyUrl, username);

        List<Player> playerList = lobbyState.GetPlayers(lobbyUrl);
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string playerListSerialized = JsonSerializer.Serialize(playerList, jsonSerializerOptions);
        await Clients.All.SendAsync("PlayerLeftLobby", playerListSerialized);

        logger.LogInformation($"Player {username} joined the lobby");
    }

    public async Task SendChatMessage(string url, string text)
    {
        logger.LogInformation($"New chat message: {null}: {text}");

        await Clients.All.SendAsync("ChatMessageReceived", text);
    }

    public override async Task OnConnectedAsync()
    {   
        logger.LogInformation($"New client connected: {Context.ConnectionId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {   
        logger.LogInformation($"Client disconnected: {Context.ConnectionId}");

        await base.OnDisconnectedAsync(exception);
    }
}
