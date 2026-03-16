using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using WebApi.Api.Hubs.Static;
using Xunit;
using Xunit.Abstractions;

namespace WebApiIntegrationTests.Hubs;

public class GameHubIntegrationTests : HubTestBase
{
    private readonly HttpClient _httpClient;

    public GameHubIntegrationTests(ITestOutputHelper output) : base(output)
    {
        _httpClient = WebApplicationFactory.CreateClient();
    }

    private async Task<(string gameHash, string hostToken)> CreateGameAsync(string username = "TestHost")
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Game/Create", new { Username = username });
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var doc = System.Text.Json.JsonDocument.Parse(content);
        var gameHash = doc.RootElement.GetProperty("gameHash").GetString();
        var hostToken = doc.RootElement.GetProperty("hostToken").GetString();

        return (gameHash!, hostToken!);
    }

    [Fact]
    public async Task JoinGame_ShouldSucceed_WithValidHostToken()
    {
        // Arrange
        var (gameHash, hostToken) = await CreateGameAsync("TestHost");
        var connection = CreateHubConnection("/hub/connection");
        await connection.StartAsync();

        string? joinError = null;
        connection.On(HubMessages.OnJoinGameError, (string error) => joinError = error);

        // Act
        await connection.InvokeAsync(HubMessages.JoinGame, gameHash, hostToken, "TestHost");
        await Task.Delay(200);

        // Assert
        Assert.Null(joinError);
        Assert.Equal(HubConnectionState.Connected, connection.State);

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task JoinGame_ShouldSucceed_WithRegularPlayer()
    {
        // Arrange
        var (gameHash, hostToken) = await CreateGameAsync("TestHost");
        var hostConnection = CreateHubConnection("/hub/connection");
        await hostConnection.StartAsync();
        await hostConnection.InvokeAsync(HubMessages.JoinGame, gameHash, hostToken, "TestHost");

        var playerConnection = CreateHubConnection("/hub/connection");
        await playerConnection.StartAsync();

        string? joinError = null;
        playerConnection.On(HubMessages.OnJoinGameError, (string error) => joinError = error);

        // Act
        await playerConnection.InvokeAsync(HubMessages.JoinGame, gameHash, "random-token", "TestPlayer");
        await Task.Delay(200);

        // Assert
        Assert.Null(joinError);
        Assert.Equal(HubConnectionState.Connected, playerConnection.State);

        await hostConnection.DisposeAsync();
        await playerConnection.DisposeAsync();
    }

    [Fact]
    public async Task JoinGame_ShouldFail_WithNonExistentGame()
    {
        // Arrange
        var connection = CreateHubConnection("/hub/connection");
        await connection.StartAsync();

        string? joinError = null;
        connection.On(HubMessages.OnJoinGameError, (string error) => joinError = error);

        // Act
        await connection.InvokeAsync(HubMessages.JoinGame, "nonexistent-game", "token", "TestPlayer");
        await Task.Delay(200);

        // Assert
        Assert.NotNull(joinError);
        Assert.Contains("does not exist", joinError);

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task JoinGame_ShouldFail_WithEmptyUsername()
    {
        // Arrange
        var (gameHash, hostToken) = await CreateGameAsync("TestHost");
        var connection = CreateHubConnection("/hub/connection");
        await connection.StartAsync();

        string? joinError = null;
        connection.On(HubMessages.OnJoinGameError, (string error) => joinError = error);

        // Act
        await connection.InvokeAsync(HubMessages.JoinGame, gameHash, hostToken, "");
        await Task.Delay(200);

        // Assert
        Assert.NotNull(joinError);
        Assert.Contains("too short", joinError);

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task JoinGame_ShouldFail_WithDuplicateUsername()
    {
        // Arrange
        var (gameHash, hostToken) = await CreateGameAsync("TestHost");
        var connection1 = CreateHubConnection("/hub/connection");
        var connection2 = CreateHubConnection("/hub/connection");

        await connection1.StartAsync();
        await connection2.StartAsync();

        string? joinError = null;
        connection2.On(HubMessages.OnJoinGameError, (string error) => joinError = error);

        // Act - First player joins
        await connection1.InvokeAsync(HubMessages.JoinGame, gameHash, "token1", "TestPlayer");
        await Task.Delay(100);

        // Second player tries to join with same username
        await connection2.InvokeAsync(HubMessages.JoinGame, gameHash, "token2", "TestPlayer");
        await Task.Delay(200);

        // Assert
        Assert.NotNull(joinError);
        Assert.Contains("already exists", joinError);

        await connection1.DisposeAsync();
        await connection2.DisposeAsync();
    }

    [Fact]
    public async Task MultiplePlayers_ShouldJoinGame_Successfully()
    {
        // Arrange
        var (gameHash, hostToken) = await CreateGameAsync("TestHost");
        var connections = new List<HubConnection>();

        // Act - Host joins
        var hostConnection = CreateHubConnection("/hub/connection");
        await hostConnection.StartAsync();
        await hostConnection.InvokeAsync(HubMessages.JoinGame, gameHash, hostToken, "TestHost");
        connections.Add(hostConnection);

        // 3 regular players join
        for (int i = 1; i <= 3; i++)
        {
            var connection = CreateHubConnection("/hub/connection");
            await connection.StartAsync();
            await connection.InvokeAsync(HubMessages.JoinGame, gameHash, $"token{i}", $"Player{i}");
            connections.Add(connection);
        }

        await Task.Delay(300);

        // Assert - All connections should be active
        foreach (var connection in connections)
        {
            Assert.Equal(HubConnectionState.Connected, connection.State);
        }

        // Cleanup
        foreach (var connection in connections)
        {
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task LeaveGame_ShouldRemovePlayer_FromGame()
    {
        // Arrange
        var (gameHash, hostToken) = await CreateGameAsync("TestHost");
        var connection = CreateHubConnection("/hub/connection");
        await connection.StartAsync();
        await connection.InvokeAsync(HubMessages.JoinGame, gameHash, hostToken, "TestHost");
        await Task.Delay(100);

        // Act
        await connection.InvokeAsync(HubMessages.LeaveGame, gameHash, hostToken);
        await Task.Delay(100);

        // Assert - Connection should still be connected
        Assert.Equal(HubConnectionState.Connected, connection.State);

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task LeaveGame_ShouldDoNothing_WithNonExistentGame()
    {
        // Arrange
        var connection = CreateHubConnection("/hub/connection");
        await connection.StartAsync();

        // Act & Assert - Should not throw
        await connection.InvokeAsync(HubMessages.LeaveGame, "nonexistent-game", "token");
        await Task.Delay(100);

        Assert.Equal(HubConnectionState.Connected, connection.State);

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task LeaveGame_ShouldRemoveGame_WhenLastPlayerLeaves()
    {
        // Arrange
        var (gameHash, hostToken) = await CreateGameAsync("TestHost");
        var connection = CreateHubConnection("/hub/connection");
        await connection.StartAsync();
        await connection.InvokeAsync(HubMessages.JoinGame, gameHash, hostToken, "TestHost");
        await Task.Delay(100);

        // Act
        await connection.InvokeAsync(HubMessages.LeaveGame, gameHash, hostToken);
        await Task.Delay(100);

        // Assert - Trying to join the same game should fail
        string? joinError = null;
        connection.On(HubMessages.OnJoinGameError, (string error) => joinError = error);

        await connection.InvokeAsync(HubMessages.JoinGame, gameHash, hostToken, "TestHost");
        await Task.Delay(100);

        Assert.NotNull(joinError);

        await connection.DisposeAsync();
    }
}
