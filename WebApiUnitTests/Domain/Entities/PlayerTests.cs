using FluentAssertions;
using WebApi.Domain.Entities;
using Xunit;

namespace WebApiUnitTests.Domain.Entities;

public class PlayerTests
{
    [Fact]
    public void NewPlayer_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var player = new Player();

        // Assert
        player.Username.Should().BeEmpty();
        player.Score.Should().Be(0);
        player.Token.Should().BeEmpty();
        player.ConnectionId.Should().BeEmpty();
    }

    [Fact]
    public void Player_ShouldAllowUpdatingProperties()
    {
        // Arrange
        var player = new Player
        {
            Username = "TestPlayer",
            Score = 100,
            Token = "test-token",
            ConnectionId = "connection-123"
        };

        // Act
        player.Score += 50;
        player.ConnectionId = "new-connection";

        // Assert
        player.Score.Should().Be(150);
        player.ConnectionId.Should().Be("new-connection");
    }
}
