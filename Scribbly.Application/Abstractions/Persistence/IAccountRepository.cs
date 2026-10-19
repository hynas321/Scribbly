using Scribbly.Application.Features.Accounts;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.Abstractions.Persistence;

public interface IAccountRepository
{
    Task<bool> AddAccountAsync(Account account, CancellationToken cancellationToken);
    Task<AccountScoreUpdateResult> IncrementAccountScoreOnceAsync(
        string gameHash,
        string credentialHash,
        string accountId,
        int number,
        CancellationToken cancellationToken);
    Task<Account?> GetAccountAsync(string id, CancellationToken cancellationToken);
    Task<IEnumerable<MainScoreboardScore>> GetTopAccountPlayerScoresAsync(CancellationToken cancellationToken);
}
