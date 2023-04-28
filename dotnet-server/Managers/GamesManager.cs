using Dotnet.Server.Models;

namespace Dotnet.Server.Managers;

class GamesManager
{
    private int maxChatMessageCount;
    public static List<Game> gameList = new List<Game>();
    
    public GamesManager(int maxChatMessageCount)
    {
        this.maxChatMessageCount = maxChatMessageCount;
    }

    public void AddGame(Game game)
    {
        bool containsGame = gameList.Any(e => e.GameHash == game.GameHash);

        if (containsGame)
        {
            throw new Exception($"Entity with the hash {game.GameHash} already exists.");
        }

        gameList.Add(game);
    }

    public void RemoveGame(string gameHash)
    {
        gameList.RemoveAll(obj => obj.GameHash == gameHash);
    }

    public int GetGameCount()
    {
        return gameList.Count;
    }

    public List<Game> GetAllGames()
    {
        return gameList;
    }

    public Game GetGame(string gameHash)
    {
        return gameList.Find(obj => obj.GameHash == gameHash);
    }

    public bool GameExists(string gameHash)
    {
        return GetGame(gameHash) != null;
    }

    public void AddPlayer(string gameHash, Player player)
    {   
        GetGame(gameHash)?.Players?.Add(player);
    }

    public void RemovePlayer(string gameHash, string token)
    {
       GetGame(gameHash)?.Players?.RemoveAll(obj => obj.Token == token);
        
    }

    public Player GetPlayerByToken(string gameHash, string token)
    {
        return GetGame(gameHash)?.Players.Find(obj => obj.Token == token);
    }

    public List<PlayerScore> GetPlayersWithoutToken(string gameHash)
    {
        Game game = GetGame(gameHash);
        List<PlayerScore> playerObjs = new List<PlayerScore>();

        foreach(Player player in game.Players)
        {
            playerObjs.Add(
                new PlayerScore
                {
                    Username = player.Username,
                    Score = player.Score
                }
            );
        }

        return playerObjs;
    }

    public bool CheckIfPlayerExistsByToken(string gameHash, string token)
    {
        return GetGame(gameHash)?.Players.Find(obj => obj.Token == token) != null;
    }

    public bool CheckIfPlayerExistsByUsername(string gameHash, string username)
    {
        return GetGame(gameHash)?.Players.Find(obj => obj.Username == username) != null;
    }


    public int GetPlayersCount(string gameHash)
    {   
        return GetGame(gameHash).Players.Count;
    }

    public List<Player> GetPlayers(string gameHash)
    {
        return GetGame(gameHash).Players;
    }

    public void AddChatMessage(string gameHash, ChatMessage message)
    {
        List<ChatMessage> messages = GetGame(gameHash).ChatMessages;

        if (messages.Count == maxChatMessageCount)
        {
            messages.RemoveAt(0);
        }

        messages.Add(message);
        
    }

    public List<ChatMessage> GetMessages(string hash)
    {
        return GetGame(hash).ChatMessages;
    }

    public GameSettings GetGameSettings(string hash)
    {      
        return GetGame(hash).GameSettings;
    }

    public void ChangeGameSettings(string hash, GameSettings settings)
    {
        GetGame(hash).GameSettings = settings;
    }
    public void AddDrawnLine(string hash, DrawnLine drawnLine)
    {
        GetGame(hash).DrawnLines.Add(drawnLine);
    }

    public int GetDrawnLineCount(string hash)
    {
        return GetGame(hash).DrawnLines.Count;
    }

    public DrawnLine GetDrawnLine(string hash, int index)
    {
        return GetGame(hash).DrawnLines[index];
    }

    public void RemoveAllDrawnLines(string hash)
    {
        GetGame(hash).DrawnLines.Clear();
    }
}