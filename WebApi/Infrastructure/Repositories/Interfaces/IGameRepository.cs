using WebApi.Domain.Entities;

namespace WebApi.Repositories.Interfaces;

public interface IGameRepository
{
    public void AddGame(string gameHash, Game game);
    public Game GetGame(string gameHash);
    public void RemoveGame(string gameHash);
    public Dictionary<string, Game> GetAllGames();
}
