using Scribbly.Application.Abstractions.Identity;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Accounts;

public class AccountSignInService
{
    private readonly IGoogleIdentityVerifier _identity;
    private readonly IAccountRepository _accounts;

    public AccountSignInService(IGoogleIdentityVerifier identity, IAccountRepository accounts)
    {
        _identity = identity;
        _accounts = accounts;
    }

    public async Task<AccountSignInResult?> SignInAsync(string idToken, CancellationToken cancellationToken)
    {
        var identity = await _identity.VerifyAsync(idToken, cancellationToken);

        if (identity is null)
        {
            return null;
        }

        bool created = await _accounts.AddAccountAsync(new Account { Id = identity.Id, Name = identity.Name, Email = identity.Email, Score = 0 }, cancellationToken);

        Account account = await _accounts.GetAccountAsync(identity.Id, cancellationToken)
            ?? throw new InvalidOperationException("The signed-in account could not be loaded.");
        return new(created, new(account.Id, account.Name, account.Score));
    }
}
