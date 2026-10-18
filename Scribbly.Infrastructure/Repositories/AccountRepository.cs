using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Features.Accounts;
using Scribbly.Domain.Entities;
using Scribbly.Infrastructure.Configuration;

namespace Scribbly.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly string _connectionString;
    public AccountRepository(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.DatabaseConnectionString;

        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException(
                $"{DatabaseOptions.SectionName}:{DatabaseOptions.DatabaseConnectionStringName} is required.");
        }
    }

    public async Task<bool> AddAccountAsync(Account account, CancellationToken cancellationToken)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await db.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        int exists = await db.QuerySingleAsync<int>(new CommandDefinition(
            "SELECT COUNT(1) FROM Account WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id",
            new
            {
                account.Id
            }, transaction, cancellationToken: cancellationToken));
        if (exists == 0)
        {
            await db.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO Account (Id, Email, Name, Score)
                VALUES (@Id, @Email, @Name, @Score)", account, transaction, cancellationToken: cancellationToken));
        }
        else
        {
            await db.ExecuteAsync(new CommandDefinition(@"
                UPDATE Account SET Name = @Name, Email = @Email WHERE Id = @Id",
                account, transaction, cancellationToken: cancellationToken));
        }
        await transaction.CommitAsync(cancellationToken);
        return exists == 0;
    }

    public async Task<AccountScoreUpdateResult> IncrementAccountScoreOnceAsync(
        string gameHash, string credentialHash, string accountId, int number, CancellationToken cancellationToken)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await db.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var key = new
        {
            GameHash = gameHash,
            CredentialHash = CredentialHashSql.AsParameter(credentialHash)
        };
        string? existingAccountId = await db.QueryFirstOrDefaultAsync<string>(new CommandDefinition(@"
            SELECT AccountId FROM AccountScoreAward WITH (UPDLOCK, HOLDLOCK)
            WHERE GameHash = @GameHash AND CredentialHash = @CredentialHash", key, transaction, cancellationToken: cancellationToken));

        string? existingCredentialForAccount = await db.QueryFirstOrDefaultAsync<string>(new CommandDefinition(@"
            SELECT CredentialHash FROM AccountScoreAward WITH (UPDLOCK, HOLDLOCK)
            WHERE GameHash = @GameHash AND AccountId = @AccountId",
            new
            {
                GameHash = gameHash,
                AccountId = accountId
            }, transaction, cancellationToken: cancellationToken));
        if (existingCredentialForAccount is not null &&
            !string.Equals(existingCredentialForAccount, credentialHash, StringComparison.Ordinal))
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(AccountScoreUpdateStatus.ConflictingSubmission, 0);
        }

        int accountExists = await db.QuerySingleAsync<int>(new CommandDefinition(
            "SELECT COUNT(1) FROM Account WITH (UPDLOCK) WHERE Id = @AccountId",
            new
            {
                AccountId = accountId
            }, transaction, cancellationToken: cancellationToken));
        if (accountExists == 0)
        {
            return new(AccountScoreUpdateStatus.AccountNotFound, 0);
        }

        if (existingAccountId is not null)
        {
            if (!string.Equals(existingAccountId, accountId, StringComparison.Ordinal))
            {
                return new(AccountScoreUpdateStatus.ConflictingSubmission, 0);
            }
            int existingScore = await db.QuerySingleAsync<int>(new CommandDefinition(
                "SELECT Score FROM Account WHERE Id = @AccountId", new
                {
                    AccountId = accountId
                }, transaction, cancellationToken: cancellationToken));
            await transaction.CommitAsync(cancellationToken);
            return new(AccountScoreUpdateStatus.AlreadyApplied, existingScore);
        }

        await db.ExecuteAsync(new CommandDefinition(
            "UPDATE Account SET Score = Score + @Number WHERE Id = @AccountId",
            new
            {
                AccountId = accountId,
                Number = number
            }, transaction, cancellationToken: cancellationToken));
        await db.ExecuteAsync(new CommandDefinition(@"
            INSERT INTO AccountScoreAward (GameHash, CredentialHash, AccountId, Score)
            VALUES (@GameHash, @CredentialHash, @AccountId, @Score)",
            new
            {
                GameHash = gameHash,
                CredentialHash = CredentialHashSql.AsParameter(credentialHash),
                AccountId = accountId,
                Score = number
            },
            transaction, cancellationToken: cancellationToken));
        int score = await db.QuerySingleAsync<int>(new CommandDefinition(
            "SELECT Score FROM Account WHERE Id = @AccountId", new
            {
                AccountId = accountId
            }, transaction, cancellationToken: cancellationToken));
        await transaction.CommitAsync(cancellationToken);
        return new(AccountScoreUpdateStatus.Applied, score);
    }

    public async Task<Account?> GetAccountAsync(string id, CancellationToken cancellationToken)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(cancellationToken);
        return await db.QueryFirstOrDefaultAsync<Account>(new CommandDefinition(
            "SELECT Id, Name, Email, Score FROM Account WHERE Id = @Id", new
            {
                Id = id
            }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<MainScoreboardScore>> GetTopAccountPlayerScoresAsync(CancellationToken cancellationToken)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(cancellationToken);
        return await db.QueryAsync<MainScoreboardScore>(new CommandDefinition(
            "SELECT Name, Score FROM Account ORDER BY Score DESC OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY",
            cancellationToken: cancellationToken));
    }
}
