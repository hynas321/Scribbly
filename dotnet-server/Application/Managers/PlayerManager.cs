using dotnet_server.Application.Managers.Interfaces;
using dotnet_server.Domain.Entities;
using dotnet_server.Repositories.Interfaces;

namespace dotnet_server.Application.Managers
{
    public class PlayerManager : IPlayerManager
    {
        private readonly IGameRepository _gameRepository;

        public PlayerManager(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public void AddPlayer(string gameHash, Player player)
        {
            Game game = _gameRepository.GetGame(gameHash);

            if (game == null)
            {
                throw new KeyNotFoundException("Game not found.");
            }

            if (game.GameState.Players.Any(p => p.Token == player.Token))
            {
                throw new ArgumentException("Player with the same token already exists.");
            }

            game.GameState.Players.Add(player);
        }

        public void RemovePlayer(string gameHash, string token)
        {
            Game game = _gameRepository.GetGame(gameHash);

            if (game == null)
            {
                throw new KeyNotFoundException("Game not found.");
            }

            int removed = game.GameState.Players.RemoveAll(p => p.Token == token);

            if (removed == 0)
            {
                throw new KeyNotFoundException("Player not found.");
            }
        }

        public (Player player, string gameHash) RemovePlayerByConnectionId(string connectionId)
        {
            Dictionary<string, Game> games = _gameRepository.GetAllGames();

            foreach (var gameEntry in games)
            {
                var playerToRemove = gameEntry.Value.GameState.Players
                    .FirstOrDefault(p => p.ConnectionId == connectionId);

                if (playerToRemove != null)
                {
                    RemovePlayer(gameEntry.Key, playerToRemove.Token);
                    return (playerToRemove, gameEntry.Key);
                }
            }

            return (null, null);
        }

        public Player GetPlayerByToken(string gameHash, string token)
        {
            Game game = _gameRepository.GetGame(gameHash);

            if (game == null)
            {
                return null;
            }

            return game.GameState.Players.Find(p => p.Token == token);
        }

        public List<PlayerScore> GetPlayerScores(string gameHash)
        {
            Game game = _gameRepository.GetGame(gameHash);

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
            Game game = _gameRepository.GetGame(gameHash);

            if (game == null)
            {
                return new List<string>();
            }

            List<PlayerScore> onlinePlayers = GetPlayerScores(gameHash);

            return game.GameState.Players
                .Where(p => onlinePlayers.Any(op => op.Username == p.Username))
                .Select(p => p.Token)
                .ToList();
        }

        public bool CheckIfPlayerExistsByToken(string gameHash, string token)
        {
            Game game = _gameRepository.GetGame(gameHash);
            return game != null && game.GameState.Players.Any(p => p.Token == token);
        }

        public bool CheckIfPlayerExistsByUsername(string gameHash, string username)
        {
            Game game = _gameRepository.GetGame(gameHash);
            return game != null && game.GameState.Players.Any(p => p.Username == username);
        }

        public void UpdatePlayerScore(string gameHash, string token, int score)
        {
            Game game = _gameRepository.GetGame(gameHash);

            if (game == null)
            {
                throw new KeyNotFoundException("Game not found.");
            }

            Player player = game.GameState.Players.Find(p => p.Token == token);

            if (player != null)
            {
                player.Score += score;
                game.GameState.Players = game.GameState.Players
                    .OrderByDescending(p => p.Score)
                    .ToList();
            }
            else
            {
                throw new KeyNotFoundException("Player not found.");
            }
        }
    }
}