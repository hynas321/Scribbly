using Dapper;
using WebApi.Api.Models.HttpRequest;
using WebApi.Infrastructure.Repositories.Interfaces;
using System.Data.SqlClient;

namespace WebApi.Infrastructure.Repositories;

public class WordRepository : IWordRepository
{
    private readonly string _connectionString;

    public WordRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DatabaseConnectionString");
    }

    public async Task<bool> AddWordAsync(string text, string language, CancellationToken cancellationToken)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            await db.OpenAsync(cancellationToken);

            const string insertQuery = @"
                INSERT INTO Word (Text, Language)
                VALUES (@Text, @Language);
            ";

            object parameters = new { Text = text, Language = language };
            CommandDefinition command = new(
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
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            await db.OpenAsync(cancellationToken);

            const string deleteQuery = @"
                DELETE FROM Word
                WHERE Text = @Text
                  AND Language = @Language;
            ";

            object parameters = new { Text = text, Language = language };
            CommandDefinition command = new(
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
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            await db.OpenAsync(cancellationToken);

            const string query = @"
                SELECT Text, Language
                FROM Word;
            ";

            CommandDefinition command = new(
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
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            await db.OpenAsync(cancellationToken);

            const string query = @"
                SELECT TOP 1 Text
                FROM Word
                WHERE Language = @Language
                ORDER BY NEWID();
            ";

            object parameters = new { Language = language };
            CommandDefinition command = new(
                commandText: query,
                parameters: parameters,
                cancellationToken: cancellationToken
            );

            string randomWord = await db.QueryFirstOrDefaultAsync<string>(command);
            return randomWord;
        }
    }
}
