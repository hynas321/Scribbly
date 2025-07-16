using Dapper;
using Microsoft.Data.Sqlite;
using WebApi.Domain.Entities;
using WebApi.Infrastructure.Repositories.Interfaces;

namespace WebApi.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly string _connectionString;

        public AccountRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DatabaseConnectionString");

            using SqliteConnection db = new(_connectionString);
            db.Open();
            db.Execute(@"
                CREATE TABLE IF NOT EXISTS Account (
                    Id TEXT PRIMARY KEY,
                    AccessToken TEXT,
                    Email TEXT,
                    Name TEXT,
                    GivenName TEXT,
                    FamilyName TEXT,
                    Score INTEGER
                )");
        }

        public async Task<bool> AddAccountAsync(Account account, CancellationToken cancellationToken)
        {
            await using SqliteConnection db = new(_connectionString);
            await db.OpenAsync(cancellationToken);

            CommandDefinition insertCommand = new(
                commandText: @"
                    INSERT OR IGNORE INTO Account 
                        (Id, AccessToken, Email, Name, GivenName, FamilyName, Score)
                    VALUES 
                        (@Id, @AccessToken, @Email, @Name, @GivenName, @FamilyName, 0)",
                parameters: account,
                cancellationToken: cancellationToken
            );

            int rows = await db.ExecuteAsync(insertCommand);
            if (rows == 0)
            {
                CommandDefinition updateCommand = new(
                    commandText: @"
                        UPDATE Account 
                        SET AccessToken = @AccessToken 
                        WHERE Id = @Id",
                    parameters: new { account.Id, account.AccessToken },
                    cancellationToken: cancellationToken
                );
                await db.ExecuteAsync(updateCommand);
                return false;
            }

            return true;
        }

        public async Task IncrementAccountScoreAsync(string accessToken, int number, CancellationToken cancellationToken)
        {
            await using SqliteConnection db = new(_connectionString);
            await db.OpenAsync(cancellationToken);

            CommandDefinition command = new(
                commandText: "UPDATE Account SET Score = Score + @Number WHERE AccessToken = @AccessToken",
                parameters: new { AccessToken = accessToken, Number = number },
                cancellationToken: cancellationToken
            );

            await db.ExecuteAsync(command);
        }

        public async Task<int> GetAccountScoreAsync(string accessToken, CancellationToken cancellationToken)
        {
            await using SqliteConnection db = new(_connectionString);
            await db.OpenAsync(cancellationToken);

            CommandDefinition query = new(
                commandText: "SELECT Score FROM Account WHERE AccessToken = @AccessToken",
                parameters: new { AccessToken = accessToken },
                cancellationToken: cancellationToken
            );

            return await db.QueryFirstOrDefaultAsync<int>(query);
        }

        public async Task<Account> GetAccountAsync(string id, CancellationToken cancellationToken)
        {
            await using SqliteConnection db = new(_connectionString);
            await db.OpenAsync(cancellationToken);

            CommandDefinition query = new(
                commandText: "SELECT * FROM Account WHERE Id = @Id",
                parameters: new { Id = id },
                cancellationToken: cancellationToken
            );

            return await db.QueryFirstOrDefaultAsync<Account>(query);
        }

        public async Task<IEnumerable<MainScoreboardScore>> GetTopAccountPlayerScoresAsync(CancellationToken cancellationToken)
        {
            await using SqliteConnection db = new(_connectionString);
            await db.OpenAsync(cancellationToken);

            CommandDefinition query = new CommandDefinition(
                commandText: "SELECT GivenName, Email, Score FROM Account ORDER BY Score DESC LIMIT 5",
                cancellationToken: cancellationToken
            );

            return await db.QueryAsync<MainScoreboardScore>(query);
        }
    }
}
