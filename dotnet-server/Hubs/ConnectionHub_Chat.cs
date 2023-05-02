using Dotnet.Server.Json;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubEvents.SendChatMessage)]
    public async Task SendChatMessage(
        string token,
        string gameHash,
        string text
    )
    {
        try 
        {
            if (text.Length < 1)
            {
                return;
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                return;
            }

            Player player = gamesManager.GetPlayerByToken(game, token);

            if (player == null)
            {
                return;
            }

            if (token != game.GameState.DrawingToken)
            {
                return;
            }

            ChatMessage message = new ChatMessage()
            {
                Username = player.Username,
                Text = text
            };

            gamesManager.AddChatMessage(gameHash, message);

            await Clients
                .Group(gameHash)
                .SendAsync(HubEvents.OnSendChatMessage, JsonHelper.Serialize(message));
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.LoadChatMessages)]
    public async Task LoadChatMessages(
        string token,
        string gameHash
    )
    {
        try 
        {
            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                return;
            }

            Player player = gamesManager.GetPlayerByToken(game, token);

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
