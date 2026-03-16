using Microsoft.AspNetCore.SignalR;

namespace WebApi.Hubs;

public partial class HubConnection : Hub
{
    protected virtual async Task SendToGroupExcept(string groupName, string excludedConnectionId, string method, object arg)
    {
        await Clients.GroupExcept(groupName, excludedConnectionId).SendAsync(method, arg);
    }
}
