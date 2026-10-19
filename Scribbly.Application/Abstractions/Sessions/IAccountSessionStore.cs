namespace Scribbly.Application.Abstractions.Sessions;

public interface IAccountSessionStore
{
    Task<string> CreateAsync(string accountId, TimeSpan lifetime, CancellationToken cancellationToken);
    Task<string?> GetAccountIdAsync(string? sessionToken, CancellationToken cancellationToken);
    Task DeleteAsync(string? sessionToken, CancellationToken cancellationToken);
}
