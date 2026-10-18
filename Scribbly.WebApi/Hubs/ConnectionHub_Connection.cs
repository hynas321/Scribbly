using Microsoft.AspNetCore.SignalR;
using Scribbly.Application.Features.Canvas;
using Scribbly.Application.Features.Chat;
using Scribbly.Application.Features.Games;
using Scribbly.Application.Features.Accounts;
using Scribbly.Application.Features.Settings;
using Scribbly.Application.Features.Settings.Configuration;
using Scribbly.Application.GameSessions;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.Auth;
using Scribbly.WebApi.Hubs.Static;

namespace Scribbly.WebApi.Hubs;

public partial class HubConnection : Hub
{
    private readonly IGameLifecycleService _games;
    private readonly IGameChatService _chat;
    private readonly IGameCanvasService _canvas;
    private readonly IGameSettingsService _settings;
    private readonly IDistributedGameSessionCoordinator _sessionCoordinator;
    private readonly IGameResultService _results;
    private readonly ILogger<HubConnection> _logger;
    private readonly GameSettingsOptions _settingsOptions;

    public HubConnection(
        ILogger<HubConnection> logger,
        IGameLifecycleService games,
        IGameChatService chat,
        IGameCanvasService canvas,
        IGameSettingsService settings,
        IDistributedGameSessionCoordinator sessionCoordinator,
        IGameResultService results,
        GameSettingsOptions settingsOptions)
    {
        _logger = logger;
        _games = games;
        _chat = chat;
        _canvas = canvas;
        _settings = settings;
        _sessionCoordinator = sessionCoordinator;
        _results = results;
        _settingsOptions = settingsOptions;
    }

    protected string RequireRoomAccessToken()
    {
        string? token = Context.GetHttpContext() is { } http ? BearerTokenResolver.FromHttpRequest(http.Request, allowSignalRQueryFallback: true) : null;
        return string.IsNullOrWhiteSpace(token) ? throw new HubException("Room authorization is required.") : token;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            DisconnectedPlayerResult result = await _games.RemoveByConnectionIdAsync(Context.ConnectionId, CancellationToken.None);
            if (result.Username is null || result.GameHash is null)
            {
                return;
            }
            if (result.GameRemoved)
            {
                await _sessionCoordinator.RequestCancellationAsync(result.GameHash, CancellationToken.None);
            }
            await Clients.GroupExcept(result.GameHash, Context.ConnectionId)
                .SendAsync(HubMessages.OnPlayerScoresUpdated, result.Scores, CancellationToken.None);
            await Clients.GroupExcept(result.GameHash, Context.ConnectionId)
                .SendAsync(HubMessages.OnAnnouncement, new AnnouncementMessage
                {
                    Text = $"{result.Username} disconnected. Waiting for reconnect...",
                    BootstrapBackgroundColor = BootstrapColors.Yellow
                }, CancellationToken.None);
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
