using System.Text.Json;
using Dotnet.Server.Managers;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class LobbyHub : Hub
{
    [HubMethodName("SendChatMessage")]
    public async Task SendChatMessage(string lobbyHash, string username, string text)
    {
        LobbyManager lobbyManager = new LobbyManager();

        ChatMessage message = new ChatMessage()
        {
            Username = username,
            Text = text
        };

        lobbyManager.AddChatMessage(lobbyHash, message);
        logger.LogInformation($"New chat message: {username}: {text}");

        List<ChatMessage> chatMessageList = lobbyManager.GetMessages(lobbyHash);
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string chatMessageListSerialized = JsonSerializer.Serialize(chatMessageList, jsonSerializerOptions);
        await Clients.All.SendAsync("ReceiveChatMessages", chatMessageListSerialized);
    }

    [HubMethodName("GetChatMessages")]
    public async Task GetChatMessages(string lobbyHash)
    {
        List<ChatMessage> chatMessageList = lobbyManager.GetMessages(lobbyHash);
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        logger.LogInformation($"Loaded chat messages");

        string chatMessageListSerialized = JsonSerializer.Serialize(chatMessageList, jsonSerializerOptions);
        await Clients.All.SendAsync("ReceiveChatMessages", chatMessageListSerialized);
    }
}