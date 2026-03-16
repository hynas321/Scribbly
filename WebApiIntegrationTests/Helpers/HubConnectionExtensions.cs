using Microsoft.AspNetCore.SignalR.Client;

namespace WebApiIntegrationTests.Helpers;

public static class HubConnectionExtensions
{
    public static async Task<HubConnection> CreateHubConnectionAsync(
        this HttpClient httpClient,
        string hubPath,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = httpClient.BaseAddress?.ToString() ?? "http://localhost";
        var hubUrl = $"{baseUrl.TrimEnd('/')}/{hubPath}";

        var handler = new SocketsHttpHandler();

        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.SkipNegotiation = false;
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                options.HttpMessageHandlerFactory = _ => handler;

                if (httpClient.DefaultRequestHeaders.Contains("adminToken"))
                {
                    var token = httpClient.DefaultRequestHeaders.GetValues("adminToken").FirstOrDefault();
                    if (token != null)
                    {
                        options.Headers.Add("adminToken", token);
                    }
                }
            })
            .WithAutomaticReconnect()
            .Build();

        await connection.StartAsync(cancellationToken);

        return connection;
    }

    public static HubConnectionBuilder WithTestAuth(this HubConnectionBuilder builder, string token)
    {
        return builder;
    }

    public static async Task WaitForConnectionAsync(this HubConnection connection, TimeSpan timeout)
    {
        var cts = new CancellationTokenSource(timeout);

        while (!cts.IsCancellationRequested)
        {
            if (connection.State == HubConnectionState.Connected)
            {
                return;
            }

            await Task.Delay(100, cts.Token);
        }

        throw new TimeoutException($"Hub connection did not reach Connected state within {timeout}");
    }

    public static async Task WaitForDisconnectionAsync(this HubConnection connection, TimeSpan timeout)
    {
        var cts = new CancellationTokenSource(timeout);

        while (!cts.IsCancellationRequested)
        {
            if (connection.State == HubConnectionState.Disconnected)
            {
                return;
            }

            await Task.Delay(100, cts.Token);
        }

        throw new TimeoutException($"Hub connection did not reach Disconnected state within {timeout}");
    }
}
