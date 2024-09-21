using Moq;
using WebApi.Application.Managers;
using WebApi.Application.Managers.Interfaces;
using WebApi.Domain.Entities;
using WebApi.Repositories.Interfaces;

namespace WebApi.UnitTests;

public class GameManagerTests
{
    private readonly IGameManager _gameManager;
    private readonly Mock<IGameRepository> _mockGameRepository;
    private readonly string _gameHash = "game123";

    public GameManagerTests()
    {
        _mockGameRepository = new Mock<IGameRepository>();
        _gameManager = new GameManager(_mockGameRepository.Object);
    }

    [Fact]
    public void CreateGame_ShouldCallAddGame_OnRepository()
    {
        // Arrange
        Game game = new Game();

        // Act
        _gameManager.CreateGame(game, _gameHash);

        // Assert
        _mockGameRepository.Verify(repo => repo.AddGame(_gameHash, game), Times.Once);
    }

    [Fact]
    public void GetGame_ShouldReturnGame_WhenGameExists()
    {
        // Arrange
        Game game = new Game { HostToken = "abc"};

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        Game? result = _gameManager.GetGame(_gameHash);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("abc", result.HostToken);
    }

    [Fact]
    public void GetGame_ShouldReturnNull_WhenGameDoesNotExist()
    {
        // Arrange
        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns((Game)null);

        // Act
        Game? result = _gameManager.GetGame(_gameHash);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void RemoveGame_ShouldCallRemoveGame_OnRepository()
    {
        // Act
        _gameManager.RemoveGame(_gameHash);

        // Assert
        _mockGameRepository.Verify(repo => repo.RemoveGame(_gameHash), Times.Once);
    }

    [Fact]
    public void RemoveGame_ShouldThrowException_WhenGameNotFound()
    {
        // Arrange
        _mockGameRepository.Setup(repo => repo.RemoveGame(It.IsAny<string>()))
            .Throws(new KeyNotFoundException("Game not found."));

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => _gameManager.RemoveGame(_gameHash));
    }
}