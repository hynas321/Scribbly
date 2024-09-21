using dotnet_server.Api.Hubs.Static;
using Microsoft.AspNetCore.SignalR;

namespace dotnet_server.Api.Hubs;

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
        try
        {
            if (AccountHubState.AccountConnections.ContainsKey(accountId))
            {
                string connectionId = AccountHubState.AccountConnections[accountId];
                AccountHubState.AccountConnections.Remove(accountId);
                await Clients.Client(connectionId).SendAsync(HubMessages.OnSessionEnded);

                _logger.LogInformation($"Session {connectionId} ended for the account ID {accountId}");
            }

            _logger.LogInformation($"New Session {Context.ConnectionId} for the account ID {accountId}");

            AccountHubState.AccountConnections.Add(accountId, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubMessages.EndSession)]
    public void EndSession(string accountId)
    {
        try
        {
            if (AccountHubState.AccountConnections.ContainsKey(accountId))
            {
                string connectionId = AccountHubState.AccountConnections[accountId];

                if (connectionId == Context.ConnectionId)
                {
                    AccountHubState.AccountConnections.Remove(accountId);

                    _logger.LogInformation($"Session {connectionId} ended for the account ID {accountId}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }

}