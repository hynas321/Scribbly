using Testcontainers.MsSql;

namespace WebApiIntegrationTests.Helpers;

[CollectionDefinition("Sql Server Collection")]
public class SqlServerCollection : ICollectionFixture<SqlServerCollectionFixture> {}

public class SqlServerCollectionFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer;

    public SqlServerCollectionFixture()
    {
        _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-CU12-ubuntu-20.04")
            .WithPassword("YourStrong@Passw0rd")
            .WithName($"test-mssql-integration-{Guid.NewGuid()}")
            .WithPortBinding(1433, true)
            .WithCleanUp(true)
            .Build();
    }

    public string ConnectionString => _msSqlContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        await InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync().AsTask();
    }

    private async Task InitializeDatabaseAsync()
    {
        using var connection = new Microsoft.Data.SqlClient.SqlConnection(ConnectionString);
        await connection.OpenAsync();

        await CreateTablesAsync(connection);
        await SeedDataAsync(connection);
    }

    private async Task CreateTablesAsync(Microsoft.Data.SqlClient.SqlConnection connection)
    {
        var createWordTable = @"
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Word')
            BEGIN
                CREATE TABLE Word (
                    Text NVARCHAR(255),
                    Language NVARCHAR(50),
                    PRIMARY KEY (Text, Language)
                );
            END";

        using var command = new Microsoft.Data.SqlClient.SqlCommand(createWordTable, connection);
        await command.ExecuteNonQueryAsync();

        var createAccountTable = @"
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Account')
            BEGIN
                CREATE TABLE Account (
                    Id NVARCHAR(100) PRIMARY KEY,
                    AccessToken NVARCHAR(MAX),
                    Email NVARCHAR(255),
                    Name NVARCHAR(255),
                    GivenName NVARCHAR(255),
                    FamilyName NVARCHAR(255),
                    Score INT
                );
            END";

        using var command2 = new Microsoft.Data.SqlClient.SqlCommand(createAccountTable, connection);
        await command2.ExecuteNonQueryAsync();
    }

    private async Task SeedDataAsync(Microsoft.Data.SqlClient.SqlConnection connection)
    {
        using var checkCommand = new Microsoft.Data.SqlClient.SqlCommand("SELECT COUNT(*) FROM Word", connection);
        var existingWordCount = (int)await checkCommand.ExecuteScalarAsync();

        if (existingWordCount > 0)
        {
            return;
        }

        var wordData = new[]
        {
            ("apple", "en"), ("jabłko", "pl"),
            ("ball", "en"), ("piłka", "pl"),
            ("book", "en"), ("książka", "pl"),
            ("chair", "en"), ("krzesło", "pl"),
            ("lamp", "en"), ("lampa", "pl"),
            ("fan", "en"), ("wentylator", "pl"),
            ("shirt", "en"), ("koszula", "pl"),
            ("pants", "en"), ("spodnie", "pl"),
            ("glasses", "en"), ("okulary", "pl"),
            ("wallet", "en"), ("portfel", "pl"),
            ("blanket", "en"), ("koc", "pl"),
            ("shovel", "en"), ("łopata", "pl"),
            ("ladder", "en"), ("drabina", "pl"),
            ("hammer", "en"), ("młotek", "pl"),
            ("key", "en"), ("klucz", "pl"),
            ("rope", "en"), ("lina", "pl"),
            ("watch", "en"), ("zegarek", "pl"),
            ("telescope", "en"), ("teleskop", "pl"),
            ("umbrella", "en"), ("parasol", "pl"),
            ("pencil", "en"), ("ołówek", "pl"),
            ("pen", "en"), ("długopis", "pl"),
            ("scissors", "en"), ("nożyczki", "pl"),
            ("paper", "en"), ("papier", "pl"),
            ("bottle", "en"), ("butelka", "pl"),
            ("bucket", "en"), ("wiadro", "pl"),
            ("candle", "en"), ("świeca", "pl"),
            ("pillow", "en"), ("poduszka", "pl"),
            ("spoon", "en"), ("łyżka", "pl"),
            ("fork", "en"), ("widelec", "pl"),
            ("knife", "en"), ("nóż", "pl"),
            ("cup", "en"), ("kubek", "pl"),
            ("broom", "en"), ("miotła", "pl"),
            ("nail", "en"), ("gwóźdź", "pl"),
            ("window", "en"), ("okno", "pl"),
            ("door", "en"), ("drzwi", "pl"),
            ("television", "en"), ("telewizor", "pl"),
            ("camera", "en"), ("aparat", "pl"),
            ("computer", "en"), ("komputer", "pl"),
            ("remote", "en"), ("pilot", "pl"),
            ("couch", "en"), ("kanapa", "pl"),
            ("carpet", "en"), ("dywan", "pl"),
            ("shelf", "en"), ("półka", "pl"),
            ("mirror", "en"), ("lustro", "pl"),
            ("picture", "en"), ("obraz", "pl"),
            ("clock", "en"), ("zegar", "pl"),
            ("toothbrush", "en"), ("szczoteczka", "pl"),
            ("soap", "en"), ("mydło", "pl"),
            ("toothpaste", "en"), ("pasta", "pl"),
            ("shampoo", "en"), ("szampon", "pl"),
            ("towel", "en"), ("ręcznik", "pl"),
            ("bicycle", "en"), ("rower", "pl"),
            ("calculator", "en"), ("kalkulator", "pl"),
            ("mouse", "en"), ("mysz", "pl"),
            ("keyboard", "en"), ("klawiatura", "pl"),
            ("charger", "en"), ("ładowarka", "pl"),
            ("tablet", "en"), ("tablet", "pl"),
            ("notebook", "en"), ("zeszyt", "pl"),
            ("radio", "en"), ("radio", "pl")
        };

        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var (text, language) in wordData)
            {
                using var insertCommand = new Microsoft.Data.SqlClient.SqlCommand(
                    "INSERT INTO Word (Text, Language) VALUES (@Text, @Language)",
                    connection,
                    transaction);
                insertCommand.Parameters.AddWithValue("@Text", text);
                insertCommand.Parameters.AddWithValue("@Language", language);
                await insertCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task CleanDatabaseAsync()
    {
        using var connection = new Microsoft.Data.SqlClient.SqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var command = new Microsoft.Data.SqlClient.SqlCommand(
            "DELETE FROM Word; DELETE FROM Account;", connection);

        await command.ExecuteNonQueryAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await CleanDatabaseAsync();
        await InitializeDatabaseAsync();
    }
}
