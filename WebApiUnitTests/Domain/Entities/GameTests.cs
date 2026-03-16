using FluentAssertions;
using WebApi.Domain.Entities;
using Xunit;

namespace WebApiUnitTests.Domain.Entities;

public class GameTests
{
    [Fact]
    public void NewGame_ShouldHaveUniqueTokens()
    {
        // Arrange & Act
        var game1 = new Game();
        var game2 = new Game();

        // Assert
        game1.HostToken.Should().NotBe(game2.HostToken);
        game1.AnnouncementToken.Should().NotBe(game2.AnnouncementToken);
    }

    [Fact]
    public void NewGame_ShouldHaveInitializedState()
    {
        // Arrange & Act
        var game = new Game();

        // Assert
        game.ChatMessages.Should().NotBeNull().And.BeEmpty();
        game.GameSettings.Should().NotBeNull();
        game.GameState.Should().NotBeNull();
        game.GameState.IsGameStarted.Should().BeFalse();
    }
}
