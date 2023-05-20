using Dotnet.Server.Models;

namespace Dotnet.Server.Managers;

class GameManager
{
    private int maxChatMessageCount = 25;
    private readonly ILogger<GameManager> logger;
    private static Dictionary<string, Game> Games = new Dictionary<string, Game>();

    public GameManager()
    {
        ILoggerFactory loggerFactory = new LoggerFactory();
        logger = loggerFactory.CreateLogger<GameManager>();
    }

    public void CreateGame(Game game, string gameHash)
    { 
        try
        {
            Games.Add(gameHash, game);
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex}");
        }
    }

    public Game GetGame(string gameHash)
    {
        try
        {
            return Games[gameHash];
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex}");
            return null;
        }
    }

    public void RemoveGame(string gameHash)
    {
        try
        {
            Games.Remove(gameHash);
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex}");
        }
    }

    public void AddPlayer(string gameHash, Player player)
    {   
        try
        {
            GetGame(gameHash).GameState.Players.Add(player);
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex}");
        }
    }

    public void RemovePlayer(string gameHash, string token)
    {
        try
        {
            GetGame(gameHash).GameState.Players.RemoveAll(obj => obj.Token == token);
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex}");
        }
    }

    public Player GetPlayerByToken(string gameHash, string token)
    {
        try
        {
            return GetGame(gameHash).GameState.Players.Find(obj => obj.Token == token);
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex}");
            return null;
        }

    }

    public List<PlayerScore> GetPlayerObjectsWithoutToken(string gameHash)
    {
        try
        {
            List<PlayerScore> playerObjs = new List<PlayerScore>();
            Game game = GetGame(gameHash);

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
        catch (Exception ex)
        {
            logger.LogError($"{ex}");
            return null;
        }
    }

    public List<string> GetOnlinePlayersTokens(string gameHash)
    {
        try
        {
            List<PlayerScore> onlinePlayers = GetPlayerObjectsWithoutToken(gameHash);
            List<string> playerTokens = new List<string>();
            Game game = GetGame(gameHash);

            foreach(Player player in game.GameState.Players)
            {
                bool playerIsOnline = onlinePlayers.Exists(obj => obj.Username == player.Username);

                if (playerIsOnline)
                {
                    playerTokens.Add(player.Token);
                }
            }

            return playerTokens;
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex}");
            return null;
        }
    }

    public bool CheckIfPlayerExistsByToken(string gameHash, string token)
    {
        try
        {
            return GetGame(gameHash).GameState.Players.Find(obj => obj.Token == token) != null;
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex}");
            return false;
        }
    }

    public bool CheckIfPlayerExistsByUsername(string gameHash, string username)
    {
        try
        {
            return GetGame(gameHash).GameState.Players.Find(obj => obj.Username == username) != null;
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex}");
            return false;
        }
    }

    public void AddChatMessage(string gameHash, ChatMessage chatMessage)
    {
        try
        {
            Game game = GetGame(gameHash);
            List<ChatMessage> messages = game.ChatMessages;

            if (messages.Count == maxChatMessageCount)
            {
                messages.RemoveAt(0);
            }

            messages.Add(chatMessage);
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex}");
        }
    }

    public void AddChatMessage(string gameHash, AnnouncementMessage message)
    {
        try
        {
            Game game = GetGame(gameHash);
            List<ChatMessage> messages = game.ChatMessages;

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
        catch (Exception ex)
        {
            logger.LogError($"{ex}");
        }
    }

    public void UpdatePlayerScore(string gameHash, string token, int score)
    {
        try
        {
            Game game = GetGame(gameHash);
            Player player = game.GameState.Players.Find(player => player.Token == token);

            if (player != null)
            {
                player.Score += score;
                game.GameState.Players.Sort((player1, player2) => player2.Score.CompareTo(player1.Score));
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex}");
        }
    }
}