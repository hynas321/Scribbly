using FluentAssertions;
using Moq;
using WebApi.Application.Managers.Interfaces;
using WebApi.Application.Services;
using WebApi.Domain.Entities;
using WebApi.Domain.Static;
using WebApi.Infrastructure.Repositories.Interfaces;
using Xunit;

namespace WebApiUnitTests.Application.Services;

public class RandomWordServiceTests
{
    private readonly Mock<IWordRepository> _mockWordRepository;
    private readonly Mock<IGameManager> _mockGameManager;
    private readonly RandomWordService _service;
    private readonly string _gameHash = "testGameHash";
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public RandomWordServiceTests()
    {
        _mockWordRepository = new Mock<IWordRepository>();
        _mockGameManager = new Mock<IGameManager>();
        _service = new RandomWordService(_mockWordRepository.Object, _mockGameManager.Object);
    }

    [Fact]
    public async Task FetchWordAsync_ShouldReturnEnglishWord_WhenLanguageIsEn()
    {
        // Arrange
        var game = new Game
        {
            GameSettings = new GameSettings { WordLanguage = Languages.EN }
        };

        _mockGameManager.Setup(m => m.GetGame(_gameHash)).Returns(game);
        _mockWordRepository.Setup(r => r.GetRandomWordAsync(Languages.EN, _cancellationToken)).ReturnsAsync("hello");

        // Act
        var result = await _service.FetchWordAsync(_gameHash, _cancellationToken);

        // Assert
        result.Should().Be("hello");
        _mockWordRepository.Verify(r => r.GetRandomWordAsync(Languages.EN, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task FetchWordAsync_ShouldReturnPolishWord_WhenLanguageIsPl()
    {
        // Arrange
        var game = new Game
        {
            GameSettings = new GameSettings { WordLanguage = Languages.PL }
        };

        _mockGameManager.Setup(m => m.GetGame(_gameHash)).Returns(game);
        _mockWordRepository.Setup(r => r.GetRandomWordAsync(Languages.PL, _cancellationToken)).ReturnsAsync("dom");

        // Act
        var result = await _service.FetchWordAsync(_gameHash, _cancellationToken);

        // Assert
        result.Should().Be("dom");
        _mockWordRepository.Verify(r => r.GetRandomWordAsync(Languages.PL, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task FetchWordAsync_ShouldFallbackToEnglish_WhenLanguageIsNotRecognized()
    {
        // Arrange
        var game = new Game
        {
            GameSettings = new GameSettings { WordLanguage = "fr" }
        };

        _mockGameManager.Setup(m => m.GetGame(_gameHash)).Returns(game);
        _mockWordRepository.Setup(r => r.GetRandomWordAsync(Languages.EN, _cancellationToken)).ReturnsAsync("hello");

        // Act
        var result = await _service.FetchWordAsync(_gameHash, _cancellationToken);

        // Assert
        result.Should().Be("hello");
        _mockWordRepository.Verify(r => r.GetRandomWordAsync(Languages.EN, _cancellationToken), Times.Once);
        _mockWordRepository.Verify(r => r.GetRandomWordAsync(It.Is<string>(s => s != Languages.EN), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FetchWordAsync_ShouldReturnNull_WhenNoWordsAvailable()
    {
        // Arrange
        var game = new Game
        {
            GameSettings = new GameSettings { WordLanguage = Languages.EN }
        };

        _mockGameManager.Setup(m => m.GetGame(_gameHash)).Returns(game);
        _mockWordRepository.Setup(r => r.GetRandomWordAsync(Languages.EN, _cancellationToken)).ReturnsAsync((string)null);

        // Act
        var result = await _service.FetchWordAsync(_gameHash, _cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FetchWordAsync_ShouldThrowNullReferenceException_WhenGameIsNull()
    {
        // Arrange
        _mockGameManager.Setup(m => m.GetGame(_gameHash)).Returns((Game)null);

        // Act
        Func<Task> act = async () => await _service.FetchWordAsync(_gameHash, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<NullReferenceException>();
    }
}
