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
            GameSettings = new GameSettings(),
            DrawnLines = new List<DrawnLine>()
        };

        Add(game);
    }

    public void AddDrawnLine(string hash, DrawnLine drawnLine)
    {
        Get(hash).DrawnLines.Add(drawnLine);
    }

    public int GetDrawnLineCount(string hash)
    {
        return Get(hash).DrawnLines.Count;
    }

    public DrawnLine GetDrawnLine(string hash, int index)
    {
        return Get(hash).DrawnLines[index];
    }

    public void RemoveAllDrawnLines(string hash)
    {
        Get(hash).DrawnLines.Clear();
    }
}