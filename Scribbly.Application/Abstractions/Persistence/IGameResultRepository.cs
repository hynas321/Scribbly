using Scribbly.Domain.Entities;

namespace Scribbly.Application.Abstractions.Persistence;

public interface IGameResultRepository
{
    Task SaveAsync(string gameHash, IReadOnlyCollection<FinalPlayerScore> scores, CancellationToken cancellationToken);
    Task<int?> GetScoreAsync(string gameHash, string credentialHash, CancellationToken cancellationToken);
    Task<IReadOnlyList<FinalPlayerScore>> GetScoresAsync(string gameHash, CancellationToken cancellationToken);
}
