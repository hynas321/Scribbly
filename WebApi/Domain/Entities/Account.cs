namespace WebApi.Domain.Entities;

public class Account
{
    public string Id { get; set; }
    public string AccessToken { get; set; }
    public string Name { get; set; }
    public int Score { get; set; }
}