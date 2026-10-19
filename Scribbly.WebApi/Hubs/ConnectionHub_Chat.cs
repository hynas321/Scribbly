using Microsoft.AspNetCore.SignalR;
using Scribbly.Application.Features.Chat;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.Hubs.Static;

namespace Scribbly.WebApi.Hubs;

public partial class HubConnection
{
    [HubMethodName(HubMessages.SendChatMessage)]
    public async Task SendChatMessage(string gameHash, string text)
    {
        SendChatMessageResult result = await _chat.SendAsync(gameHash, RequireRoomAccessToken(), text, Context.ConnectionAborted);
        if (result.Status == SendChatMessageStatus.CorrectGuess)
        {
            await Clients.Group(gameHash).SendAsync(HubMessages.OnAnnouncement,
                new AnnouncementMessage { Text = $"{result.Username} guessed the prompt (+{result.AwardedScore} points)", BootstrapBackgroundColor = BootstrapColors.Green }, Context.ConnectionAborted);
            await Clients.Group(gameHash).SendAsync(HubMessages.OnPlayerScoresUpdated, result.Scores ?? Array.Empty<PlayerScore>(), Context.ConnectionAborted);
            await Clients.Group(gameHash).SendAsync(HubMessages.OnCorrectGuessPlayersUpdated, result.CorrectGuessPlayerUsernames ?? Array.Empty<string>(), Context.ConnectionAborted);
        }
        else if (result.Status == SendChatMessageStatus.Sent && result.Message is not null)
        {
            await Clients.Group(gameHash).SendAsync(HubMessages.OnChatMessage, result.Message, Context.ConnectionAborted);
        }
        else if (result.Status == SendChatMessageStatus.MessageTooLong)
        {
            throw new HubException("Message is too long.");
        }
    }

    [HubMethodName(HubMessages.LoadChatMessages)]
    public async Task<IReadOnlyCollection<ChatMessage>> LoadChatMessages(string gameHash) =>
        await _chat.GetMessagesAsync(gameHash, RequireRoomAccessToken(), Context.ConnectionAborted)
        ?? throw new HubException("You are not a member of this room.");
}
