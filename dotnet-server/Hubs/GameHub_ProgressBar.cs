using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class GameHub : Hub
{
    [HubMethodName("StartProgressBarTimer")]
    public void StartProgressBarTimer(string hash)
    {

    }

    [HubMethodName("StopProgressBarTimer")]
    public void StopProgressBarTimer(string hash)
    {

    }
}