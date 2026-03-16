using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WebApi.Domain.Entities;
using WebApi.Infrastructure.Repositories;
using WebApi.Infrastructure.Repositories.Interfaces;
using WebApiIntegrationTests.Helpers;

namespace WebApiIntegrationTests.Repositories;

[Collection("Sql Server Collection")]
public class AccountRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly string _connectionString;
    private readonly IAccountRepository _repository;
    private readonly IConfiguration _configuration;

    public AccountRepositoryIntegrationTests(SqlServerCollectionFixture sqlFixture)
    {
        _connectionString = sqlFixture.ConnectionString;

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DatabaseConnectionString"] = _connectionString
            })
            .Build();

        _repository = new AccountRepository(_configuration);
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("DELETE FROM Account");
    }

    public async Task DisposeAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("DELETE FROM Account");
    }

    [Fact]
    public async Task AddAccountAsync_ShouldInsertNewAccount()
    {
        // Arrange
        var account = new Account
        {
            Id = "test-id-1",
            Name = "Test User",
            Score = 100
        };

        // Act
        var result = await _repository.AddAccountAsync(account, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var dbAccount = await connection.QueryFirstOrDefaultAsync<Account>(
            "SELECT * FROM Account WHERE Id = @Id", new { account.Id });

        dbAccount.Should().NotBeNull();
        dbAccount?.Name.Should().Be("Test User");
        dbAccount?.Score.Should().Be(100);
    }

    [Fact]
    public async Task AddAccountAsync_ShouldUpdateExistingAccount()
    {
        // Arrange
        var account1 = new Account
        {
            Id = "test-id-1",
            Name = "Original Name",
            AccessToken = "token1"
        };

        var account2 = new Account
        {
            Id = "test-id-1",
            Name = "Updated Name",
            AccessToken = "token2"
        };

        // Act
        await _repository.AddAccountAsync(account1, CancellationToken.None);
        var result = await _repository.AddAccountAsync(account2, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var dbAccount = await connection.QueryFirstOrDefaultAsync<Account>(
            "SELECT * FROM Account WHERE Id = @Id", new { account2.Id });

        dbAccount.Should().NotBeNull();
        dbAccount?.Name.Should().Be("Original Name");
        dbAccount?.AccessToken.Should().Be("token2");
    }

    [Fact]
    public async Task GetAccountAsync_ShouldReturnAccount_WhenExists()
    {
        // Arrange
        var account = new Account
        {
            Id = "test-id-1",
            Name = "Test User",
            Score = 250
        };

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                "INSERT INTO Account (Id, AccessToken, Name, GivenName, FamilyName, Score) VALUES (@Id, @AccessToken, @Name, '', '', @Score)",
                account);
        }

        // Act
        var result = await _repository.GetAccountAsync(account.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(account.Id);
        result.Name.Should().Be("Test User");
        result.Score.Should().Be(250);
    }

    [Fact]
    public async Task GetAccountAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetAccountAsync("non-existent-id", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IncrementAccountScoreAsync_ShouldIncrementFromZero()
    {
        // Arrange
        var account = new Account
        {
            Id = "test-id-1",
            AccessToken = "test-token",
            Score = 0
        };

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                "INSERT INTO Account (Id, AccessToken, Name, GivenName, FamilyName, Score) VALUES (@Id, @AccessToken, @Name, '', '', @Score)",
                account);
        }

        // Act
        await _repository.IncrementAccountScoreAsync("test-token", 50, CancellationToken.None);

        // Assert
        using var connection2 = new SqlConnection(_connectionString);
        await connection2.OpenAsync();

        var score = await connection2.QuerySingleAsync<int>(
            "SELECT Score FROM Account WHERE AccessToken = @AccessToken",
            new { AccessToken = "test-token" });

        score.Should().Be(50);
    }

    [Fact]
    public async Task IncrementAccountScoreAsync_ShouldAddToExistingScore()
    {
        // Arrange
        var account = new Account
        {
            Id = "test-id-1",
            AccessToken = "test-token",
            Score = 100
        };

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                "INSERT INTO Account (Id, AccessToken, Name, GivenName, FamilyName, Score) VALUES (@Id, @AccessToken, @Name, '', '', @Score)",
                account);
        }

        // Act
        await _repository.IncrementAccountScoreAsync("test-token", 75, CancellationToken.None);

        // Assert
        using var connection2 = new SqlConnection(_connectionString);
        await connection2.OpenAsync();

        var score = await connection2.QuerySingleAsync<int>(
            "SELECT Score FROM Account WHERE AccessToken = @AccessToken",
            new { AccessToken = "test-token" });

        score.Should().Be(175);
    }

    [Fact]
    public async Task GetAccountScoreAsync_ShouldReturnScore()
    {
        // Arrange
        var account = new Account
        {
            Id = "test-id-1",
            AccessToken = "test-token",
            Score = 42
        };

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                "INSERT INTO Account (Id, AccessToken, Name, GivenName, FamilyName, Score) VALUES (@Id, @AccessToken, @Name, '', '', @Score)",
                account);
        }

        // Act
        var score = await _repository.GetAccountScoreAsync("test-token", CancellationToken.None);

        // Assert
        score.Should().Be(42);
    }

    [Fact]
    public async Task GetAccountScoreAsync_ShouldReturnZero_WhenAccountNotFound()
    {
        // Act
        var score = await _repository.GetAccountScoreAsync("non-existent-token", CancellationToken.None);

        // Assert
        score.Should().Be(0);
    }

    [Fact]
    public async Task GetTopAccountPlayerScoresAsync_ShouldReturnTop5Ordered()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = "id1", Name = "User1", Score = 100 },
            new() { Id = "id2", Name = "User2", Score = 300 },
            new() { Id = "id3", Name = "User3", Score = 200 },
            new() { Id = "id4", Name = "User4", Score = 500 },
            new() { Id = "id5", Name = "User5", Score = 400 },
            new() { Id = "id6", Name = "User6", Score = 150 }
        };

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            foreach (var account in accounts)
            {
                await connection.ExecuteAsync(
                    "INSERT INTO Account (Id, AccessToken, Name, GivenName, FamilyName, Email, Score) VALUES (@Id, @AccessToken, @Name, '', '', '', @Score)",
                    account);
            }
        }

        // Act
        var result = await _repository.GetTopAccountPlayerScoresAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(5);
        result.Should().BeInDescendingOrder(s => s.Score);
        result.First().Score.Should().Be(500);
        result.Last().Score.Should().Be(150);
    }

    [Fact]
    public async Task GetTopAccountPlayerScoresAsync_ShouldReturnEmpty_WhenNoAccounts()
    {
        // Act
        var result = await _repository.GetTopAccountPlayerScoresAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
