using dotnet_server.Domain.Entities;

namespace dotnet_server.Application.Managers.Interfaces;

public interface IGameManager
{
    void CreateGame(Game game, string gameHash);
    Game GetGame(string gameHash);
    void RemoveGame(string gameHash);
}