using WebApi.Api.Hubs.Static;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.Hubs;

public partial class AccountHubConnection : Hub
{
    private readonly ILogger<AccountHubConnection> _logger;

    public AccountHubConnection(ILogger<AccountHubConnection> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    [HubMethodName(HubMessages.CreateSession)]
    public async Task CreateSession(string accountId)
    {
        if (AccountHubState.AccountConnections.TryGetValue(accountId, out string value))
        {
            string connectionId = value;
            AccountHubState.AccountConnections.Remove(accountId);
            await Clients.Client(connectionId).SendAsync(HubMessages.OnSessionEnded);

            _logger.LogInformation($"Session {connectionId} ended for the account ID {accountId}");
        }

        _logger.LogInformation($"New Session {Context.ConnectionId} for the account ID {accountId}");

        AccountHubState.AccountConnections.Add(accountId, Context.ConnectionId);
    }

    [HubMethodName(HubMessages.EndSession)]
    public void EndSession(string accountId)
    {
        if (AccountHubState.AccountConnections.TryGetValue(accountId, out string value))
        {
            string connectionId = value;

            if (connectionId == Context.ConnectionId)
            {
                AccountHubState.AccountConnections.Remove(accountId);

                _logger.LogInformation($"Session {connectionId} ended for the account ID {accountId}");
            }
        }
    }

}