using Dotnet.Server.Models;

namespace Dotnet.Server.Managers;

class LobbiesManager : HostedEntitiesManager<Lobby>
{
    public LobbiesManager(int maxChatMessageCount) : base(maxChatMessageCount)
    {
        Lobby lobby = new Lobby()
        {
            Hash = "TestLobbyHash",
            HostToken = "Test",
            Players = new List<Player>(),
            ChatMessages = new List<ChatMessage>(),
            GameSettings = new GameSettings()
        };
        
        Add(lobby);
    }
}