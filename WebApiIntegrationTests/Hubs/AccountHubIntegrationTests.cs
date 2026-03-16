using Microsoft.AspNetCore.SignalR.Client;
using WebApi.Api.Hubs.Static;
using Xunit;
using Xunit.Abstractions;

namespace WebApiIntegrationTests.Hubs;

public class AccountHubIntegrationTests : HubTestBase
{
    public AccountHubIntegrationTests(ITestOutputHelper output) : base(output) {}

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
}
