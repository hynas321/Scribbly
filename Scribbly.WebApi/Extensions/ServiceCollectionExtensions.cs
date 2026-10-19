using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using Scribbly.Application.Features.Accounts;
using Scribbly.Application.Features.Accounts.Configuration;
using Scribbly.Application.Features.Canvas.Configuration;
using Scribbly.Application.Features.Chat.Configuration;
using Scribbly.Application.Features.Games;
using Scribbly.Application.Features.Games.Configuration;
using Scribbly.Application.Features.Prompts.Configuration;
using Scribbly.Application.Features.Settings.Configuration;
using Scribbly.Application.GameSessions;
using Scribbly.Infrastructure;
using Scribbly.Infrastructure.Redis;
using Scribbly.WebApi.Configuration;
using Scribbly.WebApi.GameSessions;

namespace Scribbly.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public const string ReactAppCorsPolicy = "AllowReactApp";

    public static IServiceCollection AddScribblyBackend(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        RedisOptions redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>() ?? new();

        if (!environment.IsEnvironment("Testing") && !redisOptions.Enabled)
        {
            throw new InvalidOperationException("Redis must be enabled outside the Testing environment.");
        }

        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });
        services.AddScribblyApplication();
        services.AddInfrastructure(configuration);
        AuthenticationOptions authenticationOptions = configuration.GetSection(AuthenticationOptions.SectionName).Get<AuthenticationOptions>()
            ?? throw new InvalidOperationException($"{AuthenticationOptions.SectionName} configuration is required.");
        services.AddSingleton(authenticationOptions);
        GameOptions gameOptions = configuration.GetSection(GameOptions.SectionName).Get<GameOptions>()
            ?? throw new InvalidOperationException($"{GameOptions.SectionName} configuration is required.");
        services.AddSingleton(gameOptions);
        CanvasOptions canvasOptions = configuration.GetSection(CanvasOptions.SectionName).Get<CanvasOptions>()
            ?? throw new InvalidOperationException($"{CanvasOptions.SectionName} configuration is required.");
        ValidateCanvasOptions(canvasOptions);
        services.AddSingleton(canvasOptions);
        ChatOptions chatOptions = configuration.GetSection(ChatOptions.SectionName).Get<ChatOptions>()
            ?? throw new InvalidOperationException($"{ChatOptions.SectionName} configuration is required.");
        ValidateChatOptions(chatOptions);
        services.AddSingleton(chatOptions);
        PromptOptions promptOptions = configuration.GetSection(PromptOptions.SectionName).Get<PromptOptions>()
            ?? throw new InvalidOperationException($"{PromptOptions.SectionName} configuration is required.");
        ValidatePromptOptions(promptOptions);
        services.AddSingleton(promptOptions);
        GameSettingsOptions gameSettingsOptions = configuration.GetSection(GameSettingsOptions.SectionName).Get<GameSettingsOptions>()
            ?? throw new InvalidOperationException($"{GameSettingsOptions.SectionName} configuration is required.");
        ValidateGameSettingsOptions(gameSettingsOptions);
        services.AddSingleton(gameSettingsOptions);
        services.AddSingleton(GameLifecycleOptions.FromOptions(gameOptions));
        services.AddSingleton(AccountSessionOptions.FromOptions(authenticationOptions));
        services.AddSingleton<IGameSessionEvents, SignalRGameSessionEvents>();
        services.AddSingleton<IGameSessionRunner, DistributedGameSessionRunner>();
        services.AddHostedService<DistributedGameSessionCoordinator>(p => (DistributedGameSessionCoordinator)p.GetRequiredService<IDistributedGameSessionCoordinator>());
        services.AddHostedService<DisconnectedPlayerCleanupService>();
        services.AddHostedService<GameSessionRecoveryService>();
        services.AddHostedService<CompletedGameResultRecoveryService>();

        ISignalRServerBuilder signalR = services.AddSignalR(options => options.EnableDetailedErrors = false)
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.PayloadSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });
        if (redisOptions.Enabled)
        {
            signalR.AddStackExchangeRedis(redisOptions.ConnectionString, options =>
                options.Configuration.ChannelPrefix = RedisChannel.Literal($"{redisOptions.KeyPrefix}:signalr"));
        }

        services.AddCors(options =>
        {
            CorsOptions corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new();
            if (corsOptions.AllowedOrigins.Length == 0)
            {
                throw new InvalidOperationException(
                    $"At least one CORS origin must be configured in {CorsOptions.SectionName}:{nameof(CorsOptions.AllowedOrigins)}.");
            }
            options.AddPolicy(ReactAppCorsPolicy, policy => policy
                .WithOrigins(corsOptions.AllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
        });
        services.AddHttpClient();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }

    private static void ValidateCanvasOptions(CanvasOptions options)
    {
        if (options.Width <= 0 || options.Height <= 0 ||
            options.MaxPointsPerChunk <= 0 || options.MaxPointsPerStroke <= 0 ||
            options.MaxStrokes <= 0 || options.MaxTotalPoints <= 0 ||
            options.MaxStrokeIdLength <= 0 || options.MinThickness <= 0 ||
            options.MaxThickness < options.MinThickness ||
            options.DefaultThickness < options.MinThickness ||
            options.DefaultThickness > options.MaxThickness || options.ThicknessStep <= 0 ||
            options.SampleIntervalMilliseconds <= 0 || options.FlushIntervalMilliseconds <= 0)
        {
            throw new InvalidOperationException("Canvas configuration contains invalid limits or timing values.");
        }
    }

    private static void ValidateChatOptions(ChatOptions options)
    {
        if (options.MaxMessageCount <= 0 || options.MaxMessageLength <= 0)
        {
            throw new InvalidOperationException("Chat configuration contains invalid message limits.");
        }
    }

    private static void ValidatePromptOptions(PromptOptions options)
    {
        if (options.MaxTextLength <= 0 || options.MaxCategoryLength <= 0)
        {
            throw new InvalidOperationException("Prompt configuration contains invalid length limits.");
        }
    }

    private static void ValidateGameSettingsOptions(GameSettingsOptions options)
    {
        if (options.MinDrawingTimeSeconds <= 0 ||
            options.MaxDrawingTimeSeconds < options.MinDrawingTimeSeconds ||
            options.DrawingTimeStepSeconds <= 0 ||
            options.MinRounds <= 0 ||
            options.MaxRounds < options.MinRounds ||
            options.MinPlayers <= 0 ||
            options.MaxPlayers < options.MinPlayers ||
            options.MaxPromptCategoryLength <= 0 ||
            options.MinHintPercentage <= 0 ||
            options.MaxHintPercentage < options.MinHintPercentage ||
            options.HintPercentageStep <= 0)
        {
            throw new InvalidOperationException("GameSettings configuration contains invalid policy values.");
        }
    }
}
