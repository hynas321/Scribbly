using Dapper;
using WebApi.Api.Models.HttpRequest;
using WebApi.Infrastructure.Repositories.Interfaces;
using Microsoft.Data.Sqlite;

namespace WebApi.Infrastructure.Repositories;

public class WordRepository : IWordRepository
{
    private readonly string _connectionString;
    private readonly string _databasePassword;

    public WordRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DatabaseConnectionString");
        _databasePassword = configuration["DatabasePassword"];

        using (SqliteConnection db = new SqliteConnection(_connectionString))
        {
            db.Open();
            db.Execute($"PRAGMA key = '{_databasePassword}';");
            db.Execute(@"CREATE TABLE IF NOT EXISTS Word (
                            Text TEXT,
                            Language TEXT
                          );");
        }
    }

    public async Task<bool> AddWordAsync(string text, string language, CancellationToken cancellationToken)
    {
        using (SqliteConnection db = new SqliteConnection(_connectionString))
        {
            await db.OpenAsync(cancellationToken);

            const string insertQuery = @"
                INSERT INTO Word (Text, Language)
                VALUES (@Text, @Language);
            ";

            object parameters = new { Text = text, Language = language };
            CommandDefinition command = new CommandDefinition(
                commandText: insertQuery,
                parameters: parameters,
                cancellationToken: cancellationToken
            );

            int rows = await db.ExecuteAsync(command);
            return rows > 0;
        }
    }

    public async Task<bool> DeleteWordAsync(string text, string language, CancellationToken cancellationToken)
    {
        using (SqliteConnection db = new SqliteConnection(_connectionString))
        {
            await db.OpenAsync(cancellationToken);

            const string deleteQuery = @"
                DELETE FROM Word
                WHERE Text = @Text
                  AND Language = @Language;
            ";

            object parameters = new { Text = text, Language = language };
            CommandDefinition command = new CommandDefinition(
                commandText: deleteQuery,
                parameters: parameters,
                cancellationToken: cancellationToken
            );

            int rows = await db.ExecuteAsync(command);
            return rows > 0;
        }
    }

    public async Task<List<WordBody>> GetWordsAsync(CancellationToken cancellationToken)
    {
        using (SqliteConnection db = new SqliteConnection(_connectionString))
        {
            await db.OpenAsync(cancellationToken);

            const string query = @"
                SELECT Text, Language
                FROM Word;
            ";

            CommandDefinition command = new CommandDefinition(
                commandText: query,
                cancellationToken: cancellationToken
            );

            IEnumerable<WordBody> result = await db.QueryAsync<WordBody>(command);
            List<WordBody> words = result.ToList();
            return words;
        }
    }

    public async Task<string> GetRandomWordAsync(string language, CancellationToken cancellationToken)
    {
        using (SqliteConnection db = new SqliteConnection(_connectionString))
        {
            await db.OpenAsync(cancellationToken);

            const string query = @"
                SELECT Text
                FROM Word
                WHERE Language = @Language
                ORDER BY RANDOM()
                LIMIT 1;
            ";

            object parameters = new { Language = language };
            CommandDefinition command = new CommandDefinition(
                commandText: query,
                parameters: parameters,
                cancellationToken: cancellationToken
            );

            string randomWord = await db.QueryFirstOrDefaultAsync<string>(command);
            return randomWord;
        }
    }
}
