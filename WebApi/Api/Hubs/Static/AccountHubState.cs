namespace WebApi.Api.Hubs.Static;

public class AccountHubState
{
    public static Dictionary<string, string> AccountConnections { get; set; } = new Dictionary<string, string>();
}