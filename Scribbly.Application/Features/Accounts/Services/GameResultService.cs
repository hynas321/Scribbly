using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Persistence;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Accounts;

public class GameResultService : IGameResultService
{
    private readonly IGameStateStore _games;
    private readonly IGameResultRepository _results;

    public GameResultService(IGameStateStore games, IGameResultRepository results)
    {
        _games = games;
        _results = results;
    }

    public async Task<bool> CaptureAsync(string gameHash, CancellationToken cancellationToken)
    {
        var game = await _games.GetSnapshotAsync(gameHash, cancellationToken);

        if (game is null || game.GameState.CurrentRound <= game.GameSettings.RoundsCount)
        {
            return false;
        }
        if (game.ResultsCaptured)
        {
            return true;
        }

        var scores = game.FinalScores.Count > 0 ? game.FinalScores.ToArray() : game.GameState.Players
            .Concat(game.GameState.DisconnectedPlayers)
            .Select(player => new FinalPlayerScore(player.CredentialHash, player.Score, player.Username)).ToArray();

        await _results.SaveAsync(gameHash, scores, cancellationToken);
        await _games.MutateAsync(gameHash, current =>
        {
            if (current.HostCredentialHash != game.HostCredentialHash || current.GameState.CurrentRound <= current.GameSettings.RoundsCount)
            {
                return GameMutation<bool>.NoChange(false);
            }

            if (current.ResultsCaptured)
            {
                return GameMutation<bool>.NoChange(true);
            }

            current.ResultsCaptured = true;
            return GameMutation<bool>.Persist(true);
        }, cancellationToken);

        return true;
    }
}
