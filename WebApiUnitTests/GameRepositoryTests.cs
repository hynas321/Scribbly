using WebApi.Domain.Entities;
using WebApi.Repositories;
using WebApi.Repositories.Interfaces;

namespace WebApi.UnitTests;

public class GameRepositoryTests
{
    private readonly IGameRepository _gameRepository;

    public GameRepositoryTests()
    {
        _gameRepository = new GameRepository();
    }

    [Fact]
    public void AddGame_ShouldThrowException_WhenGameAlreadyExists()
    {
        // Arrange
        Game game = new Game();
        string gameHash = "hash123";

        _gameRepository.AddGame(gameHash, game);

        // Act and Assert
        Assert.Throws<ArgumentException>(() => _gameRepository.AddGame(gameHash, game));
    }

    [Fact]
    public void RemoveGame_ShouldThrowException_WhenGameNotFound()
    {
        // Act and Assert
        Assert.Throws<KeyNotFoundException>(() => _gameRepository.RemoveGame("nonExistentHash"));
    }

    [Fact]
    public void GetGame_ShouldReturnNull_WhenGameDoesNotExist()
    {
        // Act
        Game? result = _gameRepository.GetGame("nonExistentHash");

        // Assert
        Assert.Null(result);
    }
}