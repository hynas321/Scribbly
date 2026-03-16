using FluentAssertions;
using WebApi.Domain.Entities;
using Xunit;

namespace WebApiUnitTests.Domain.Entities;

public class GameSettingsTests
{
    [Fact]
    public void NewGameSettings_ShouldHaveDefaultValues()
    {
        // Arrange and Act
        var settings = new GameSettings();

        // Assert
        settings.DrawingTimeSeconds.Should().Be(75);
        settings.RoundsCount.Should().Be(6);
        settings.WordLanguage.Should().Be("en");
    }

    [Fact]
    public void GameSettings_ShouldAllowCustomConfiguration()
    {
        // Arrange and Act
        var settings = new GameSettings
        {
            DrawingTimeSeconds = 90,
            RoundsCount = 10,
            WordLanguage = "pl"
        };

        // Assert
        settings.DrawingTimeSeconds.Should().Be(90);
        settings.RoundsCount.Should().Be(10);
        settings.WordLanguage.Should().Be("pl");
    }
}
