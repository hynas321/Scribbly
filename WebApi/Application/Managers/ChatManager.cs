using WebApi.Application.Managers.Interfaces;
using WebApi.Domain.Entities;
using WebApi.Repositories.Interfaces;

namespace WebApi.Application.Managers;

public class ChatManager : IChatManager
{
    private const int MaxChatMessageCount = 25;

    private readonly IGameRepository _gameRepository;

    public ChatManager(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public void AddChatMessage(string gameHash, ChatMessage chatMessage)
    {
        Game game = _gameRepository.GetGame(gameHash);

        if (game is null)
        {
            throw new KeyNotFoundException("Game not found.");
        }

        List<ChatMessage> messages = game.ChatMessages;

        if (messages.Count >= MaxChatMessageCount)
        {
            messages.RemoveAt(0);
        }

        messages.Add(chatMessage);
    }

    public void AddAnnouncementMessage(string gameHash, AnnouncementMessage message)
    {
        ChatMessage chatMessage = new()
        {
            Username = null,
            Text = message.Text,
            BootstrapBackgroundColor = message.BootstrapBackgroundColor
        };

        AddChatMessage(gameHash, chatMessage);
    }
}