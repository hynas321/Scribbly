using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using WebApi.Api.Models.HttpRequest;
using WebApiIntegrationTests.Helpers;
using Xunit;

namespace WebApiIntegrationTests.Controllers;

[Collection("Sql Server Collection")]
public class WordControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly string _connectionString;

    public WordControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClientWithTestAdminToken();
        _connectionString = factory.ConnectionString;

        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        connection.Execute("DELETE FROM Word");
    }

    [Fact]
    public async Task Add_ShouldReturn401_WhenNoAdminToken()
    {
        // Arrange
        var clientWithoutToken = _factory.CreateClient();
        var body = new WordBody { Text = "testword", Language = "en" };

        // Act
        var response = await clientWithoutToken.PostAsJsonAsync("/api/Word/Add", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Add_ShouldReturn401_WhenInvalidAdminToken()
    {
        // Arrange
        var clientWithInvalidToken = _factory.CreateClient();
        clientWithInvalidToken.DefaultRequestHeaders.Add("adminToken", "invalid-token");

        var body = new WordBody { Text = "testword", Language = "en" };

        // Act
        var response = await clientWithInvalidToken.PostAsJsonAsync("/api/Word/Add", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Add_ShouldAddWord_WhenValidAdminToken()
    {
        // Arrange
        var body = new WordBody { Text = "testword", Language = "en" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Word/Add", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        var word = await connection.QueryFirstOrDefaultAsync<WordBody>(
            "SELECT * FROM Word WHERE Text = @Text AND Language = @Language",
            new { body.Text, body.Language });

        word.Should().NotBeNull();
        word?.Text.Should().Be("testword");
    }

    [Fact]
    public async Task Add_ShouldReturn400_WhenInvalidLanguage()
    {
        // Arrange
        var body = new WordBody { Text = "testword", Language = "fr" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Word/Add", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Add_ShouldAddPolishWord()
    {
        // Arrange
        var body = new WordBody { Text = "testowe", Language = "pl" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Word/Add", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        var word = await connection.QueryFirstOrDefaultAsync<WordBody>(
            "SELECT * FROM Word WHERE Text = @Text AND Language = @Language",
            new { body.Text, body.Language });

        word.Should().NotBeNull();
        word?.Language.Should().Be("pl");
    }

    [Fact]
    public async Task Delete_ShouldDeleteWord_WhenValidAdminToken()
    {
        // Arrange
        var body = new WordBody { Text = "deleteword", Language = "en" };

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            await connection.ExecuteAsync(
                "INSERT INTO Word (Text, Language) VALUES (@Text, @Language)",
                body);
        }

        // Act
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/Word/Delete")
        {
            Content = JsonContent.Create(body)
        };
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var connection2 = new SqlConnection(_connectionString);
        connection2.Open();

        var word = await connection2.QueryFirstOrDefaultAsync<WordBody>(
            "SELECT * FROM Word WHERE Text = @Text AND Language = @Language",
            new { body.Text, body.Language });

        word.Should().BeNull();
    }

    [Fact]
    public async Task Delete_ShouldReturn404_WhenWordNotExists()
    {
        // Arrange
        var body = new WordBody { Text = "nonexistent", Language = "en" };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/Word/Delete")
        {
            Content = JsonContent.Create(body)
        };
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetWords_ShouldReturnAllWords_WhenValidAdminToken()
    {
        // Arrange
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            await connection.ExecuteAsync("INSERT INTO Word (Text, Language) VALUES ('word1', 'en')");
            await connection.ExecuteAsync("INSERT INTO Word (Text, Language) VALUES ('word2', 'en')");
            await connection.ExecuteAsync("INSERT INTO Word (Text, Language) VALUES ('word3', 'pl')");
        }

        // Act
        var response = await _client.GetAsync("/api/Word/GetWords");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var words = await response.Content.ReadFromJsonAsync<List<WordBody>>();

        words.Should().NotBeNull();
        words.Should().HaveCountGreaterOrEqualTo(3);
        words.Should().Contain(w => w.Text == "word1" && w.Language == "en");
        words.Should().Contain(w => w.Text == "word2" && w.Language == "en");
        words.Should().Contain(w => w.Text == "word3" && w.Language == "pl");
    }

    [Fact]
    public async Task GetWords_ShouldReturnEmpty_WhenNoWords()
    {
        // Act
        var response = await _client.GetAsync("/api/Word/GetWords");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var words = await response.Content.ReadFromJsonAsync<List<WordBody>>();

        words.Should().NotBeNull();
        words.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWords_ShouldReturn401_WhenNoAdminToken()
    {
        // Arrange
        var clientWithoutToken = _factory.CreateClient();

        // Act
        var response = await clientWithoutToken.GetAsync("/api/Word/GetWords");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
