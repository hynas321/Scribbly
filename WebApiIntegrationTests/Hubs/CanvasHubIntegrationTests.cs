using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using WebApi.Api.Hubs.Static;
using Xunit;
using Xunit.Abstractions;

namespace WebApiIntegrationTests.Hubs;

public class CanvasHubIntegrationTests : HubTestBase
{
    private readonly HttpClient _httpClient;

    public CanvasHubIntegrationTests(ITestOutputHelper output) : base(output)
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
    public async Task LoadCanvas_ShouldWork_ForExistingGame()
    {
        // Arrange
        var (gameHash, hostToken) = await CreateGameAsync("TestHost");
        var connection = CreateHubConnection("/hub/connection");
        await connection.StartAsync();
        await connection.InvokeAsync(HubMessages.JoinGame, gameHash, hostToken, "TestHost");
        await Task.Delay(100);

        string? canvasData = null;
        connection.On(HubMessages.OnLoadCanvas, (string data) => canvasData = data);

        // Act
        await connection.InvokeAsync(HubMessages.LoadCanvas, gameHash);
        await Task.Delay(100);

        // Assert
        Assert.NotNull(canvasData);

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task LoadCanvas_ShouldFail_ForNonExistentGame()
    {
        // Arrange
        var connection = CreateHubConnection("/hub/connection");
        await connection.StartAsync();

        string? canvasData = null;
        connection.On(HubMessages.OnLoadCanvas, (string data) => canvasData = data);

        // Act
        await connection.InvokeAsync(HubMessages.LoadCanvas, "nonexistent-game");
        await Task.Delay(100);

        // Assert
        Assert.Null(canvasData);

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task LoadCanvas_ShouldReturnEmptyData_ForNewGame()
    {
        // Arrange
        var (gameHash, hostToken) = await CreateGameAsync("TestHost");
        var connection = CreateHubConnection("/hub/connection");
        await connection.StartAsync();
        await connection.InvokeAsync(HubMessages.JoinGame, gameHash, hostToken, "TestHost");
        await Task.Delay(100);

        string? canvasData = null;
        connection.On(HubMessages.OnLoadCanvas, (string data) => canvasData = data);

        // Act
        await connection.InvokeAsync(HubMessages.LoadCanvas, gameHash);
        await Task.Delay(100);

        // Assert
        Assert.NotNull(canvasData);
        Assert.Contains("[]", canvasData);

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task MultipleConnections_ShouldLoadCanvas_Successfully()
    {
        // Arrange
        var (gameHash, hostToken) = await CreateGameAsync("TestHost");

        var connection1 = CreateHubConnection("/hub/connection");
        var connection2 = CreateHubConnection("/hub/connection");

        await connection1.StartAsync();
        await connection2.StartAsync();

        await connection1.InvokeAsync(HubMessages.JoinGame, gameHash, hostToken, "TestHost");
        await connection2.InvokeAsync(HubMessages.JoinGame, gameHash, "player-token", "Player");
        await Task.Delay(100);

        var canvasData1 = "";
        var canvasData2 = "";

        connection1.On(HubMessages.OnLoadCanvas, (string data) => canvasData1 = data);
        connection2.On(HubMessages.OnLoadCanvas, (string data) => canvasData2 = data);

        // Act
        await connection1.InvokeAsync(HubMessages.LoadCanvas, gameHash);
        await connection2.InvokeAsync(HubMessages.LoadCanvas, gameHash);
        await Task.Delay(100);

        // Assert
        Assert.NotNull(canvasData1);
        Assert.NotNull(canvasData2);

        await connection1.DisposeAsync();
        await connection2.DisposeAsync();
    }
}
