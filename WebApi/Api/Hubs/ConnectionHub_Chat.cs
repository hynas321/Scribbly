using WebApi.Api.Hubs.Static;
using WebApi.Api.Utilities;
using WebApi.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using WebApi.Api.Hubs.Attributes;

namespace WebApi.Hubs;

public partial class HubConnection : Hub
{
    [HubMethodName(HubMessages.SendChatMessage)]
    [ValidateHubArgument("gameHash", ValidationType.GameHash)]
    [ValidateHubArgument("token", ValidationType.PlayerToken)]
    public async Task SendChatMessage(string gameHash, string token, string text)
    {
        Game game = Context.Items["Game"] as Game;
        Player player = Context.Items["Player"] as Player;

        if (text.Length < 1)
        {
            _logger.LogError($"Game #{gameHash} SendChatMessage: Text is too short {text}");
            return;
        }

        if (game.GameState.NoChatPermissionTokens.Contains(token))
        {
            _logger.LogError($"Game #{gameHash} SendChatMessage: Player with the token {token} cannot send a message");
            return;
        }

        if (token == game.GameState.DrawingToken)
        {
            _logger.LogError($"Game #{gameHash} SendChatMessage: Player with the drawing token {token} cannot send a message");
            return;
        }

        ChatMessage message = new()
        {
            Username = player.Username,
            Text = text
        };

        if (message.Text.ToLower().Trim() == game.GameState.ActualSecretWord && game.GameState.DrawingToken != "")
        {
            await AddPlayerScoreAndAnnouncement(gameHash, player.Token);

            List<PlayerScore> playerScores = _playerManager.GetPlayerScores(gameHash);
            List<string> correctguessPlayerUsernames = game.GameState.CorrectGuessPlayerUsernames;

            game.GameState.NoChatPermissionTokens.Add(token);
            game.GameState.CorrectGuessPlayerUsernames.Add(player.Username);
                
            await Clients.Group(gameHash).SendAsync(HubMessages.OnUpdatePlayerScores, JsonHelper.Serialize(playerScores));
            await Clients.Group(gameHash).SendAsync(HubMessages.OnUpdateCorrectGuessPlayerUsernames, JsonHelper.Serialize(correctguessPlayerUsernames));
            return;
        }

        _chatManager.AddChatMessage(gameHash, message);

        await Clients.Group(gameHash).SendAsync(HubMessages.OnSendChatMessage, JsonHelper.Serialize(message));

        _logger.LogInformation($"Game #{gameHash} SendChatMessage: Message {text} sent by player {player.Username}");
    }

    [HubMethodName(HubMessages.LoadChatMessages)]
    [ValidateHubArgument("gameHash", ValidationType.GameHash)]
    [ValidateHubArgument("token", ValidationType.PlayerToken)]
    public async Task LoadChatMessages(string gameHash, string token)
    {
        Game game = Context.Items["Game"] as Game;

        await Clients
            .Client(Context.ConnectionId)
            .SendAsync(HubMessages.OnLoadChatMessages, JsonHelper.Serialize(game.ChatMessages));
    }

    public async Task SendAnnouncement(string gameHash, string text, string backgroundColor)
    {
        AnnouncementMessage message = new()
        {
            Text = text,
            BootstrapBackgroundColor = backgroundColor
        };

        await Clients.Group(gameHash).SendAsync(HubMessages.OnSendAnnouncement, JsonHelper.Serialize(message));
    }
}
