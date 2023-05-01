using Dotnet.Server.Models;

namespace Dotnet.Server.Managers;

class GamesManager
{
    private int maxChatMessageCount;
    public static List<Game> GameList { get; set; } = new List<Game>();
    
    public GamesManager(int maxChatMessageCount)
    {
        this.maxChatMessageCount = maxChatMessageCount;
    }

    public void AddGame(Game game)
    {
        bool containsGame = GameList.Any(e => e.GameHash == game.GameHash);

        if (containsGame)
        {
            throw new Exception($"Entity with the hash {game.GameHash} already exists.");
        }

        GameList.Add(game);
    }

    public void RemoveGame(string gameHash)
    {
        GameList.RemoveAll(obj => obj.GameHash == gameHash);
    }

    public List<Game> GetAllGames()
    {
        return GameList;
    }

    public Game GetGameByHash(string gameHash)
    {
        return GameList.Find(obj => obj.GameHash == gameHash);
    }

    public Game GetGameByPlayerToken(string token)
    {
        return GameList.Find(game => game.GameState.Players.Exists(player => player.Token == token));
    }

    public bool CheckIfGameExistsByHash(string gameHash)
    {
        return GetGameByHash(gameHash) != null;
    }

    public void AddPlayer(Game game, Player player)
    {   
        game.GameState.Players.Add(player);
    }

    public void AddPlayerScore(Game game, PlayerScore playerScore)
    {
        game.GameState.PlayerScores.Add(playerScore);
    }

    public void RemovePlayer(Game game, string token)
    {
        game.GameState.Players.RemoveAll(obj => obj.Token == token);
    }

    public void RemovePlayerScore(Game game, string username)
    {
        game.GameState.PlayerScores.RemoveAll(obj => obj.Username == username);
    }

    public Player GetPlayerByToken(string gameHash, string token)
    {
        return GetGameByHash(gameHash).GameState.Players.Find(obj => obj.Token == token);
    }

    public Player GetPlayerByToken(Game game, string token)
    {
        return game.GameState.Players.Find(obj => obj.Token == token);
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
        return GetGameByHash(gameHash).GameState.Players.Find(obj => obj.Token == token) != null;
    }

    public bool CheckIfPlayerExistsByToken(Game game, string token)
    {
        return game.GameState.Players.Find(obj => obj.Token == token) != null; 
    }

    public bool CheckIfPlayerExistsByUsername(string gameHash, string username)
    {
        return GetGameByHash(gameHash).GameState.Players.Find(obj => obj.Username == username) != null;
    }

    public bool CheckIfPlayerExistsByUsername(Game game, string username)
    {
        return game.GameState.Players.Find(obj => obj.Username == username) != null;
    }

    public bool CheckIfPlayerScoreExistByUsername(string gameHash, string username)
    {
        return GetGameByHash(gameHash).GameState.PlayerScores.Find(obj => obj.Username == username) != null;
    }

    public bool CheckIfPlayerScoreExistByUsername(Game game, string username)
    {
        return game.GameState.PlayerScores.Find(obj => obj.Username == username) != null;
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
}