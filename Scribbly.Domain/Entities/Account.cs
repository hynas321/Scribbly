namespace Scribbly.Domain.Entities;

public class Account
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Email { get; set; } = string.Empty;
}
