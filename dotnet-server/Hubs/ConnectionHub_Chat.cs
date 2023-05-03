using Dotnet.Server.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubEvents.SendChatMessage)]
    public async Task SendChatMessage(string token, string text)
    {
        try 
        {
            if (text.Length < 1)
            {
                logger.LogError($"SendChatMessage: Text is too short {text}");
            }

            Game game = gameManager.GetGame();

            if (game == null)
            {
                logger.LogError($"SendChatMessage: Game does not exist");
            }

            Player player = gameManager.GetPlayerByToken(token);

            if (player == null)
            {
                logger.LogError($"SendChatMessage: Player with the token {token} does not exist");
            }

            if (token == game.GameState.DrawingToken)
            {
                logger.LogError($"SendChatMessage: Player with the drawing token {token} cannot send a message");
            }

            ChatMessage message = new ChatMessage()
            {
                Username = player.Username,
                Text = text
            };

            gameManager.AddChatMessage(message);

            await Clients.All.SendAsync(HubEvents.OnLoadChatMessages, JsonHelper.Serialize(game.ChatMessages));

            logger.LogInformation($"SendChatMessage: Message {text} sent by player {player.Username}");
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.LoadChatMessages)]
    public async Task LoadChatMessages(string token)
    {
        try 
        {
            Game game = gameManager.GetGame();

            if (game == null)
            {
                return;
            }

            Player player = gameManager.GetPlayerByToken(token);

            if (player == null)
            {
                return;
            }

            await Clients
                .Client(Context.ConnectionId)
                .SendAsync(HubEvents.OnLoadChatMessages, JsonHelper.Serialize(game.ChatMessages));
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

}
