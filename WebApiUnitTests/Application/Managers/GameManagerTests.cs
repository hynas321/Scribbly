using FluentAssertions;
using Moq;
using WebApi.Application.Managers;
using WebApi.Application.Managers.Interfaces;
using WebApi.Domain.Entities;
using WebApi.Repositories.Interfaces;
using Xunit;

namespace WebApiUnitTests.Application.Managers;

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
        var game = new Game();

        // Act
        _gameManager.CreateGame(game, _gameHash);

        // Assert
        _mockGameRepository.Verify(repo => repo.AddGame(_gameHash, game), Times.Once);
    }

    [Fact]
    public void CreateGame_ShouldPassSameGameInstance_ToRepository()
    {
        // Arrange
        var game = new Game { HostToken = "testHostToken" };

        // Act
        _gameManager.CreateGame(game, _gameHash);

        // Assert
        _mockGameRepository.Verify(repo => repo.AddGame(_gameHash, It.Is<Game>(g => g.HostToken == "testHostToken")), Times.Once);
    }

    [Fact]
    public void GetGame_ShouldReturnGame_WhenGameExists()
    {
        // Arrange
        var game = new Game { HostToken = "abc" };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        Game? result = _gameManager.GetGame(_gameHash);

        // Assert
        result.Should().NotBeNull();
        result.HostToken.Should().Be("abc");
    }

    [Fact]
    public void GetGame_ShouldReturnNull_WhenGameDoesNotExist()
    {
        // Arrange
        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns((Game)null);

        // Act
        Game? result = _gameManager.GetGame(_gameHash);

        // Assert
        result.Should().BeNull();
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

        // Act
        Action act = () => _gameManager.RemoveGame(_gameHash);

        // Assert
        act.Should().Throw<KeyNotFoundException>().WithMessage("Game not found.");
    }

    [Fact]
    public void GetGame_ShouldCallGetGame_OnRepositoryWithCorrectHash()
    {
        // Arrange
        var specificHash = "specificHash123";
        var game = new Game();
        _mockGameRepository.Setup(repo => repo.GetGame(specificHash)).Returns(game);

        // Act
        var result = _gameManager.GetGame(specificHash);

        // Assert
        _mockGameRepository.Verify(repo => repo.GetGame(specificHash), Times.Once);
        result.Should().NotBeNull();
    }
}
