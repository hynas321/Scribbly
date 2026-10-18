using Scribbly.Application.Features.Accounts;

namespace Scribbly.Application.Abstractions.Persistence;

public interface IAccountScoreService
{
    Task<AccountScoreUpdateResult> SubmitAsync(
        string gameHash,
        string roomAccessToken,
        string accountId,
        CancellationToken cancellationToken);
}
