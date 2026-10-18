using Dapper;
using Microsoft.Data.SqlClient;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Abstractions.Prompts;
using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Features.Prompts.Configuration;
using Scribbly.Infrastructure.Configuration;

namespace Scribbly.Infrastructure.Repositories;

public class PromptRepository : IPromptRepository, IRandomPromptProvider
{
    private readonly string _connectionString;
    private readonly PromptOptions _options;

    public PromptRepository(DatabaseOptions databaseOptions, PromptOptions options)
    {
        _connectionString = databaseOptions.DatabaseConnectionString;

        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException(
                $"{DatabaseOptions.SectionName}:{DatabaseOptions.DatabaseConnectionStringName} is required.");
        }
        _options = options;
    }

    public async Task<bool> AddAsync(GamePrompt prompt, CancellationToken cancellationToken)
    {
        if (!GamePromptPolicy.IsValid(prompt, _options))
        {
            throw new ArgumentException("Invalid prompt.", nameof(prompt));
        }
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(cancellationToken);
        try
        {
            return await db.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO Prompt (Text, Language, Category, Kind)
                VALUES (@Text, @Language, @Category, @Kind);",
                Normalize(prompt), cancellationToken: cancellationToken)) > 0;
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(GamePrompt prompt, CancellationToken cancellationToken)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(cancellationToken);
        return await db.ExecuteAsync(new CommandDefinition(@"
            DELETE FROM Prompt
            WHERE Text = @Text AND Language = @Language AND Kind = @Kind;",
            Normalize(prompt), cancellationToken: cancellationToken)) > 0;
    }

    public async Task<IReadOnlyCollection<GamePrompt>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(cancellationToken);
        IEnumerable<GamePrompt> rows = await db.QueryAsync<GamePrompt>(new CommandDefinition(@"
            SELECT Text, Language, Category, Kind
            FROM Prompt
            ORDER BY Language, Category, Kind, Text;", cancellationToken: cancellationToken));
        return rows.ToArray();
    }

    public async Task<IReadOnlyCollection<string>> GetCategoriesAsync(
        string language,
        CancellationToken cancellationToken)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(cancellationToken);
        IEnumerable<string> rows = await db.QueryAsync<string>(new CommandDefinition(@"
            SELECT DISTINCT Category
            FROM Prompt
            WHERE Language = @Language
            ORDER BY Category;",
            new
            {
                Language = language
            }, cancellationToken: cancellationToken));
        return rows.ToArray();
    }

    public async Task<GamePrompt?> GetRandomPromptAsync(
        string language, IReadOnlyCollection<string> categories, CancellationToken cancellationToken = default)
    {
        bool filterByCategories = categories is { Count: > 0 };
        string categoryFilter = filterByCategories ? " AND Category IN @Categories" : string.Empty;
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(cancellationToken);
        return await db.QueryFirstOrDefaultAsync<GamePrompt>(new CommandDefinition($@"
            DECLARE @Kind VARCHAR(16) = (
                SELECT TOP 1 Kind
                FROM (SELECT DISTINCT Kind FROM Prompt WHERE Language = @Language{categoryFilter}) kinds
                ORDER BY NEWID()
            );
            DECLARE @Category NVARCHAR(64) = (
                SELECT TOP 1 Category
                FROM (SELECT DISTINCT Category FROM Prompt WHERE Language = @Language AND Kind = @Kind{categoryFilter}) categories
                ORDER BY NEWID()
            );
            SELECT TOP 1 Text, Language, Category, Kind
            FROM Prompt
            WHERE Language = @Language AND Kind = @Kind AND Category = @Category
            ORDER BY NEWID();",
            new
            {
                Language = language,
                Categories = categories ?? Array.Empty<string>()
            }, cancellationToken: cancellationToken));
    }

    private static GamePrompt Normalize(GamePrompt prompt) => prompt with
    {
        Text = prompt.Text.Trim(),
        Language = prompt.Language.Trim().ToLowerInvariant(),
        Category = prompt.Category.Trim(),
        Kind = prompt.Kind.Trim().ToLowerInvariant()
    };
}
