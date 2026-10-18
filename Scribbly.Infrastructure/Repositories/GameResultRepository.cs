using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Domain.Entities;
using Scribbly.Infrastructure.Configuration;

namespace Scribbly.Infrastructure.Repositories;

public class GameResultRepository : IGameResultRepository
{
    private readonly string _connectionString;
    public GameResultRepository(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.DatabaseConnectionString;

        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException(
                $"{DatabaseOptions.SectionName}:{DatabaseOptions.DatabaseConnectionStringName} is required.");
        }
    }

    public async Task SaveAsync(
        string gameHash,
        IReadOnlyCollection<FinalPlayerScore> scores,
        CancellationToken cancellationToken)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await db.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        foreach (FinalPlayerScore score in scores.OrderBy(s => s.CredentialHash, StringComparer.Ordinal))
        {
            var parameters = new
            {
                GameHash = gameHash,
                CredentialHash = CredentialHashSql.AsParameter(score.CredentialHash),
                score.Score,
                score.Username
            };
            int? existing = await db.QuerySingleOrDefaultAsync<int?>(new CommandDefinition(@"
                SELECT Score FROM GameResult WITH (UPDLOCK, HOLDLOCK)
                WHERE GameHash = @GameHash AND CredentialHash = @CredentialHash",
                parameters, transaction, cancellationToken: cancellationToken));
            if (existing.HasValue)
            {
                if (existing.Value != score.Score)
                {
                    throw new InvalidOperationException("A completed player's result cannot be changed.");
                }
                continue;
            }
            await db.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO GameResult (GameHash, CredentialHash, Score, Username)
                VALUES (@GameHash, @CredentialHash, @Score, @Username)", parameters, transaction, cancellationToken: cancellationToken));
        }
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FinalPlayerScore>> GetScoresAsync(
        string gameHash,
        CancellationToken cancellationToken)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(cancellationToken);
        var scores = await db.QueryAsync<FinalPlayerScore>(new CommandDefinition(@"
            SELECT CredentialHash, Score, Username FROM GameResult
            WHERE GameHash = @GameHash ORDER BY Score DESC, Username, CredentialHash",
            new
            {
                GameHash = gameHash
            }, cancellationToken: cancellationToken));
        return scores.ToArray();
    }

    public async Task<int?> GetScoreAsync(
        string gameHash,
        string credentialHash,
        CancellationToken cancellationToken)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(cancellationToken);
        return await db.QuerySingleOrDefaultAsync<int?>(new CommandDefinition(@"
            SELECT Score FROM GameResult WHERE GameHash = @GameHash AND CredentialHash = @CredentialHash",
            new
            {
                GameHash = gameHash,
                CredentialHash = CredentialHashSql.AsParameter(credentialHash)
            }, cancellationToken: cancellationToken));
    }
}
