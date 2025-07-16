using WebApi.Domain.Entities;
using WebApi.Repositories.Interfaces;

namespace WebApi.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly Dictionary<string, Game> _games = new();

        public void AddGame(string gameHash, Game game)
        {
            if (_games.ContainsKey(gameHash))
            {
                throw new ArgumentException("Game with the same hash already exists.");
            }

            _games.Add(gameHash, game);
        }

        public Game GetGame(string gameHash)
        {
            _games.TryGetValue(gameHash, out var game);
            return game;
        }

        public void RemoveGame(string gameHash)
        {
            if (!_games.Remove(gameHash))
            {
                throw new KeyNotFoundException("Game not found.");
            }
        }

        public Dictionary<string, Game> GetAllGames()
        {
            return _games;
        }
    }
}