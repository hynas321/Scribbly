using Moq;
using WebApi.Application.Managers;
using WebApi.Domain.Entities;
using WebApi.Repositories.Interfaces;

namespace WebApi.UnitTests;

public class PlayerManagerTests
{
    private readonly PlayerManager _playerManager;
    private readonly Mock<IGameRepository> _mockGameRepository;
    private readonly string _gameHash = "game123";

    public PlayerManagerTests()
    {
        _mockGameRepository = new Mock<IGameRepository>();
        _playerManager = new PlayerManager(_mockGameRepository.Object);
    }

    [Fact]
    public void AddPlayer_ShouldThrowException_WhenGameDoesNotExist()
    {
        // Arrange
        Player player = new() { Token = "playerToken1" };

        _mockGameRepository.Setup(repo => repo.GetGame(It.IsAny<string>())).Returns((Game)null);

        // Act and Assert
        Assert.Throws<KeyNotFoundException>(() => _playerManager.AddPlayer(_gameHash, player));
    }

    [Fact]
    public void AddPlayer_ShouldThrowException_WhenPlayerAlreadyExists()
    {
        // Arrange
        Player existingPlayer = new() { Token = "playerToken1" };
        Game game = new() { GameState = new GameState { Players = [existingPlayer] } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        Player newPlayer = new() { Token = "playerToken1" };

        // Act and Assert
        Assert.Throws<ArgumentException>(() => _playerManager.AddPlayer(_gameHash, newPlayer));
    }

    [Fact]
    public void AddPlayer_ShouldAddPlayer_WhenGameExists()
    {
        // Arrange
        Game game = new() { GameState = new GameState { Players = [] } };
        Player newPlayer = new() { Token = "playerToken1" };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        _playerManager.AddPlayer(_gameHash, newPlayer);

        // Assert
        Assert.Contains(game.GameState.Players, p => p.Token == "playerToken1");
    }

    [Fact]
    public void RemovePlayer_ShouldThrowException_WhenGameDoesNotExist()
    {
        // Arrange
        _mockGameRepository.Setup(repo => repo.GetGame(It.IsAny<string>())).Returns((Game)null);

        // Act and Assert
        Assert.Throws<KeyNotFoundException>(() => _playerManager.RemovePlayer(_gameHash, "playerToken1"));
    }

    [Fact]
    public void RemovePlayer_ShouldThrowException_WhenPlayerNotFound()
    {
        // Arrange
        Game game = new() { GameState = new GameState { Players = [] } };
        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act and Assert
        Assert.Throws<KeyNotFoundException>(() => _playerManager.RemovePlayer(_gameHash, "nonExistentPlayer"));
    }

    [Fact]
    public void RemovePlayer_ShouldRemovePlayer_WhenPlayerExists()
    {
        // Arrange
        Player player = new() { Token = "playerToken1" };
        Game game = new() { GameState = new GameState { Players = [player] } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        _playerManager.RemovePlayer(_gameHash, "playerToken1");

        // Assert
        Assert.DoesNotContain(game.GameState.Players, p => p.Token == "playerToken1");
    }

    [Fact]
    public void GetPlayerByToken_ShouldReturnPlayer_WhenPlayerExists()
    {
        // Arrange
        Player player = new Player { Token = "playerToken1" };
        Game game = new() { GameState = new GameState { Players = [player] } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        Player result = _playerManager.GetPlayerByToken(_gameHash, "playerToken1");

        // Assert
        Assert.Equal(player, result);
    }

    [Fact]
    public void GetPlayerByToken_ShouldReturnNull_WhenPlayerDoesNotExist()
    {
        // Arrange
        Game game = new() { GameState = new GameState { Players = [] } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        Player result = _playerManager.GetPlayerByToken(_gameHash, "nonExistentPlayer");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void UpdatePlayerScore_ShouldThrowException_WhenGameDoesNotExist()
    {
        // Arrange
        _mockGameRepository.Setup(repo => repo.GetGame(It.IsAny<string>())).Returns((Game)null);

        // Act and Assert
        Assert.Throws<KeyNotFoundException>(() => _playerManager.UpdatePlayerScore(_gameHash, "player1", 10));
    }

    [Fact]
    public void UpdatePlayerScore_ShouldThrowException_WhenPlayerNotFound()
    {
        // Arrange
        Game game = new() { GameState = new GameState { Players = [] } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act and Assert
        Assert.Throws<KeyNotFoundException>(() => _playerManager.UpdatePlayerScore(_gameHash, "nonExistentPlayer", 10));
    }

    [Fact]
    public void UpdatePlayerScore_ShouldUpdateScore_WhenPlayerExists()
    {
        // Arrange
        Player player = new() { Token = "player1", Score = 10 };
        Game game = new() { GameState = new GameState { Players = [player] } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        _playerManager.UpdatePlayerScore(_gameHash, "player1", 20);

        // Assert
        Assert.Equal(30, player.Score);
    }
}