namespace Dotnet.Server.Models;

public class Account
{
    public string Id { get; set; }
    public string AccessToken { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
    public int Score { get; set; }
}