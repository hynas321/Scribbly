using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Api.Models.HttpRequest;
using WebApi.Api.Models.HttpResponse;
using WebApi.Application.Managers.Interfaces;
using WebApiIntegrationTests.Helpers;
using Xunit;

namespace WebApiIntegrationTests.Controllers;

public class GameControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public GameControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_ShouldReturnGameHashAndToken_WhenValidUsername()
    {
        // Arrange
        var body = new CreateGameBody { Username = "TestPlayer" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Game/Create", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateGameResponse>();
        result.Should().NotBeNull();
        result?.GameHash.Should().NotBeNullOrEmpty();
        result?.GameHash.Length.Should().Be(8);
        result?.HostToken.Should().NotBeNullOrEmpty();
        result?.HostToken.Length.Should().Be(16);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenInvalidUsername_Empty()
    {
        // Arrange
        var body = new CreateGameBody { Username = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Game/Create", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenInvalidUsername_TooLong()
    {
        // Arrange
        var body = new CreateGameBody { Username = new string('A', 19) }; // Max is 18

        // Act
        var response = await _client.PostAsJsonAsync("/api/Game/Create", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenInvalidUsername_Null()
    {
        // Arrange
        var body = new CreateGameBody { Username = null };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Game/Create", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Exists_ShouldReturnTrue_WhenGameExists()
    {
        // Arrange
        var body = new CreateGameBody { Username = "TestPlayer" };
        var createResponse = await _client.PostAsJsonAsync("/api/Game/Create", body);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateGameResponse>();

        // Act
        var response = await _client.GetAsync($"/api/Game/Exists/{createResult.GameHash}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var exists = await response.Content.ReadFromJsonAsync<bool>();
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Exists_ShouldReturnFalse_WhenGameNotExists()
    {
        // Arrange
        var nonExistentHash = "nonexistent";

        // Act
        var response = await _client.GetAsync($"/api/Game/Exists/{nonExistentHash}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var exists = await response.Content.ReadFromJsonAsync<bool>();
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Create_ShouldCreateGameInRepository()
    {
        // Arrange
        var scope = _factory.Services.CreateScope();
        var gameManager = scope.ServiceProvider.GetRequiredService<IGameManager>();
        var body = new CreateGameBody { Username = "TestPlayer" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Game/Create", body);
        var result = await response.Content.ReadFromJsonAsync<CreateGameResponse>();

        // Assert
        var game = gameManager.GetGame(result.GameHash);
        game.Should().NotBeNull();
        game.GameState.HostPlayerUsername.Should().Be("TestPlayer");
    }

    [Fact]
    public async Task Create_ShouldCreateUniqueGameHashes()
    {
        // Arrange
        var body = new CreateGameBody { Username = "TestPlayer" };

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/Game/Create", body);
        var response2 = await _client.PostAsJsonAsync("/api/Game/Create", body);
        var result1 = await response1.Content.ReadFromJsonAsync<CreateGameResponse>();
        var result2 = await response2.Content.ReadFromJsonAsync<CreateGameResponse>();

        // Assert
        result1.GameHash.Should().NotBe(result2.GameHash);
        result1.HostToken.Should().NotBe(result2.HostToken);
    }
}
