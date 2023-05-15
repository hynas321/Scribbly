namespace Dotnet.Server.Hubs;

public class AccountHubState
{
    public static Dictionary<string, string> AccountConnections { get; set; } = new Dictionary<string, string>();
}