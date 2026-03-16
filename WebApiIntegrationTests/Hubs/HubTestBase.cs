using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace WebApiIntegrationTests.Hubs;

[Collection("SignalR Integration Tests")]
public abstract class HubTestBase : IAsyncLifetime
{
    protected readonly WebApplicationFactory<Program> WebApplicationFactory;
    protected readonly ITestOutputHelper Output;

    protected HubTestBase(ITestOutputHelper output)
    {
        WebApplicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services => {});
                builder.UseEnvironment("Testing");
            });
        Output = output;
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await WebApplicationFactory.DisposeAsync();
    }

    protected HubConnection CreateHubConnection(string hubPath)
    {
        var serverAddress = WebApplicationFactory.Server.BaseAddress;

        var hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri(serverAddress, hubPath), options =>
            {
                options.HttpMessageHandlerFactory = _ => WebApplicationFactory.Server.CreateHandler();
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
}
