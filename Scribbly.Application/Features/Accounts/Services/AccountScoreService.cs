using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Abstractions.Security;

namespace Scribbly.Application.Features.Accounts;

public class AccountScoreService : IAccountScoreService
{
    private readonly IGameStateStore _games;
    private readonly IGameResultRepository _results;
    private readonly IGameResultService _capture;
    private readonly IAccountRepository _accounts;
    private readonly IRoomCredentialService _credentials;

    public AccountScoreService(
        IGameStateStore games,
        IAccountRepository accounts,
        IGameResultRepository results,
        IGameResultService capture,
        IRoomCredentialService credentials)
    {
        _games = games;
        _accounts = accounts;
        _results = results;
        _capture = capture;
        _credentials = credentials;
    }

    public async Task<AccountScoreUpdateResult> SubmitAsync(
        string gameHash,
        string roomAccessToken,
        string accountId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(roomAccessToken))
        {
            return new(AccountScoreUpdateStatus.InvalidCredential, 0);
        }

        string hash = _credentials.ComputeCredentialHash(roomAccessToken);
        int? score = await _results.GetScoreAsync(gameHash, hash, cancellationToken);

        if (!score.HasValue)
        {
            var game = await _games.GetSnapshotAsync(gameHash, cancellationToken);

            if (game is null)
            {
                return new(AccountScoreUpdateStatus.PlayerNotFound, 0);
            }

            if (game.GameState.CurrentRound <= game.GameSettings.RoundsCount)
            {
                return new(AccountScoreUpdateStatus.GameNotFinished, 0);
            }

            await _capture.CaptureAsync(gameHash, cancellationToken);
            score = await _results.GetScoreAsync(gameHash, hash, cancellationToken);

            if (!score.HasValue)
            {
                return new(AccountScoreUpdateStatus.PlayerNotFound, 0);
            }
        }

        return await _accounts.IncrementAccountScoreOnceAsync(gameHash, hash, accountId, score.Value, cancellationToken);
    }
}
