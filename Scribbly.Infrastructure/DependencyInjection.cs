using StackExchange.Redis;
using Scribbly.Application.Abstractions.Canvas;
using Scribbly.Application.Abstractions.Identity;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Abstractions.Sessions;
using Scribbly.Application.Abstractions.Time;
using Scribbly.Application.Abstractions.Prompts;
using Scribbly.Application.GameSessions;
using Scribbly.Infrastructure.Identity;
using Scribbly.Infrastructure.Configuration;
using Scribbly.Infrastructure.Redis;
using Scribbly.Infrastructure.Repositories;
using Scribbly.Infrastructure.Sessions;
using Scribbly.Infrastructure.Time;

namespace Scribbly.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        DatabaseOptions databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new();
        services.AddSingleton(databaseOptions);
        services.AddSingleton<IGoogleIdentityVerifier, GoogleIdentityVerifier>();
        services.AddSingleton<IAccountRepository, AccountRepository>();
        services.AddSingleton<IGameResultRepository, GameResultRepository>();
        services.AddSingleton<PromptRepository>();
        services.AddSingleton<IPromptRepository>(p => p.GetRequiredService<PromptRepository>());
        services.AddSingleton<IRandomPromptProvider>(p => p.GetRequiredService<PromptRepository>());

        RedisOptions redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>() ?? new();
        IHealthChecksBuilder healthChecks = services.AddHealthChecks();
        if (!redisOptions.Enabled)
        {
            return services;
        }
        if (string.IsNullOrWhiteSpace(redisOptions.ConnectionString))
        {
            throw new InvalidOperationException(
                $"{RedisOptions.SectionName}:{nameof(RedisOptions.ConnectionString)} is required when Redis is enabled.");
        }

        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions.ConnectionString));
        services.AddSingleton<IGameStateStore, RedisGameStateStore>();
        services.AddSingleton<IGameConnectionIndex, RedisGameConnectionIndex>();
        services.AddSingleton<ICanvasStateStore, RedisCanvasStateStore>();
        services.AddSingleton<IAccountSessionStore, RedisAccountSessionStore>();
        services.AddSingleton<IGameSessionLeaseProvider, RedisGameSessionLeaseProvider>();
        services.AddSingleton<IGameSessionCancellationBus, RedisGameSessionCancellationBus>();
        services.AddSingleton<IAsyncDelay, SystemAsyncDelay>();
        services.AddSingleton(GameSessionLeaseExecutionOptions.FromLeaseDuration(TimeSpan.FromSeconds(redisOptions.SessionLeaseSeconds)));
        services.AddSingleton<IGameSessionLeaseExecutor, GameSessionLeaseExecutor>();
        healthChecks.AddCheck<RedisHealthCheck>("redis", tags: new[] { "ready" });
        return services;
    }
}
