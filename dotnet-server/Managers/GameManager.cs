using Dotnet.Server.Models;

namespace Dotnet.Server.Managers;

class GameManager
{
    private int maxChatMessageCount;
    private static Game game = null;
    
    public GameManager(int maxChatMessageCount)
    {
        this.maxChatMessageCount = maxChatMessageCount;
    }

    public void SetGame(Game game)
    {
        GameManager.game = game;
    }

    public Game GetGame()
    {
        return GameManager.game;
    }

    public void RemoveGame()
    {
        GameManager.game = null;
    }

    public void AddPlayer(Player player)
    {   
        game.GameState.Players.Add(player);
    }

    public void AddPlayerScore(PlayerScore playerScore)
    {
        game.GameState.PlayerScores.Add(playerScore);
    }

    public void RemovePlayer(string token)
    {
        game.GameState.Players.RemoveAll(obj => obj.Token == token);
    }

    public void RemovePlayerScore(string username)
    {
        game.GameState.PlayerScores.RemoveAll(obj => obj.Username == username);
    }

    public Player GetPlayerByToken(string token)
    {
        return game.GameState.Players.Find(obj => obj.Token == token);
    }

    public List<PlayerScore> GetPlayersWithoutToken()
    {
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

    public bool CheckIfPlayerExistsByToken(string token)
    {
        return game.GameState.Players.Find(obj => obj.Token == token) != null;
    }

    public bool CheckIfPlayerExistsByUsername(string username)
    {
        return game.GameState.Players.Find(obj => obj.Username == username) != null;
    }

    public bool CheckIfPlayerScoreExistByUsername(string username)
    {
        return game.GameState.PlayerScores.Find(obj => obj.Username == username) != null;
    }

    public bool CheckIfHostIsOnline()
    {
        Player hostPlayer = GetPlayerByToken(game.HostToken);
        return game.GameState.PlayerScores != null;
    }

    public void AddChatMessage(ChatMessage message)
    {
        List<ChatMessage> messages = game.ChatMessages;

        if (messages.Count == maxChatMessageCount)
        {
            messages.RemoveAt(0);
        }

        messages.Add(message);
    }

    public void RemoveChatMessages()
    {
        game.ChatMessages.Clear();
    }

}