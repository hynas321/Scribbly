using Dotnet.Server.Models;

namespace Dotnet.Server.Managers;

class GameManager
{
    private int maxChatMessageCount = 25;
    private static Game Game = null;

    public GameManager()
    {
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

    public void RemovePlayer(string token)
    {
        Game.GameState.Players.RemoveAll(obj => obj.Token == token);
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
        List<PlayerScore> onlinePlayers = GetPlayerObjectsWithoutToken();
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

    public void AddChatMessage(ChatMessage chatMessage)
    {
        List<ChatMessage> messages = Game.ChatMessages;

        if (messages.Count == maxChatMessageCount)
        {
            messages.RemoveAt(0);
        }

        messages.Add(chatMessage);
    }

    public void AddChatMessage(AnnouncementMessage message)
    {
        List<ChatMessage> messages = Game.ChatMessages;

        if (messages.Count == maxChatMessageCount)
        {
            messages.RemoveAt(0);
        }

        ChatMessage chatMessage = new ChatMessage()
        {
            Username = null,
            Text = message.Text,
            BootstrapBackgroundColor = message.BootstrapBackgroundColor
        };

        messages.Add(chatMessage);
    }

    public void UpdatePlayerScore(string token, int score)
    {
        Player player = Game.GameState.Players.Find(player => player.Token == token);

        if (player != null)
        {
            player.Score += score;
            Game.GameState.Players.Sort((player1, player2) => player2.Score.CompareTo(player1.Score));
        }
    }
}