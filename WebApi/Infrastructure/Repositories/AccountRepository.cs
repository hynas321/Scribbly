using Dapper;
using System.Data.SqlClient;
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
        }

        public async Task<bool> AddAccountAsync(Account account, CancellationToken cancellationToken)
        {
            await using SqlConnection db = new SqlConnection(_connectionString);
            await db.OpenAsync(cancellationToken);

            var exists = await db.QueryFirstOrDefaultAsync<int>(
                new CommandDefinition(
                    "SELECT COUNT(1) FROM Account WHERE Id = @Id",
                    new { account.Id },
                    cancellationToken: cancellationToken
                )
            );

            if (exists == 0)
            {
                CommandDefinition insertCommand = new CommandDefinition(
                    commandText: @"
                        INSERT INTO Account 
                            (Id, AccessToken, Email, Name, GivenName, FamilyName, Score)
                        VALUES 
                            (@Id, @AccessToken, @Email, @Name, @GivenName, @FamilyName, 0)",
                    parameters: account,
                    cancellationToken: cancellationToken
                );
                await db.ExecuteAsync(insertCommand);
                return true;
            }
            else
            {
                CommandDefinition updateCommand = new CommandDefinition(
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
        }

        public async Task IncrementAccountScoreAsync(string accessToken, int number, CancellationToken cancellationToken)
        {
            await using SqlConnection db = new SqlConnection(_connectionString);
            await db.OpenAsync(cancellationToken);

            CommandDefinition command = new CommandDefinition(
                commandText: "UPDATE Account SET Score = Score + @Number WHERE AccessToken = @AccessToken",
                parameters: new { AccessToken = accessToken, Number = number },
                cancellationToken: cancellationToken
            );

            await db.ExecuteAsync(command);
        }

        public async Task<int> GetAccountScoreAsync(string accessToken, CancellationToken cancellationToken)
        {
            await using SqlConnection db = new SqlConnection(_connectionString);
            await db.OpenAsync(cancellationToken);

            CommandDefinition query = new CommandDefinition(
                commandText: "SELECT Score FROM Account WHERE AccessToken = @AccessToken",
                parameters: new { AccessToken = accessToken },
                cancellationToken: cancellationToken
            );

            return await db.QueryFirstOrDefaultAsync<int>(query);
        }

        public async Task<Account> GetAccountAsync(string id, CancellationToken cancellationToken)
        {
            await using SqlConnection db = new SqlConnection(_connectionString);
            await db.OpenAsync(cancellationToken);

            CommandDefinition query = new CommandDefinition(
                commandText: "SELECT * FROM Account WHERE Id = @Id",
                parameters: new { Id = id },
                cancellationToken: cancellationToken
            );

            return await db.QueryFirstOrDefaultAsync<Account>(query);
        }

        public async Task<IEnumerable<MainScoreboardScore>> GetTopAccountPlayerScoresAsync(CancellationToken cancellationToken)
        {
            await using SqlConnection db = new SqlConnection(_connectionString);
            await db.OpenAsync(cancellationToken);

            CommandDefinition query = new CommandDefinition(
                commandText: "SELECT GivenName, Email, Score FROM Account ORDER BY Score DESC OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY",
                cancellationToken: cancellationToken
            );

            return await db.QueryAsync<MainScoreboardScore>(query);
        }
    }
}
