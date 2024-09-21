using dotnet_server.Application.Managers.Interfaces;
using dotnet_server.Domain.Entities;
using dotnet_server.Repositories.Interfaces;

namespace dotnet_server.Application.Managers
{
    public class GameManager : IGameManager
    {
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
    }
}