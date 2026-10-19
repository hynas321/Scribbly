using Testcontainers.MsSql;
using Testcontainers.Redis;

namespace Scribbly.WebApi.IntegrationTests.Infrastructure;

public sealed class DockerContainersFixture : IAsyncLifetime
{
    private const string SqlPassword = "Scribbly_Test_123!";

    private MsSqlContainer? _sqlServer;
    private RedisContainer? _redis;

    public string SqlConnectionString { get; private set; } = string.Empty;

    public string RedisConnectionString => _redis?.GetConnectionString() ?? string.Empty;

    public Exception? StartupFailure { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            _sqlServer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-CU12-ubuntu-20.04")
                .WithPassword(SqlPassword)
                .Build();
            _redis = new RedisBuilder()
                .WithImage("redis:7.2-alpine")
                .Build();

            await Task.WhenAll(
                _sqlServer!.StartAsync(),
                _redis!.StartAsync());

            await InitializeSqlDatabaseAsync();
            SqlConnectionString = BuildDatabaseConnectionString(_sqlServer!.GetConnectionString());
        }
        catch (Exception exception)
        {
            StartupFailure = exception;
        }
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            DisposeContainerAsync(_sqlServer),
            DisposeContainerAsync(_redis));
    }

    public void EnsureStarted()
    {
        if (StartupFailure is not null)
        {
            throw new InvalidOperationException(
                "Docker containers failed to initialize.",
                StartupFailure);
        }
    }

    private async Task InitializeSqlDatabaseAsync()
    {
        string repositoryRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            ".."));
        string dockerDirectory = Path.Combine(
            repositoryRoot,
            "Scribbly.Infrastructure",
            "Docker");
        string migrationsDirectory = Path.Combine(
            repositoryRoot,
            "Scribbly.Infrastructure",
            "Database",
            "Migrations");

        await _sqlServer!.ExecScriptAsync(
            await File.ReadAllTextAsync(Path.Combine(dockerDirectory, "init.sql")));

        foreach (string migration in Directory.GetFiles(migrationsDirectory, "*.sql").OrderBy(path => path))
        {
            await _sqlServer!.ExecScriptAsync(await File.ReadAllTextAsync(migration));
        }
    }

    private static string BuildDatabaseConnectionString(string connectionString)
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "MyDatabase",
            TrustServerCertificate = true
        };
        return builder.ConnectionString;
    }

    private static async Task DisposeContainerAsync(IAsyncDisposable? container)
    {
        if (container is null)
        {
            return;
        }

        try
        {
            await container.DisposeAsync();
        }
        catch
        {
        }
    }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute()
    {
        try
        {
            _ = new MsSqlBuilder()
                .WithPassword("Scribbly_Test_123!")
                .Build();
        }
        catch
        {
            Skip = "Docker daemon is required for this integration test.";
        }
    }
}
