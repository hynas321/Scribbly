using Dotnet.Server.Models;

namespace Dotnet.Server.Managers;

class GameManager
{
    private int maxChatMessageCount;
    public static Game Game = null;
    
    public GameManager(int maxChatMessageCount)
    {
        this.maxChatMessageCount = maxChatMessageCount;
    }

    public void SetGame(Game game)
    {
        GameManager.Game = game;
    }

    public Game GetGame()
    {
        return GameManager.Game;
    }

    public void RemoveGame()
    {
        GameManager.Game = null;
    }

    public void AddPlayer(Player player)
    {   
        Game.GameState.Players.Add(player);
    }

    public void AddPlayerScore(PlayerScore playerScore)
    {
        Game.GameState.PlayerScores.Add(playerScore);
        Game.GameState.PlayerScores.Sort((score1, score2) => score2.Score.CompareTo(score1.Score));
    }

    public void RemovePlayer(string token)
    {
        Game.GameState.Players.RemoveAll(obj => obj.Token == token);
    }

    public void RemovePlayerScore(string username)
    {
        Game.GameState.PlayerScores.RemoveAll(obj => obj.Username == username);
    }

    public Player GetPlayerByToken(string token)
    {
        return Game.GameState.Players.Find(obj => obj.Token == token);
    }

    public List<PlayerScore> GetPlayerObjectsWithoutToken()
    {
        List<PlayerScore> playerObjs = new List<PlayerScore>();

        foreach(Player player in Game.GameState.Players)
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

    public List<string> GetOnlinePlayersTokens()
    {
        List<PlayerScore> onlinePlayers = Game.GameState.PlayerScores;
        List<string> playerTokens = new List<string>();

        foreach(Player player in Game.GameState.Players)
        {
            bool playerIsOnline = onlinePlayers.Exists(obj => obj.Username == player.Username);

            if (playerIsOnline)
            {
                playerTokens.Add(player.Token);
            }
        }

        return playerTokens;
    }

    public bool CheckIfPlayerExistsByToken(string token)
    {
        return Game.GameState.Players.Find(obj => obj.Token == token) != null;
    }

    public bool CheckIfPlayerExistsByUsername(string username)
    {
        return Game.GameState.Players.Find(obj => obj.Username == username) != null;
    }

    public bool CheckIfPlayerScoreExistByUsername(string username)
    {
        return Game.GameState.PlayerScores.Find(obj => obj.Username == username) != null;
    }

    public bool CheckIfHostIsOnline()
    {
        Player hostPlayer = GetPlayerByToken(Game.HostToken);
        return Game.GameState.PlayerScores != null;
    }

    public void AddChatMessage(ChatMessage message)
    {
        List<ChatMessage> messages = Game.ChatMessages;

        if (messages.Count == maxChatMessageCount)
        {
            messages.RemoveAt(0);
        }

        messages.Add(message);
    }

    public void UpdatePlayerScore(string token, int score)
    {
        Player player = Game.GameState.Players.Find(player => player.Token == token);

        if (player != null)
        {
            player.Score += score;

            PlayerScore playerScore = Game.GameState.PlayerScores.Find(playerScore => playerScore.Username == player.Username);
            
            if (playerScore != null)
            {
                playerScore.Score = score;
                Game.GameState.PlayerScores.Sort((score1, score2) => score2.Score.CompareTo(score1.Score));
            }
        }
    }
}