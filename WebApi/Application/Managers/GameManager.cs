using WebApi.Application.Managers.Interfaces;
using WebApi.Domain.Entities;
using WebApi.Repositories.Interfaces;

namespace WebApi.Application.Managers;

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