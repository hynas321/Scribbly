using FluentAssertions;
using WebApi.Domain.Entities;
using Xunit;

namespace WebApiUnitTests.Domain.Entities;

public class GameStateTests
{
    [Fact]
    public void NewGameState_ShouldHaveInitializedCollections()
    {
        // Arrange & Act
        var gameState = new GameState();

        // Assert
        gameState.Players.Should().NotBeNull().And.BeEmpty();
        gameState.DrawnLines.Should().NotBeNull().And.BeEmpty();
        gameState.DrawingPlayersTokens.Should().NotBeNull().And.BeEmpty();
        gameState.NoChatPermissionTokens.Should().NotBeNull().And.BeEmpty();
        gameState.CorrectGuessPlayerUsernames.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void NewGameState_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var gameState = new GameState();

        // Assert
        gameState.IsGameStarted.Should().BeFalse();
        gameState.CurrentRound.Should().Be(1);
        gameState.CorrectAnswerCount.Should().Be(0);
    }

    [Fact]
    public void CorrectGuessPlayerUsernames_ShouldTrackMultipleGuesses()
    {
        // Arrange
        var gameState = new GameState();

        // Act
        gameState.CorrectGuessPlayerUsernames.Add("Player1");
        gameState.CorrectGuessPlayerUsernames.Add("Player2");
        gameState.CorrectGuessPlayerUsernames.Add("Player3");

        // Assert
        gameState.CorrectGuessPlayerUsernames.Should().HaveCount(3);
    }

    [Fact]
    public void DrawnLines_ShouldTrackMultipleLines()
    {
        // Arrange
        var gameState = new GameState();
        var line1 = new DrawnLine();
        var line2 = new DrawnLine();

        // Act
        gameState.DrawnLines.Add(line1);
        gameState.DrawnLines.Add(line2);

        // Assert
        gameState.DrawnLines.Should().HaveCount(2);
    }
}
