using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Scribbly.Application.Abstractions.Canvas;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Features.Canvas;
using Scribbly.Application.Features.Canvas.Configuration;
using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Persistence;
using Scribbly.Domain.Entities;
using Scribbly.Infrastructure.Configuration;
using Scribbly.Infrastructure.Redis;
using Scribbly.Infrastructure.Repositories;
using Scribbly.WebApi.UnitTests.TestDoubles;
using StackExchange.Redis;
using Xunit;

namespace Scribbly.WebApi.IntegrationTests.Infrastructure;

public class ContainerBackedInfrastructureIntegrationTests
    : IClassFixture<DockerContainersFixture>
{
    private readonly DockerContainersFixture _containers;

    public ContainerBackedInfrastructureIntegrationTests(DockerContainersFixture containers)
    {
        _containers = containers;
    }

    [DockerFact]
    public async Task SqlServerRepositories_PersistAccountsResultsAndPrompts()
    {
        _containers.EnsureStarted();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DatabaseConnectionString"] = _containers.SqlConnectionString
            })
            .Build();
        DatabaseOptions databaseOptions = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>()!;
        var accounts = new AccountRepository(databaseOptions);
        var results = new GameResultRepository(databaseOptions);
        var prompts = new PromptRepository(databaseOptions, PromptTestOptions.Create());
        string accountId = $"container-account-{Guid.NewGuid():N}";
        string gameHash = $"container-game-{Guid.NewGuid():N}";
        var account = new Account
        {
            Id = accountId,
            Name = "Container tester",
            Email = "container@example.test"
        };

        Assert.True(await accounts.AddAccountAsync(account, CancellationToken.None));
        await results.SaveAsync(
            gameHash,
            new[]
            {
                new FinalPlayerScore("container-hash", 42, "Container tester")
            },
            CancellationToken.None);
        bool promptAdded = await prompts.AddAsync(
            new GamePrompt("container prompt", "en", "integration", "word"),
            CancellationToken.None);

        Account storedAccount = (await accounts.GetAccountAsync(
            accountId,
            CancellationToken.None))!;
        IReadOnlyList<FinalPlayerScore> storedResults = await results.GetScoresAsync(
            gameHash,
            CancellationToken.None);
        IReadOnlyCollection<GamePrompt> storedPrompts = await prompts.GetAllAsync(
            CancellationToken.None);

        Assert.Equal(accountId, storedAccount.Id);
        Assert.Single(storedResults);
        Assert.Equal(42, storedResults[0].Score);
        Assert.True(promptAdded);
        Assert.Contains(
            storedPrompts,
            prompt => prompt.Text == "container prompt" && prompt.Category == "integration");
    }

    [DockerFact]
    public async Task RedisStores_PersistGameCanvasAndConnectionState()
    {
        _containers.EnsureStarted();

        using IConnectionMultiplexer connection = await ConnectionMultiplexer.ConnectAsync(
            _containers.RedisConnectionString);
        var options = Options.Create(new RedisOptions
        {
            Enabled = true,
            ConnectionString = _containers.RedisConnectionString,
            KeyPrefix = $"scribbly-test-{Guid.NewGuid():N}",
            GameTtlHours = 1,
            LockTimeoutSeconds = 2,
            LockLeaseSeconds = 5
        });
        var canvasOptions = CanvasTestOptions.Create();
        var games = new RedisGameStateStore(connection, options);
        var canvas = new RedisCanvasStateStore(connection, options, canvasOptions);
        var connectionIndex = new RedisGameConnectionIndex(connection, options);
        string gameHash = $"container-game-{Guid.NewGuid():N}";
        var game = new Game
        {
            HostCredentialHash = "host-hash",
            GameState =
            {
                HostPlayerUsername = "host",
                Players =
                {
                    new Player
                    {
                        Username = "host",
                        CredentialHash = "host-hash",
                        ConnectionId = "container-connection"
                    }
                }
            }
        };

        try
        {
            Assert.True(await games.CreateAsync(gameHash, game));

            GameStoreResult<GameSettings> mutation = await games.MutateAsync(
                gameHash,
                current =>
                {
                    current.GameSettings.DrawingTimeSeconds = 45;
                    return GameMutation<GameSettings>.Persist(current.GameSettings);
                });
            Game? storedGame = await games.GetSnapshotAsync(gameHash);
            CanvasStoreAppendResult stroke = await canvas.AppendChunkAsync(
                gameHash,
                new CanvasStrokeChunk(
                    1,
                    "container-stroke",
                    0,
                    "#112233",
                    4,
                    new ushort[] { 10, 20, 30, 40 }));
            CanvasSnapshot snapshot = await canvas.GetSnapshotAsync(gameHash, 1);
            string? indexedGame = await connectionIndex.FindGameHashAsync("container-connection");

            Assert.True(mutation.Found);
            Assert.Equal(45, storedGame!.GameSettings.DrawingTimeSeconds);
            Assert.Equal(CanvasStoreAppendStatus.Applied, stroke.Status);
            Assert.Equal(new ushort[] { 10, 20, 30, 40 }, snapshot.Strokes.Single().Coordinates);
            Assert.Equal(gameHash, indexedGame);
        }
        finally
        {
            await canvas.DeleteAsync(gameHash);
            await games.DeleteAsync(gameHash);
        }
    }
}
