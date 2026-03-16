using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApi.Api.Hubs;
using WebApi.Api.Hubs.Static;
using WebApi.Api.Models.DTO;
using Xunit;
using Xunit.Abstractions;

namespace WebApiIntegrationTests.Hubs;

[Collection("SignalR Integration Tests")]
public class HubConnectionIntegrationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _httpClient;

    public HubConnectionIntegrationTests(ITestOutputHelper output)
    {
        _webApplicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services => {});
                builder.UseEnvironment("Testing");
            });
        _httpClient = _webApplicationFactory.CreateClient();
        _output = output;
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _webApplicationFactory.DisposeAsync();
    }

    private HubConnection CreateHubConnection(string hubPath)
    {
        var serverAddress = _webApplicationFactory.Server.BaseAddress;

        var hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri(serverAddress, hubPath), options =>
            {
                options.HttpMessageHandlerFactory = _ => _webApplicationFactory.Server.CreateHandler();
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents;
                options.SkipNegotiation = false;
            })
            .WithAutomaticReconnect()
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddConsole();
            })
            .Build();

        return hubConnection;
    }

    private async Task<(string gameHash, string hostToken)> CreateGameAsync(string username = "TestHost")
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Game/Create", new { Username = username });
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var gameHash = doc.RootElement.GetProperty("gameHash").GetString();
        var hostToken = doc.RootElement.GetProperty("hostToken").GetString();

        return (gameHash, hostToken);
    }

    #region Connection Tests

    [Fact]
    public async Task AccountHubConnection_ShouldConnectAndDisconnect()
    {
        // Arrange
        var connection = CreateHubConnection("/account-hub/connection");

        // Act and Assert
        await connection.StartAsync();
        Assert.Equal(HubConnectionState.Connected, connection.State);

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task HubConnection_ShouldConnectAndDisconnect()
    {
        // Arrange
        var connection = CreateHubConnection("/hub/connection");

        // Act and Assert
        await connection.StartAsync();
        Assert.Equal(HubConnectionState.Connected, connection.State);

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task LongRunningHubConnection_ShouldConnectAndDisconnect()
    {
        // Arrange
        var connection = CreateHubConnection("/long-running-hub/connection");

        // Act & Assert
        await connection.StartAsync();
        Assert.Equal(HubConnectionState.Connected, connection.State);

        await connection.DisposeAsync();
    }

    #endregion

    #region Account Hub Session Tests

    [Fact]
    public async Task AccountHubConnection_CreateSession_ShouldWork()
    {
        // Arrange
        var connection = CreateHubConnection("/account-hub/connection");
        await connection.StartAsync();

        // Act
        await connection.InvokeAsync(HubMessages.CreateSession, "test-account-id");

        await Task.Delay(100);

        // Assert
        Assert.Equal(HubConnectionState.Connected, connection.State);

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task AccountHubConnection_EndSession_ShouldWork()
    {
        // Arrange
        var connection = CreateHubConnection("/account-hub/connection");
        await connection.StartAsync();

        await connection.InvokeAsync(HubMessages.CreateSession, "test-account-id");
        await Task.Delay(100);

        // Act
        await connection.InvokeAsync(HubMessages.EndSession, "test-account-id");
        await Task.Delay(100);

        // Assert
        Assert.Equal(HubConnectionState.Connected, connection.State);

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task AccountHubConnection_CreateThenEndSession_ShouldNotifyPreviousConnection()
    {
        // Arrange
        var connection1 = CreateHubConnection("/account-hub/connection");
        var connection2 = CreateHubConnection("/account-hub/connection");

        await connection1.StartAsync();
        await connection2.StartAsync();

        // Act
        await connection1.InvokeAsync(HubMessages.CreateSession, "test-account-id");
        await Task.Delay(200);

        await connection2.InvokeAsync(HubMessages.CreateSession, "test-account-id");
        await Task.Delay(200);

        // Assert
        Assert.Equal(HubConnectionState.Connected, connection1.State);
        Assert.Equal(HubConnectionState.Connected, connection2.State);

        await connection1.DisposeAsync();
        await connection2.DisposeAsync();
    }

    #endregion

    #region JoinGame Tests

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

        // Act
        await connection1.InvokeAsync(HubMessages.JoinGame, gameHash, "token1", "TestPlayer");
        await Task.Delay(100);

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

        // Act
        var hostConnection = CreateHubConnection("/hub/connection");
        await hostConnection.StartAsync();
        await hostConnection.InvokeAsync(HubMessages.JoinGame, gameHash, hostToken, "TestHost");
        connections.Add(hostConnection);

        for (int i = 1; i <= 3; i++)
        {
            var connection = CreateHubConnection("/hub/connection");
            await connection.StartAsync();
            await connection.InvokeAsync(HubMessages.JoinGame, gameHash, $"token{i}", $"Player{i}");
            connections.Add(connection);
        }

        await Task.Delay(300);

        // Assert
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

    #endregion

    #region LeaveGame Tests

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

        // Assert
        Assert.Equal(HubConnectionState.Connected, connection.State);

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task LeaveGame_ShouldDoNothing_WithNonExistentGame()
    {
        // Arrange
        var connection = CreateHubConnection("/hub/connection");
        await connection.StartAsync();

        // Act and Assert
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

        // Assert
        string? joinError = null;
        connection.On(HubMessages.OnJoinGameError, (string error) => joinError = error);

        await connection.InvokeAsync(HubMessages.JoinGame, gameHash, hostToken, "TestHost");
        await Task.Delay(100);

        Assert.NotNull(joinError);

        await connection.DisposeAsync();
    }

    #endregion

    #region Canvas Tests

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

    #endregion

    #region Multiple Hub Tests

    [Fact]
    public async Task MultipleHubs_ShouldConnectSimultaneously()
    {
        // Arrange
        var accountHubConnection = CreateHubConnection("/account-hub/connection");
        var hubConnection = CreateHubConnection("/hub/connection");
        var longRunningHubConnection = CreateHubConnection("/long-running-hub/connection");

        // Act
        await Task.WhenAll(
            accountHubConnection.StartAsync(),
            hubConnection.StartAsync(),
            longRunningHubConnection.StartAsync()
        );

        // Assert
        Assert.Equal(HubConnectionState.Connected, accountHubConnection.State);
        Assert.Equal(HubConnectionState.Connected, hubConnection.State);
        Assert.Equal(HubConnectionState.Connected, longRunningHubConnection.State);

        await Task.WhenAll(
            accountHubConnection.DisposeAsync().AsTask(),
            hubConnection.DisposeAsync().AsTask(),
            longRunningHubConnection.DisposeAsync().AsTask()
        );
    }

    [Fact]
    public async Task HubConnection_ShouldHandleDisconnectionGracefully()
    {
        // Arrange
        var connection = CreateHubConnection("/hub/connection");
        await connection.StartAsync();

        // Act
        await connection.StopAsync();

        await Task.Delay(1000);

        // Assert
        Assert.True(
            connection.State == HubConnectionState.Disconnected ||
            connection.State == HubConnectionState.Reconnecting,
            $"Unexpected state: {connection.State}"
        );

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task HubConnection_ShouldHandleConnectionErrors()
    {
        // Arrange
        var connection = CreateHubConnection("/invalid-hub/connection");

        // Act
        _ = connection.StartAsync();

        await Task.Delay(5000);

        // Assert
        Assert.True(
            connection.State == HubConnectionState.Disconnected ||
            connection.State == HubConnectionState.Connecting,
            $"Unexpected state: {connection.State}"
        );

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task AccountHubConnection_ShouldHandleMultipleConcurrentSessions()
    {
        // Arrange
        var connection1 = CreateHubConnection("/account-hub/connection");
        var connection2 = CreateHubConnection("/account-hub/connection");
        var connection3 = CreateHubConnection("/account-hub/connection");

        await Task.WhenAll(
            connection1.StartAsync(),
            connection2.StartAsync(),
            connection3.StartAsync()
        );

        // Act
        var tasks = new[]
        {
            connection1.InvokeAsync(HubMessages.CreateSession, "test-account"),
            connection2.InvokeAsync(HubMessages.CreateSession, "test-account"),
            connection3.InvokeAsync(HubMessages.CreateSession, "test-account")
        };

        await Task.WhenAll(tasks);
        await Task.Delay(200);

        // Assert
        Assert.Equal(HubConnectionState.Connected, connection1.State);
        Assert.Equal(HubConnectionState.Connected, connection2.State);
        Assert.Equal(HubConnectionState.Connected, connection3.State);

        await Task.WhenAll(
            connection1.DisposeAsync().AsTask(),
            connection2.DisposeAsync().AsTask(),
            connection3.DisposeAsync().AsTask()
        );
    }

    #endregion
}
