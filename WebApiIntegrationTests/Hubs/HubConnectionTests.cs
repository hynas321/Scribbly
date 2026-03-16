using Microsoft.AspNetCore.SignalR.Client;
using Xunit.Abstractions;

namespace WebApiIntegrationTests.Hubs;

public class HubConnectionTests : HubTestBase
{
    public HubConnectionTests(ITestOutputHelper output) : base(output) {}

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

        // Act and Assert
        await connection.StartAsync();
        Assert.Equal(HubConnectionState.Connected, connection.State);

        await connection.DisposeAsync();
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
}
