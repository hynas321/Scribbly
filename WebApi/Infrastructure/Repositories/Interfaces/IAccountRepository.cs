using WebApi.Domain.Entities;

namespace WebApi.Infrastructure.Repositories.Interfaces;

public interface IAccountRepository
{
    public Task<bool> AddAccountAsync(Account account, CancellationToken cancellationToken);
    public Task IncrementAccountScoreAsync(string accessToken, int number, CancellationToken cancellationToken);
    public Task<int> GetAccountScoreAsync(string accessToken, CancellationToken cancellationToken);
    public Task<Account> GetAccountAsync(string id, CancellationToken cancellationToken);
    public Task<IEnumerable<MainScoreboardScore>> GetTopAccountPlayerScoresAsync(CancellationToken cancellationToken);
}