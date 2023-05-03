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
            logger.LogInformation(token);
            logger.LogInformation(gameHash);
            logger.LogInformation(text);

            if (text.Length < 1)
            {
                logger.LogError($"SendChatMessage: Text is too short {text}");
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                logger.LogError($"SendChatMessage: Game with the hash {gameHash} does not exist");
            }

            Player player = gamesManager.GetPlayerByToken(game, token);

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

            gamesManager.AddChatMessage(gameHash, message);

            await Clients
                .Group(gameHash)
                .SendAsync(HubEvents.OnLoadChatMessages, JsonHelper.Serialize(game.ChatMessages));

            logger.LogInformation($"SendChatMessage: Message {text} sent by player {player.Username}");
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
