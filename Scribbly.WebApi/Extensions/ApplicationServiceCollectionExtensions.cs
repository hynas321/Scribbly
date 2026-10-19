using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Abstractions.Security;
using Scribbly.Application.Features.Accounts;
using Scribbly.Application.Features.Canvas;
using Scribbly.Application.Features.Chat;
using Scribbly.Application.Features.Games;
using Scribbly.Application.Features.Rounds;
using Scribbly.Application.Features.Settings;
using Scribbly.Application.GameSessions;
using Scribbly.Application.Security;
using Scribbly.Application.Services;
using Scribbly.Application.Services.Interfaces;

namespace Scribbly.WebApi.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddScribblyApplication(this IServiceCollection services)
    {
        services.AddSingleton<IRoomCredentialService, RoomCredentialService>();
        services.AddSingleton<GameSessionExecutionContext>();
        services.AddSingleton<IAccountScoreService, AccountScoreService>();
        services.AddSingleton<AccountSignInService>();
        services.AddSingleton<AccountSessionService>();
        services.AddSingleton<IGameResultService, GameResultService>();
        services.AddSingleton<IRandomPromptService, SharedStateRandomPromptService>();
        services.AddSingleton<IDistributedGameSessionCoordinator, DistributedGameSessionCoordinator>();
        services.AddSingleton<IGameLifecycleService, GameLifecycleService>();
        services.AddSingleton<IGameChatService, GameChatService>();
        services.AddSingleton<IGameCanvasService, GameCanvasService>();
        services.AddSingleton<IGameSettingsService, GameSettingsService>();
        services.AddSingleton<IGameRoundService, GameRoundService>();
        services.AddSingleton<IDrawingPlayerSelector, RandomDrawingPlayerSelector>();
        services.AddSingleton(GameSessionTimingOptions.Default);
        return services;
    }
}
