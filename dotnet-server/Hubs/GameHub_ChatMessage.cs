using System.Text.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class GameHub : Hub
{
    [HubMethodName("SendChatMessage")]
    public async Task SendChatMessage(string gameHash, ChatMessage message)
    {
        try
        {
            gamesManager.AddChatMessage(gameHash, message);

            List<ChatMessage> chatMessageList = gamesManager.GetMessages(gameHash);
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string chatMessageListSerialized = JsonSerializer.Serialize(chatMessageList, jsonSerializerOptions);
            await Clients.All.SendAsync("ReceiveChatMessages", chatMessageListSerialized);

            logger.LogInformation($"Game #{gameHash}: Player '{message.Username}' posted a new chat message '{message.Text}'.");
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Game #{gameHash}: Player '{message.Username}' could not post a new chat message '{message.Text}'. {ex}");
        }
    }

    [HubMethodName("GetChatMessages")]
    public async Task GetChatMessages(string gameHash, string username)
    {
        try 
        {
            List<ChatMessage> chatMessageList = gamesManager.GetMessages(gameHash);

            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string chatMessageListSerialized = JsonSerializer.Serialize(chatMessageList, jsonSerializerOptions);
            await Clients.All.SendAsync("ReceiveChatMessages", chatMessageListSerialized);
        }
        catch (Exception ex)
        {
            logger.LogError($"Game #{gameHash}: Player '{username}' could not load chat messages. {ex}");
        }
    }
}