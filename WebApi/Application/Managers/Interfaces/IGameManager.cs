using WebApi.Domain.Entities;

namespace WebApi.Application.Managers.Interfaces;

public interface IGameManager
{
    void CreateGame(Game game, string gameHash);
    Game GetGame(string gameHash);
    void RemoveGame(string gameHash);
}