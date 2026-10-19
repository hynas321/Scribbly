using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Abstractions.Sessions;

namespace Scribbly.Application.Features.Accounts;

public class AccountSessionService
{
    private readonly IAccountSessionStore _sessions;
    private readonly IAccountRepository _accounts;
    private readonly AccountSessionOptions _options;

    public AccountSessionService(
        IAccountSessionStore sessions,
        IAccountRepository accounts,
        AccountSessionOptions options)
    {
        _sessions = sessions;
        _accounts = accounts;
        _options = options;
    }

    public Task<string> CreateAsync(string accountId, CancellationToken cancellationToken) =>
        _sessions.CreateAsync(accountId, _options.Lifetime, cancellationToken);

    public Task DeleteAsync(string? token, CancellationToken cancellationToken) =>
        _sessions.DeleteAsync(token, cancellationToken);
    public Task<string?> GetAccountIdAsync(string? token, CancellationToken cancellationToken) =>
        _sessions.GetAccountIdAsync(token, cancellationToken);

    public async Task<AccountProfile?> GetProfileAsync(string? token, CancellationToken cancellationToken)
    {
        string? accountId = await _sessions.GetAccountIdAsync(token, cancellationToken);

        if (accountId is null)
        {
            return null;
        }

        var account = await _accounts.GetAccountAsync(accountId, cancellationToken);

        if (account is null)
        {
            await _sessions.DeleteAsync(token, cancellationToken);
            return null;
        }

        return new(account.Id, account.Name, account.Score);
    }
}
