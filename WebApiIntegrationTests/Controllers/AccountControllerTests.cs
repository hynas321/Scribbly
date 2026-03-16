using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Api.Models.HttpRequest;
using WebApi.Application.Managers.Interfaces;
using WebApi.Domain.Entities;
using WebApiIntegrationTests.Helpers;

namespace WebApiIntegrationTests.Controllers;

[Collection("Sql Server Collection")]
public class AccountControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly string _connectionString;

    public AccountControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _connectionString = factory.ConnectionString;

        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        connection.Execute("DELETE FROM Account");
    }

    [Fact]
    public async Task Add_ShouldCreateAccount_WhenValidData()
    {
        // Arrange
        var account = new Account
        {
            Id = "test-id-1",
            Name = "Test User",
            Score = 0
        };

        var body = new AddAccountBody { Account = account };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Account/Add", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        var dbAccount = await connection.QueryFirstOrDefaultAsync<Account>(
            "SELECT * FROM Account WHERE Id = @Id", new { account.Id });

        dbAccount.Should().NotBeNull();
        dbAccount?.Name.Should().Be("Test User");
    }

    [Fact]
    public async Task Add_ShouldReturn200_WhenAccountAlreadyExists()
    {
        // Arrange
        var account = new Account
        {
            Id = "test-id-1",
            Name = "Test User"
        };

        var body = new AddAccountBody { Account = account };

        await _client.PostAsJsonAsync("/api/Account/Add", body);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Account/Add", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetScore_ShouldReturnScore_WhenAccountExists()
    {
        // Arrange
        var accountId = "test-id-get-score";
        var accountName = "Test User";
        var expectedScore = 100;
        var accessToken = "test-token-get-score";

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        await connection.ExecuteAsync(
            "INSERT INTO Account (Id, AccessToken, Name, GivenName, FamilyName, Email, Score) VALUES (@Id, @AccessToken, @Name, '', '', '', @Score)",
            new { Id = accountId, AccessToken = accessToken, Name = accountName, Score = expectedScore });

        // Act
        var response = await _client.GetAsync($"/api/Account/GetScore/{accountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var score = await response.Content.ReadFromJsonAsync<int>();
        score.Should().Be(expectedScore);
    }

    [Fact]
    public async Task GetScore_ShouldReturn404_WhenAccountNotFound()
    {
        // Arrange
        var nonExistentId = "non-existent-id";

        // Act
        var response = await _client.GetAsync($"/api/Account/GetScore/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task IncrementScore_ShouldUpdateScore_WhenValidToken()
    {
        var account = new Account
        {
            Id = "test-id-1",
            Name = "Test User",
            Score = 50,
            AccessToken = "test-token"
        };

        var createGameBody = new CreateGameBody { Username = "Host" };
        var gameResponse = await _client.PostAsJsonAsync("/api/Game/Create", createGameBody);
        var gameResult = await gameResponse.Content.ReadFromJsonAsync<WebApi.Api.Models.HttpResponse.CreateGameResponse>();

        var player = new Player
        {
            Token = gameResult?.HostToken,
            Score = 25
        };

        var scope = _factory.Services.CreateScope();
        var playerManager = scope.ServiceProvider.GetRequiredService<IPlayerManager>();
        playerManager.AddPlayer(gameResult.GameHash, player);

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        await connection.ExecuteAsync(
            "INSERT INTO Account (Id, AccessToken, Name, GivenName, FamilyName, Email, Score) VALUES (@Id, @AccessToken, @Name, '', '', '', @Score)",
            account);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/Account/IncrementScore/{gameResult.GameHash}");
        request.Headers.Add("token", gameResult.HostToken);
        request.Headers.Add("accessToken", account.AccessToken);
        var incrementResponse = await _client.SendAsync(request);

        // Assert
        incrementResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newScore = await incrementResponse.Content.ReadFromJsonAsync<int>();
        newScore.Should().Be(75);
    }

    [Fact]
    public async Task IncrementScore_ShouldReturn404_WhenPlayerNotFound()
    {
        // Arrange
        var account = new Account
        {
            Id = "test-id-1",
            AccessToken = "test-token"
        };

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        await connection.ExecuteAsync(
            "INSERT INTO Account (Id, AccessToken, Name, GivenName, FamilyName, Score) VALUES (@Id, @AccessToken, @Name, '', '', 0)",
            account);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/Account/IncrementScore/fakehash");
        request.Headers.Add("token", "fake-token");
        request.Headers.Add("accessToken", account.AccessToken);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTop_ShouldReturnTop5OrderedByScore()
    {
        // Arrange
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        var accounts = new List<Account>
        {
            new() { Id = "id1", Name = "User1", Score = 100 },
            new() { Id = "id2", Name = "User2", Score = 200 },
            new() { Id = "id3", Name = "User3", Score = 150 },
            new() { Id = "id4", Name = "User4", Score = 300 },
            new() { Id = "id5", Name = "User5", Score = 50 },
            new() { Id = "id6", Name = "User6", Score = 250 }
        };

        foreach (var account in accounts)
        {
            await connection.ExecuteAsync(
                "INSERT INTO Account (Id, AccessToken, Name, GivenName, FamilyName, Score) VALUES (@Id, @AccessToken, @Name, '', '', @Score)",
                account);
        }

        // Act
        var response = await _client.GetAsync("/api/Account/GetTop");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }
}
