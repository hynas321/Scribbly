using FluentAssertions;
using Moq;
using WebApi.Domain.Entities;
using WebApi.Infrastructure.Repositories.Interfaces;
using Xunit;

namespace WebApiUnitTests.Infrastructure.Repositories;

public class AccountRepositoryTests
{
    private readonly Mock<IAccountRepository> _mockRepository;

    public AccountRepositoryTests()
    {
        _mockRepository = new Mock<IAccountRepository>();
    }

    [Fact]
    public async Task AddAccountAsync_ShouldReturnTrue_WhenNewAccountIsAdded()
    {
        // Arrange
        var account = new Account();
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(r => r.AddAccountAsync(account, cancellationToken)).ReturnsAsync(true);

        // Act
        var result = await _mockRepository.Object.AddAccountAsync(account, cancellationToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AddAccountAsync_ShouldReturnFalse_WhenAccountAlreadyExists()
    {
        // Arrange
        var account = new Account();
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(r => r.AddAccountAsync(account, cancellationToken)).ReturnsAsync(false);

        // Act
        var result = await _mockRepository.Object.AddAccountAsync(account, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IncrementAccountScoreAsync_ShouldCallRepository()
    {
        // Arrange
        var accessToken = "testToken";
        var scoreIncrement = 10;
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(r => r.IncrementAccountScoreAsync(accessToken, scoreIncrement, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _mockRepository.Object.IncrementAccountScoreAsync(accessToken, scoreIncrement, cancellationToken);

        // Assert
        _mockRepository.Verify(r => r.IncrementAccountScoreAsync(accessToken, scoreIncrement, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetAccountScoreAsync_ShouldReturnScore()
    {
        // Arrange
        var accessToken = "testToken";
        var expectedScore = 100;
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(r => r.GetAccountScoreAsync(accessToken, cancellationToken))
            .ReturnsAsync(expectedScore);

        // Act
        var result = await _mockRepository.Object.GetAccountScoreAsync(accessToken, cancellationToken);

        // Assert
        result.Should().Be(expectedScore);
    }

    [Fact]
    public async Task GetAccountScoreAsync_ShouldReturnZero_WhenAccountNotFound()
    {
        // Arrange
        var accessToken = "nonExistentToken";
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(r => r.GetAccountScoreAsync(accessToken, cancellationToken))
            .ReturnsAsync(0);

        // Act
        var result = await _mockRepository.Object.GetAccountScoreAsync(accessToken, cancellationToken);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetAccountAsync_ShouldReturnAccount_WhenAccountExists()
    {
        // Arrange
        var accountId = "testAccountId";
        var expectedAccount = new Account
        {
            Id = accountId,
            Name = "Test User",
            Score = 50
        };
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(r => r.GetAccountAsync(accountId, cancellationToken)).ReturnsAsync(expectedAccount);

        // Act
        var result = await _mockRepository.Object.GetAccountAsync(accountId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(accountId);
        result.Name.Should().Be("Test User");
        result.Score.Should().Be(50);
    }

    [Fact]
    public async Task GetAccountAsync_ShouldReturnNull_WhenAccountNotFound()
    {
        // Arrange
        var accountId = "nonExistentId";
        var cancellationToken = CancellationToken.None;

        _mockRepository.Setup(r => r.GetAccountAsync(accountId, cancellationToken)).ReturnsAsync((Account)null);

        // Act
        var result = await _mockRepository.Object.GetAccountAsync(accountId, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTopAccountPlayerScoresAsync_ShouldReturnTop5Scores()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var topScores = new List<MainScoreboardScore>
        {
            new() { GivenName = "Player1", Email = "player1@test.com", Score = 100 },
            new() { GivenName = "Player2", Email = "player2@test.com", Score = 90 },
            new() { GivenName = "Player3", Email = "player3@test.com", Score = 80 },
            new() { GivenName = "Player4", Email = "player4@test.com", Score = 70 },
            new() { GivenName = "Player5", Email = "player5@test.com", Score = 60 }
        };

        _mockRepository.Setup(r => r.GetTopAccountPlayerScoresAsync(cancellationToken)).ReturnsAsync(topScores);

        // Act
        var result = await _mockRepository.Object.GetTopAccountPlayerScoresAsync(cancellationToken);

        // Assert
        result.Should().HaveCount(5);
        result.Should().BeInDescendingOrder(s => s.Score);
        result.First().Score.Should().Be(100);
        result.Last().Score.Should().Be(60);
    }

    [Fact]
    public async Task GetTopAccountPlayerScoresAsync_ShouldReturnEmpty_WhenNoAccountsExist()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var emptyScores = Enumerable.Empty<MainScoreboardScore>();

        _mockRepository.Setup(r => r.GetTopAccountPlayerScoresAsync(cancellationToken)).ReturnsAsync(emptyScores);

        // Act
        var result = await _mockRepository.Object.GetTopAccountPlayerScoresAsync(cancellationToken);

        // Assert
        result.Should().BeEmpty();
    }
}
