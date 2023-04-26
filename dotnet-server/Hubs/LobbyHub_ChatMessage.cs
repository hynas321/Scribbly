using System.Text.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class LobbyHub : Hub
{
    [HubMethodName(HubEvents.SendChatMessage)]
    public async Task SendChatMessage(string lobbyHash, ChatMessage message)
    {
        try
        {
            lobbiesManager.AddChatMessage(lobbyHash, message);

            List<ChatMessage> chatMessageList = lobbiesManager.GetMessages(lobbyHash);
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string chatMessageListSerialized = JsonSerializer.Serialize(chatMessageList, jsonSerializerOptions);
            await Clients.All.SendAsync(HubEvents.OnLoadChatMessages, chatMessageListSerialized);

            logger.LogInformation($"Lobby #{lobbyHash}: Player '{message.Username}' posted a new chat message '{message.Text}'.");
        }
        catch (Exception ex)
        {
            logger.LogError($"Lobby #{lobbyHash}: Player '{message.Username}' could not post a new chat message '{message.Text}'. {ex}");
        }
    }

    [HubMethodName(HubEvents.LoadChatMessages)]
    public async Task LoadChatMessages(string lobbyHash, string username)
    {
        try
        {
            List<ChatMessage> chatMessageList = lobbiesManager.GetMessages(lobbyHash);
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string chatMessageListSerialized = JsonSerializer.Serialize(chatMessageList, jsonSerializerOptions);
            await Clients.All.SendAsync(HubEvents.OnLoadChatMessages, chatMessageListSerialized);
        }
        catch (Exception ex)
        {
            logger.LogError($"Lobby #{lobbyHash}: Player '{username}' could not load chat messages. {ex}");
        }
    }
}