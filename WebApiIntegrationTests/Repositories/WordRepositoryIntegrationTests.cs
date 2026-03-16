using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WebApi.Api.Models.HttpRequest;
using WebApi.Infrastructure.Repositories;
using WebApi.Infrastructure.Repositories.Interfaces;
using WebApiIntegrationTests.Helpers;
using Xunit;

namespace WebApiIntegrationTests.Repositories;

[Collection("Sql Server Collection")]
public class WordRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly string _connectionString;
    private readonly IWordRepository _repository;
    private readonly IConfiguration _configuration;

    public WordRepositoryIntegrationTests(SqlServerCollectionFixture sqlFixture)
    {
        _connectionString = sqlFixture.ConnectionString;
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DatabaseConnectionString"] = _connectionString
            })
            .Build();

        _repository = new WordRepository(_configuration);
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("DELETE FROM Word");

        await connection.ExecuteAsync("INSERT INTO Word (Text, Language) VALUES ('apple', 'en')");
        await connection.ExecuteAsync("INSERT INTO Word (Text, Language) VALUES ('banana', 'en')");
        await connection.ExecuteAsync("INSERT INTO Word (Text, Language) VALUES (N'dom', 'pl')");
    }

    public async Task DisposeAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("DELETE FROM Word");
    }

    [Fact]
    public async Task AddWordAsync_ShouldInsertNewWord()
    {
        // Arrange
        var text = "computer";
        var language = "en";

        // Act
        var result = await _repository.AddWordAsync(text, language, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var word = await connection.QueryFirstOrDefaultAsync<WordBody>(
            "SELECT * FROM Word WHERE Text = @Text AND Language = @Language",
            new { Text = text, Language = language });

        word.Should().NotBeNull();
        word?.Text.Should().Be(text);
        word?.Language.Should().Be(language);
    }

    [Fact]
    public async Task AddWordAsync_ShouldInsertPolishWord()
    {
        // Arrange
        var text = "komputer";
        var language = "pl";

        // Act
        var result = await _repository.AddWordAsync(text, language, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var word = await connection.QueryFirstOrDefaultAsync<WordBody>(
            "SELECT * FROM Word WHERE Text = @Text AND Language = @Language",
            new { Text = text, Language = language });

        word.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteWordAsync_ShouldDeleteWord()
    {
        // Arrange
        var text = "apple";
        var language = "en";

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var word = await connection.QueryFirstOrDefaultAsync<WordBody>(
                "SELECT * FROM Word WHERE Text = @Text AND Language = @Language",
                new { Text = text, Language = language });

            word.Should().NotBeNull();
        }

        // Act
        var result = await _repository.DeleteWordAsync(text, language, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        using var connection2 = new SqlConnection(_connectionString);
        await connection2.OpenAsync();

        var deletedWord = await connection2.QueryFirstOrDefaultAsync<WordBody>(
            "SELECT * FROM Word WHERE Text = @Text AND Language = @Language",
            new { Text = text, Language = language });

        deletedWord.Should().BeNull();
    }

    [Fact]
    public async Task DeleteWordAsync_ShouldReturnFalse_WhenWordNotFound()
    {
        // Arrange
        var text = "nonexistent";
        var language = "en";

        // Act
        var result = await _repository.DeleteWordAsync(text, language, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetWordsAsync_ShouldReturnAllWords()
    {
        // Act
        var result = await _repository.GetWordsAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterOrEqualTo(3);
        result.Should().Contain(w => w.Text == "apple" && w.Language == "en");
        result.Should().Contain(w => w.Text == "banana" && w.Language == "en");
        result.Should().Contain(w => w.Text == "dom" && w.Language == "pl");
    }

    [Fact]
    public async Task GetWordsAsync_ShouldReturnEmpty_WhenNoWords()
    {
        // Arrange
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync("DELETE FROM Word");
        }

        // Act
        var result = await _repository.GetWordsAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRandomWordAsync_ShouldReturnWord_WhenLanguageMatches()
    {
        // Act
        var result = await _repository.GetRandomWordAsync("en", CancellationToken.None);

        // Assert
        result.Should().NotBeNullOrEmpty();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var word = await connection.QueryFirstOrDefaultAsync<string>(
            "SELECT Text FROM Word WHERE Text = @Text AND Language = 'en'",
            new { Text = result });

        word.Should().Be(result);
    }

    [Fact]
    public async Task GetRandomWordAsync_ShouldReturnPolishWord_WhenLanguageIsPl()
    {
        // Act
        var result = await _repository.GetRandomWordAsync("pl", CancellationToken.None);

        // Assert
        result.Should().Be("dom");
    }

    [Fact]
    public async Task GetRandomWordAsync_ShouldReturnNull_WhenNoWordsForLanguage()
    {
        // Arrange
        var language = "de";

        // Act
        var result = await _repository.GetRandomWordAsync(language, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRandomWordAsync_ShouldReturnDifferentWords_OnMultipleCalls()
    {
        // Arrange
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync("INSERT INTO Word (Text, Language) VALUES ('cherry', 'en')");
            await connection.ExecuteAsync("INSERT INTO Word (Text, Language) VALUES ('date', 'en')");
            await connection.ExecuteAsync("INSERT INTO Word (Text, Language) VALUES ('elderberry', 'en')");
        }

        // Act
        var words = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            var word = await _repository.GetRandomWordAsync("en", CancellationToken.None);
            words.Add(word);
        }

        // Assert
        words.Should().HaveCount(10);
        words.Distinct().Should().HaveCountGreaterOrEqualTo(2, "random words should vary");
    }

    [Fact]
    public async Task WordLifecycle_AddGetDelete_ShouldWorkCorrectly()
    {
        var text = "tempword";
        var language = "en";

        var addResult = await _repository.AddWordAsync(text, language, CancellationToken.None);
        addResult.Should().BeTrue();

        var words = await _repository.GetWordsAsync(CancellationToken.None);
        words.Should().Contain(w => w.Text == text);

        var deleteResult = await _repository.DeleteWordAsync(text, language, CancellationToken.None);
        deleteResult.Should().BeTrue();

        words = await _repository.GetWordsAsync(CancellationToken.None);
        words.Should().NotContain(w => w.Text == text);
    }

    [Fact]
    public async Task AddWordAsync_ShouldHandleDuplicateWords()
    {
        // Arrange
        var text = "duplicate";
        var language = "en";

        // Act
        var result1 = await _repository.AddWordAsync(text, language, CancellationToken.None);
        var result2 = await _repository.AddWordAsync(text, language, CancellationToken.None);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
    }
}
