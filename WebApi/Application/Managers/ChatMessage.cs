using dotnet_server.Application.Managers.Interfaces;
using dotnet_server.Domain.Entities;
using dotnet_server.Repositories.Interfaces;

namespace dotnet_server.Application.Managers
{
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

            if (game == null)
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
            ChatMessage chatMessage = new ChatMessage
            {
                Username = null,
                Text = message.Text,
                BootstrapBackgroundColor = message.BootstrapBackgroundColor
            };

            AddChatMessage(gameHash, chatMessage);
        }
    }
}