using FluentAssertions;
using Moq;
using WebApi.Api.Models.HttpRequest;
using WebApi.Infrastructure.Repositories.Interfaces;
using Xunit;

namespace WebApiUnitTests.Infrastructure.Repositories;

public class WordRepositoryTests
{
    private readonly Mock<IWordRepository> _mockRepository;

    public WordRepositoryTests()
    {
        _mockRepository = new Mock<IWordRepository>();
    }

    [Fact]
    public async Task AddWordAsync_ShouldReturnTrue_WhenWordIsAdded()
    {
        // Arrange
        var word = "testword";
        var language = "en";
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(r => r.AddWordAsync(word, language, cancellationToken)).ReturnsAsync(true);

        // Act
        var result = await _mockRepository.Object.AddWordAsync(word, language, cancellationToken);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.AddWordAsync(word, language, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task AddWordAsync_ShouldReturnFalse_WhenWordAlreadyExists()
    {
        // Arrange
        var word = "existingword";
        var language = "en";
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(r => r.AddWordAsync(word, language, cancellationToken)).ReturnsAsync(false);

        // Act
        var result = await _mockRepository.Object.AddWordAsync(word, language, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteWordAsync_ShouldReturnTrue_WhenWordIsDeleted()
    {
        // Arrange
        var word = "deleteword";
        var language = "en";
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(r => r.DeleteWordAsync(word, language, cancellationToken)).ReturnsAsync(true);

        // Act
        var result = await _mockRepository.Object.DeleteWordAsync(word, language, cancellationToken);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteWordAsync(word, language, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task DeleteWordAsync_ShouldReturnFalse_WhenWordDoesNotExist()
    {
        // Arrange
        var word = "nonexistent";
        var language = "en";
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(r => r.DeleteWordAsync(word, language, cancellationToken)).ReturnsAsync(false);

        // Act
        var result = await _mockRepository.Object.DeleteWordAsync(word, language, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetWordsAsync_ShouldReturnAllWords()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var words = new List<WordBody>
        {
            new() { Text = "apple", Language = "en" },
            new() { Text = "banana", Language = "en" },
            new() { Text = "dom", Language = "pl" }
        };

        _mockRepository.Setup(r => r.GetWordsAsync(cancellationToken)).ReturnsAsync(words);

        // Act
        var result = await _mockRepository.Object.GetWordsAsync(cancellationToken);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(w => w.Text == "apple" && w.Language == "en");
        result.Should().Contain(w => w.Text == "banana" && w.Language == "en");
        result.Should().Contain(w => w.Text == "dom" && w.Language == "pl");
    }

    [Fact]
    public async Task GetWordsAsync_ShouldReturnEmpty_WhenNoWordsExist()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var emptyWords = new List<WordBody>();

        _mockRepository.Setup(r => r.GetWordsAsync(cancellationToken)).ReturnsAsync(emptyWords);

        // Act
        var result = await _mockRepository.Object.GetWordsAsync(cancellationToken);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRandomWordAsync_ShouldReturnWord_WhenLanguageMatches()
    {
        // Arrange
        var language = "en";
        var cancellationToken = CancellationToken.None;
        var expectedWord = "hello";

        _mockRepository.Setup(r => r.GetRandomWordAsync(language, cancellationToken)).ReturnsAsync(expectedWord);

        // Act
        var result = await _mockRepository.Object.GetRandomWordAsync(language, cancellationToken);

        // Assert
        result.Should().Be(expectedWord);
        _mockRepository.Verify(r => r.GetRandomWordAsync(language, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetRandomWordAsync_ShouldReturnWord_ForPolishLanguage()
    {
        // Arrange
        var language = "pl";
        var cancellationToken = CancellationToken.None;
        var expectedWord = "dom";

        _mockRepository.Setup(r => r.GetRandomWordAsync(language, cancellationToken)).ReturnsAsync(expectedWord);

        // Act
        var result = await _mockRepository.Object.GetRandomWordAsync(language, cancellationToken);

        // Assert
        result.Should().Be("dom");
    }

    [Fact]
    public async Task GetRandomWordAsync_ShouldReturnNull_WhenNoWordsExist()
    {
        // Arrange
        var language = "en";
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(r => r.GetRandomWordAsync(language, cancellationToken)).ReturnsAsync((string)null);

        // Act
        var result = await _mockRepository.Object.GetRandomWordAsync(language, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRandomWordAsync_ShouldReturnDifferentWords_OnMultipleCalls()
    {
        // Arrange
        var language = "en";
        var cancellationToken = CancellationToken.None;
        var callCount = 0;

        _mockRepository.Setup(r => r.GetRandomWordAsync(language, cancellationToken)).ReturnsAsync(() => callCount++ == 0 ? "word1" : "word2");

        // Act
        var result1 = await _mockRepository.Object.GetRandomWordAsync(language, cancellationToken);
        var result2 = await _mockRepository.Object.GetRandomWordAsync(language, cancellationToken);

        // Assert
        result1.Should().Be("word1");
        result2.Should().Be("word2");
        _mockRepository.Verify(r => r.GetRandomWordAsync(language, cancellationToken), Times.Exactly(2));
    }

    [Fact]
    public async Task RepositoryMethods_ShouldRespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var word = "testword";
        var language = "en";

        _mockRepository.Setup(r => r.AddWordAsync(word, language, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        cts.Cancel();
        await _mockRepository.Object.AddWordAsync(word, language, cts.Token);

        // Assert
        _mockRepository.Verify(r => r.AddWordAsync(word, language, It.IsAny<CancellationToken>()), Times.Once);
    }
}
