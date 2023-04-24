using Dotnet.Server.Models;

namespace Dotnet.Server.Managers;

class GamesManager : HostedEntitiesManager<Game>
{
    public GamesManager(int maxChatMessageCount) : base(maxChatMessageCount)
    {   
        Game game = new Game()
        {
            Hash = "TestGameHash",
            HostToken = "Test",
            Players = new List<Player>(),
            ChatMessages = new List<ChatMessage>(),
            GameSettings = new GameSettings()
        };

        Add(game);
    }
}