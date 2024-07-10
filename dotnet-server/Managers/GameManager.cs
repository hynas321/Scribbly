using Dotnet.Server.Managers;
using Dotnet.Server.Models;
using dotnet_server.Repositories.Interfaces;

public class GameManager : IGameManager
{
    private const int MaxChatMessageCount = 25;
    private readonly IGameRepository _gameRepository;

    public GameManager(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public void CreateGame(Game game, string gameHash)
    {
        _gameRepository.AddGame(gameHash, game);
    }

    public Game GetGame(string gameHash)
    {
        return _gameRepository.GetGame(gameHash);
    }

    public void RemoveGame(string gameHash)
    {
        _gameRepository.RemoveGame(gameHash);
    }

    public void AddPlayer(string gameHash, Player player)
    {
        var game = _gameRepository.GetGame(gameHash);
        game.GameState.Players.Add(player);
    }

    public void RemovePlayer(string gameHash, string token)
    {
        var game = _gameRepository.GetGame(gameHash);
        game.GameState.Players.RemoveAll(p => p.Token == token);
    }

    public Player GetPlayerByToken(string gameHash, string token)
    {
        var game = _gameRepository.GetGame(gameHash);
        return game.GameState.Players.Find(p => p.Token == token);
    }

    public List<PlayerScore> GetPlayerObjectsWithoutToken(string gameHash)
    {
        var game = _gameRepository.GetGame(gameHash);

        if (game == null)
        {
            return new List<PlayerScore>();
        }

        return game.GameState.Players.Select(p => new PlayerScore
        {
            Username = p.Username,
            Score = p.Score
        }).ToList();
    }

    public List<string> GetOnlinePlayersTokens(string gameHash)
    {
        var game = _gameRepository.GetGame(gameHash);

        if (game == null)
        {
            return new List<string>();
        }

        var onlinePlayers = GetPlayerObjectsWithoutToken(gameHash);

        return game.GameState.Players
            .Where(p => onlinePlayers.Any(op => op.Username == p.Username))
            .Select(p => p.Token)
            .ToList();
    }

    public bool CheckIfPlayerExistsByToken(string gameHash, string token)
    {
        var game = _gameRepository.GetGame(gameHash);
        return game.GameState.Players.Any(p => p.Token == token);
    }

    public bool CheckIfPlayerExistsByUsername(string gameHash, string username)
    {
        var game = _gameRepository.GetGame(gameHash);
        return game.GameState.Players.Any(p => p.Username == username);
    }

    public void AddChatMessage(string gameHash, ChatMessage chatMessage)
    {
        var game = _gameRepository.GetGame(gameHash);
        var messages = game.ChatMessages;

        if (messages.Count >= MaxChatMessageCount)
        {
            messages.RemoveAt(0);
        }

        messages.Add(chatMessage);
    }

    public void AddChatMessage(string gameHash, AnnouncementMessage message)
    {
        var chatMessage = new ChatMessage
        {
            Username = null,
            Text = message.Text,
            BootstrapBackgroundColor = message.BootstrapBackgroundColor
        };

        AddChatMessage(gameHash, chatMessage);
    }

    public void UpdatePlayerScore(string gameHash, string token, int score)
    {
        var game = _gameRepository.GetGame(gameHash);
        var player = game.GameState.Players.Find(p => p.Token == token);

        if (player != null)
        {
            player.Score += score;
            game.GameState.Players.Sort((p1, p2) => p2.Score.CompareTo(p1.Score));
        }
    }

    public (Player player, string gameHash) RemovePlayer(string connectionId)
    {
        var games = _gameRepository.GetAllGames();

        foreach (var game in games)
        {
            var playerToRemove = game.Value.GameState.Players
                .FirstOrDefault(p => p.ConnectionId == connectionId);

            if (playerToRemove != null)
            {
                RemovePlayer(game.Key, playerToRemove.Token);
                return (playerToRemove, game.Key);
            }
        }

        return (null, null);
    }
}