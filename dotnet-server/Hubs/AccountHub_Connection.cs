using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class AccountHubConnection : Hub
{
    private readonly ILogger<AccountHubConnection> logger;

    public AccountHubConnection(ILogger<AccountHubConnection> logger)
    {
        this.logger = logger;
    }

    public override async Task OnConnectedAsync()
    {   
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {   
        await base.OnDisconnectedAsync(exception);
    }

    [HubMethodName(HubEvents.CreateSession)]
    public async Task CreateSession(string accountId)
    {
        try
        {   
            if (AccountHubState.AccountConnections.ContainsKey(accountId))
            {
                string connectionId = AccountHubState.AccountConnections[accountId];
                AccountHubState.AccountConnections.Remove(accountId);
                await Clients.Client(connectionId).SendAsync(HubEvents.OnSessionEnded);

                logger.LogInformation($"Session {connectionId} ended for the account ID {accountId}");
            }
            
            logger.LogInformation($"New Session {Context.ConnectionId} for the account ID {accountId}");

            AccountHubState.AccountConnections.Add(accountId, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.EndSession)]
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

                    logger.LogInformation($"Session {connectionId} ended for the account ID {accountId}");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(Convert.ToString(ex));
        }
    }

}