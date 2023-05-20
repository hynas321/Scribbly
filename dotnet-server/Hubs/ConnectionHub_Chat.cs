using Dotnet.Server.JsonConfig;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubEvents.SendChatMessage)]
    public async Task SendChatMessage(string gameHash, string token, string text)
    {
        try 
        {
            if (text.Length < 1)
            {
                logger.LogError($"Game #{gameHash} SendChatMessage: Text is too short {text}");
                return;
            }

            Game game = gameManager.GetGame(gameHash);

            if (game == null)
            {
                logger.LogError($"Game #{gameHash} SendChatMessage: Game does not exist");
                return;
            }

            Player player = gameManager.GetPlayerByToken(gameHash, token);

            if (player == null)
            {
                logger.LogError($"Game #{gameHash} SendChatMessage: Player with the token {token} does not exist");
                return;
            }

            if (token == game.GameState.DrawingToken)
            {
                logger.LogError($"Game #{gameHash} SendChatMessage: Player with the drawing token {token} cannot send a message");
                return;
            }

            if (game.GameState.NoChatPermissionTokens.Contains(token))
            {
                logger.LogError($"Game #{gameHash} SendChatMessage: Player with the token {token} cannot send a message");
                return;
            }

            ChatMessage message = new ChatMessage()
            {
                Username = player.Username,
                Text = text
            };

            if (message.Text.ToLower().Trim() == game.GameState.ActualSecretWord && game.GameState.DrawingToken != "")
            {
                await AddPlayerScoreAndAnnouncement(gameHash, player.Token);

                List<PlayerScore> playerScores = gameManager.GetPlayerObjectsWithoutToken(gameHash);
                List<string> correctguessPlayerUsernames = game.GameState.CorrectGuessPlayerUsernames;

                game.GameState.NoChatPermissionTokens.Add(token);
                game.GameState.CorrectGuessPlayerUsernames.Add(player.Username);
                
                await Clients.Group(gameHash).SendAsync(HubEvents.OnUpdatePlayerScores, JsonHelper.Serialize(playerScores));
                await Clients.Group(gameHash).SendAsync(HubEvents.onUpdateCorrectGuessPlayerUsernames, JsonHelper.Serialize(correctguessPlayerUsernames));
                return;
            }

            gameManager.AddChatMessage(gameHash, message);

            await Clients.Group(gameHash).SendAsync(HubEvents.OnSendChatMessage, JsonHelper.Serialize(message));

            logger.LogInformation($"Game #{gameHash} SendChatMessage: Message {text} sent by player {player.Username}");
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.LoadChatMessages)]
    public async Task LoadChatMessages(string gameHash, string token)
    {
        try 
        {
            Game game = gameManager.GetGame(gameHash);

            if (game == null)
            {
                logger.LogError($"Game #{gameHash} LoadChatMessages: Game does not exist");
                return;
            }

            Player player = gameManager.GetPlayerByToken(gameHash, token);

            if (player == null)
            {
                logger.LogError($"Game #{gameHash} LoadChatMessages: Player with the token {token} does not exist");
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

    public async Task SendAnnouncement(string gameHash, string text, string backgroundColor)
    {
        try 
        {
            Game game = new Game();

            if (game == null)
            {
                logger.LogError($"Game #{gameHash} SendAnnouncement: Game does not exist");
                return;
            }

            AnnouncementMessage message = new AnnouncementMessage()
            {
                Text = text,
                BootstrapBackgroundColor = backgroundColor
            };

            //gameManager.AddChatMessage(message);

            await Clients.Group(gameHash).SendAsync(HubEvents.OnSendAnnouncement, JsonHelper.Serialize(message));
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }
}
