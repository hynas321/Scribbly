using FluentAssertions;
using Moq;
using WebApi.Application.Managers;
using WebApi.Domain.Entities;
using WebApi.Repositories.Interfaces;
using Xunit;

namespace WebApiUnitTests.Application.Managers;

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
        var player = new Player { Token = "playerToken1" };

        _mockGameRepository.Setup(repo => repo.GetGame(It.IsAny<string>())).Returns((Game)null);

        // Act
        Action act = () => _playerManager.AddPlayer(_gameHash, player);

        // Assert
        act.Should().Throw<KeyNotFoundException>().WithMessage("Game not found.");
    }

    [Fact]
    public void AddPlayer_ShouldThrowException_WhenPlayerAlreadyExists()
    {
        // Arrange
        var existingPlayer = new Player { Token = "playerToken1" };
        var game = new Game { GameState = new GameState { Players = new List<Player> { existingPlayer } } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        var newPlayer = new Player { Token = "playerToken1" };

        // Act
        Action act = () => _playerManager.AddPlayer(_gameHash, newPlayer);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Player with the same token already exists.");
    }

    [Fact]
    public void AddPlayer_ShouldAddPlayer_WhenGameExists()
    {
        // Arrange
        var game = new Game { GameState = new GameState { Players = new List<Player>() } };
        var newPlayer = new Player { Token = "playerToken1" };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        _playerManager.AddPlayer(_gameHash, newPlayer);

        // Assert
        game.GameState.Players.Should().Contain(p => p.Token == "playerToken1");
    }

    [Fact]
    public void RemovePlayer_ShouldThrowException_WhenGameDoesNotExist()
    {
        // Arrange
        _mockGameRepository.Setup(repo => repo.GetGame(It.IsAny<string>())).Returns((Game)null);

        // Act
        Action act = () => _playerManager.RemovePlayer(_gameHash, "playerToken1");

        // Assert
        act.Should().Throw<KeyNotFoundException>().WithMessage("Game not found.");
    }

    [Fact]
    public void RemovePlayer_ShouldThrowException_WhenPlayerNotFound()
    {
        // Arrange
        var game = new Game { GameState = new GameState { Players = new List<Player>() } };
        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        Action act = () => _playerManager.RemovePlayer(_gameHash, "nonExistentPlayer");

        // Assert
        act.Should().Throw<KeyNotFoundException>().WithMessage("Player not found.");
    }

    [Fact]
    public void RemovePlayer_ShouldRemovePlayer_WhenPlayerExists()
    {
        // Arrange
        var player = new Player { Token = "playerToken1" };
        var game = new Game { GameState = new GameState { Players = new List<Player> { player } } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        _playerManager.RemovePlayer(_gameHash, "playerToken1");

        // Assert
        game.GameState.Players.Should().NotContain(p => p.Token == "playerToken1");
    }

    [Fact]
    public void GetPlayerByToken_ShouldReturnPlayer_WhenPlayerExists()
    {
        // Arrange
        var player = new Player { Token = "playerToken1" };
        var game = new Game { GameState = new GameState { Players = new List<Player> { player } } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        Player result = _playerManager.GetPlayerByToken(_gameHash, "playerToken1");

        // Assert
        result.Should().Be(player);
    }

    [Fact]
    public void GetPlayerByToken_ShouldReturnNull_WhenPlayerDoesNotExist()
    {
        // Arrange
        var game = new Game { GameState = new GameState { Players = new List<Player>() } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        Player result = _playerManager.GetPlayerByToken(_gameHash, "nonExistentPlayer");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void UpdatePlayerScore_ShouldThrowException_WhenGameDoesNotExist()
    {
        // Arrange
        _mockGameRepository.Setup(repo => repo.GetGame(It.IsAny<string>())).Returns((Game)null);

        // Act
        Action act = () => _playerManager.UpdatePlayerScore(_gameHash, "player1", 10);

        // Assert
        act.Should().Throw<KeyNotFoundException>().WithMessage("Game not found.");
    }

    [Fact]
    public void UpdatePlayerScore_ShouldThrowException_WhenPlayerNotFound()
    {
        // Arrange
        var game = new Game { GameState = new GameState { Players = new List<Player>() } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        Action act = () => _playerManager.UpdatePlayerScore(_gameHash, "nonExistentPlayer", 10);

        // Assert
        act.Should().Throw<KeyNotFoundException>().WithMessage("Player not found.");
    }

    [Fact]
    public void UpdatePlayerScore_ShouldUpdateScore_WhenPlayerExists()
    {
        // Arrange
        var player = new Player { Token = "player1", Score = 10 };
        var game = new Game { GameState = new GameState { Players = new List<Player> { player } } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        _playerManager.UpdatePlayerScore(_gameHash, "player1", 20);

        // Assert
        player.Score.Should().Be(30);
    }

    [Fact]
    public void UpdatePlayerScore_ShouldOrderPlayersByScoreDesc()
    {
        // Arrange
        var player1 = new Player { Token = "player1", Score = 10 };
        var player2 = new Player { Token = "player2", Score = 20 };
        var game = new Game { GameState = new GameState { Players = new List<Player> { player1, player2 } } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        _playerManager.UpdatePlayerScore(_gameHash, "player1", 25);

        // Assert
        player1.Score.Should().Be(35);
        game.GameState.Players[0].Token.Should().Be("player1");
        game.GameState.Players[1].Token.Should().Be("player2");
    }

    [Fact]
    public void GetPlayerScores_ShouldReturnEmptyList_WhenGameNotFound()
    {
        // Arrange
        _mockGameRepository.Setup(repo => repo.GetGame(It.IsAny<string>())).Returns((Game)null);

        // Act
        var result = _playerManager.GetPlayerScores(_gameHash);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPlayerScores_ShouldReturnPlayerScores()
    {
        // Arrange
        var player1 = new Player { Token = "player1", Username = "User1", Score = 100 };
        var player2 = new Player { Token = "player2", Username = "User2", Score = 200 };
        var game = new Game { GameState = new GameState { Players = new List<Player> { player1, player2 } } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        var result = _playerManager.GetPlayerScores(_gameHash);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Username == "User1" && p.Score == 100);
        result.Should().Contain(p => p.Username == "User2" && p.Score == 200);
    }

    [Fact]
    public void CheckIfPlayerExistsByToken_ShouldReturnTrue_WhenPlayerExists()
    {
        // Arrange
        var player = new Player { Token = "playerToken1" };
        var game = new Game { GameState = new GameState { Players = new List<Player> { player } } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        var result = _playerManager.CheckIfPlayerExistsByToken(_gameHash, "playerToken1");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckIfPlayerExistsByToken_ShouldReturnFalse_WhenPlayerNotExists()
    {
        // Arrange
        var game = new Game { GameState = new GameState { Players = new List<Player>() } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        var result = _playerManager.CheckIfPlayerExistsByToken(_gameHash, "nonExistentPlayer");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckIfPlayerExistsByUsername_ShouldReturnTrue_WhenPlayerExists()
    {
        // Arrange
        var player = new Player { Token = "playerToken1", Username = "TestUser" };
        var game = new Game { GameState = new GameState { Players = new List<Player> { player } } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        var result = _playerManager.CheckIfPlayerExistsByUsername(_gameHash, "TestUser");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckIfPlayerExistsByUsername_ShouldReturnFalse_WhenPlayerNotExists()
    {
        // Arrange
        var game = new Game { GameState = new GameState { Players = new List<Player>() } };

        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        var result = _playerManager.CheckIfPlayerExistsByUsername(_gameHash, "NonExistentUser");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RemovePlayerByConnectionId_ShouldReturnPlayerAndGameHash_WhenFound()
    {
        // Arrange
        var player = new Player { Token = "playerToken1", ConnectionId = "conn123" };
        var game = new Game { GameState = new GameState { Players = new List<Player> { player } } };
        var games = new Dictionary<string, Game> { { _gameHash, game } };

        _mockGameRepository.Setup(repo => repo.GetAllGames()).Returns(games);
        _mockGameRepository.Setup(repo => repo.GetGame(_gameHash)).Returns(game);

        // Act
        var (resultPlayer, resultGameHash) = _playerManager.RemovePlayerByConnectionId("conn123");

        // Assert
        resultPlayer.Should().Be(player);
        resultGameHash.Should().Be(_gameHash);
        game.GameState.Players.Should().BeEmpty();
    }

    [Fact]
    public void RemovePlayerByConnectionId_ShouldReturnNulls_WhenNotFound()
    {
        // Arrange
        _mockGameRepository.Setup(repo => repo.GetAllGames()).Returns(new Dictionary<string, Game>());

        // Act
        var (resultPlayer, resultGameHash) = _playerManager.RemovePlayerByConnectionId("nonExistentConn");

        // Assert
        resultPlayer.Should().BeNull();
        resultGameHash.Should().BeNull();
    }
}
