using FluentAssertions;
using WebApi.Domain.Entities;
using WebApi.Repositories;
using WebApi.Repositories.Interfaces;

namespace WebApiUnitTests.Infrastructure.Repositories;

public class GameRepositoryTests
{
    private readonly IGameRepository _repository;

    public GameRepositoryTests()
    {
        _repository = new GameRepository();
    }

    [Fact]
    public void AddGame_ShouldAddGameToDictionary()
    {
        // Arrange
        var gameHash = "testGameHash";
        var game = new Game();

        // Act
        _repository.AddGame(gameHash, game);

        // Assert
        var retrievedGame = _repository.GetGame(gameHash);
        retrievedGame.Should().NotBeNull().And.Be(game);
    }

    [Fact]
    public void AddGame_ShouldThrowException_WhenGameHashAlreadyExists()
    {
        // Arrange
        var gameHash = "testGameHash";
        var game1 = new Game();
        var game2 = new Game();

        // Act
        _repository.AddGame(gameHash, game1);
        Action act = () => _repository.AddGame(gameHash, game2);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Game with the same hash already exists.");
    }

    [Fact]
    public void GetGame_ShouldReturnGame_WhenGameExists()
    {
        // Arrange
        var gameHash = "testGameHash";
        var game = new Game { HostToken = "host123" };
        _repository.AddGame(gameHash, game);

        // Act
        var result = _repository.GetGame(gameHash);

        // Assert
        result.Should().NotBeNull();
        result.HostToken.Should().Be("host123");
    }

    [Fact]
    public void GetGame_ShouldReturnNull_WhenGameDoesNotExist()
    {
        // Act
        var result = _repository.GetGame("nonExistentHash");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void RemoveGame_ShouldRemoveGameFromDictionary()
    {
        // Arrange
        var gameHash = "testGameHash";
        var game = new Game();
        _repository.AddGame(gameHash, game);

        // Act
        _repository.RemoveGame(gameHash);

        // Assert
        _repository.GetGame(gameHash).Should().BeNull();
    }

    [Fact]
    public void RemoveGame_ShouldThrowException_WhenGameDoesNotExist()
    {
        // Act
        Action act = () => _repository.RemoveGame("nonExistentHash");

        // Assert
        act.Should().Throw<KeyNotFoundException>().WithMessage("Game not found.");
    }

    [Fact]
    public void GetAllGames_ShouldReturnAllGames()
    {
        // Arrange
        _repository.AddGame("hash1", new Game());
        _repository.AddGame("hash2", new Game());
        _repository.AddGame("hash3", new Game());

        // Act
        var allGames = _repository.GetAllGames();

        // Assert
        allGames.Should().HaveCount(3);
    }

    [Fact]
    public void GetAllGames_ShouldReturnEmptyDictionary_WhenNoGamesExist()
    {
        // Act
        var allGames = _repository.GetAllGames();

        // Assert
        allGames.Should().NotBeNull().And.BeEmpty();
    }
}
