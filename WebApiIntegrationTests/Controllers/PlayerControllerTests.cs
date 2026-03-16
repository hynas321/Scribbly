using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Application.Managers.Interfaces;
using WebApi.Domain.Entities;
using WebApiIntegrationTests.Helpers;
using Xunit;

namespace WebApiIntegrationTests.Controllers;

public class PlayerControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly IGameManager _gameManager;
    private readonly IPlayerManager _playerManager;

    public PlayerControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        var scope = factory.Services.CreateScope();
        _gameManager = scope.ServiceProvider.GetRequiredService<IGameManager>();
        _playerManager = scope.ServiceProvider.GetRequiredService<IPlayerManager>();
    }

    [Fact]
    public async Task Exists_ShouldReturnTrue_WhenPlayerExists()
    {
        // Arrange
        var game = new Game();
        var gameHash = "testGameHash-Exists-Player";
        _gameManager.CreateGame(game, gameHash);

        var player = new Player { Token = "playerToken123" };
        _playerManager.AddPlayer(gameHash, player);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Player/Exists/{gameHash}");
        request.Headers.Add("token", "playerToken123");
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var exists = await response.Content.ReadFromJsonAsync<bool>();
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Exists_ShouldReturnFalse_WhenPlayerNotExists()
    {
        // Arrange
        var game = new Game();
        var gameHash = "testGameHash-NotExists";
        _gameManager.CreateGame(game, gameHash);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Player/Exists/{gameHash}");
        request.Headers.Add("token", "nonExistentToken");
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var exists = await response.Content.ReadFromJsonAsync<bool>();
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Exists_ShouldReturn404_WhenGameNotExists()
    {
        // Arrange
        var nonExistentHash = "nonExistentHash";

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Player/Exists/{nonExistentHash}");
        request.Headers.Add("token", "someToken");
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Exists_ShouldReturnTrue_WhenMultiplePlayersExist()
    {
        // Arrange
        var game = new Game();
        var gameHash = "testGameHash-Multiple";
        _gameManager.CreateGame(game, gameHash);

        var player1 = new Player { Token = "player1" };
        var player2 = new Player { Token = "player2" };
        var player3 = new Player { Token = "player3" };

        _playerManager.AddPlayer(gameHash, player1);
        _playerManager.AddPlayer(gameHash, player2);
        _playerManager.AddPlayer(gameHash, player3);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Player/Exists/{gameHash}");
        request.Headers.Add("token", "player2");
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var exists = await response.Content.ReadFromJsonAsync<bool>();
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Exists_ShouldWorkAfterPlayerRemoval()
    {
        // Arrange
        var game = new Game();
        var gameHash = "testGameHash-Removal";
        _gameManager.CreateGame(game, gameHash);

        var player = new Player { Token = "playerToken" };
        _playerManager.AddPlayer(gameHash, player);
        _playerManager.RemovePlayer(gameHash, player.Token);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Player/Exists/{gameHash}");
        request.Headers.Add("token", player.Token);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var exists = await response.Content.ReadFromJsonAsync<bool>();
        exists.Should().BeFalse();
    }
}
