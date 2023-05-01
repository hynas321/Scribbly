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

    public Game GetGameByHash(string gameHash)
    {
        return gameList.Find(obj => obj.GameHash == gameHash);
    }

    public Game GetGameByPlayerToken(string token)
    {
        return gameList.Find(game => game.GameState.Players.Exists(player => player.Token == token));
    }

    public bool CheckIfGameExistsByHash(string gameHash)
    {
        return GetGameByHash(gameHash) != null;
    }

    public void AddPlayer(string gameHash, Player player)
    {   
        GetGameByHash(gameHash)?.GameState.Players?.Add(player);
    }

    public void RemovePlayer(string gameHash, string token)
    {
       GetGameByHash(gameHash)?.GameState.Players?.RemoveAll(obj => obj.Token == token);
        
    }

    public Player GetPlayerByToken(string gameHash, string token)
    {
        return GetGameByHash(gameHash)?.GameState.Players.Find(obj => obj.Token == token);
    }

    public List<PlayerScore> GetPlayersWithoutToken(string gameHash)
    {
        Game game = GetGameByHash(gameHash);
        List<PlayerScore> playerObjs = new List<PlayerScore>();

        foreach(Player player in game.GameState.Players)
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
        return GetGameByHash(gameHash)?.GameState.Players.Find(obj => obj.Token == token) != null;
    }

    public bool CheckIfPlayerExistsByUsername(string gameHash, string username)
    {
        return GetGameByHash(gameHash)?.GameState.Players.Find(obj => obj.Username == username) != null;
    }

    public bool CheckIfPlayerIsHost(string gameHash, string token)
    {
        Game game = GetGameByHash(gameHash);
        Player player = GetPlayerByToken(gameHash, token);

        return game.HostToken == player.Token;
    }

    public void AddChatMessage(string gameHash, ChatMessage message)
    {
        List<ChatMessage> messages = GetGameByHash(gameHash).ChatMessages;

        if (messages.Count == maxChatMessageCount)
        {
            messages.RemoveAt(0);
        }

        messages.Add(message);
        
    }

    public List<ChatMessage> GetMessages(string hash)
    {
        return GetGameByHash(hash).ChatMessages;
    }

    public GameSettings GetGameSettings(string hash)
    {      
        return GetGameByHash(hash).GameSettings;
    }

    public void ChangeGameSettings(string hash, GameSettings settings)
    {
        GetGameByHash(hash).GameSettings = settings;
    }
    public void AddDrawnLine(string hash, DrawnLine drawnLine)
    {
        GetGameByHash(hash).DrawnLines.Add(drawnLine);
    }

    public int GetDrawnLineCount(string hash)
    {
        return GetGameByHash(hash).DrawnLines.Count;
    }

    public DrawnLine GetDrawnLine(string hash, int index)
    {
        return GetGameByHash(hash).DrawnLines[index];
    }

    public void RemoveAllDrawnLines(string hash)
    {
        GetGameByHash(hash).DrawnLines.Clear();
    }

    internal bool CheckIfGameExistsByHash(object gameHash)
    {
        throw new NotImplementedException();
    }
}