using Scribbly.Application.Abstractions.Canvas;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Abstractions.Security;
using Scribbly.Application.Features.Settings;
using Scribbly.Application.Features.Settings.Configuration;
using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Persistence;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Rounds;

public partial class GameRoundService : IGameRoundService
{
    private readonly IGameStateStore _store;
    private readonly ICanvasStateStore _canvas;
    private readonly IRoomCredentialService _credentials;
    private readonly GameSettingsOptions _settingsOptions;

    public GameRoundService(
        IGameStateStore store,
        ICanvasStateStore canvas,
        IRoomCredentialService credentials,
        GameSettingsOptions settingsOptions)
    {
        _store = store;
        _canvas = canvas;
        _credentials = credentials;
        _settingsOptions = settingsOptions;
    }

    public Task<StartSessionResult> StartSessionAsync(
        string gameHash,
        GameSettings settings,
        CancellationToken cancellationToken = default) =>
        StartSessionCoreAsync(gameHash, settings, null, false, cancellationToken);
    public Task<StartSessionResult> ResumeSessionAsync(
        string gameHash,
        GameSettings settings,
        CancellationToken cancellationToken = default) =>
        StartSessionCoreAsync(gameHash, settings, null, true, cancellationToken);
    public Task<StartSessionResult> StartAuthorizedSessionAsync(
        string gameHash, string roomAccessToken, GameSettings settings, CancellationToken cancellationToken = default) =>
        StartSessionCoreAsync(gameHash, settings, _credentials.ComputeCredentialHash(roomAccessToken), false, cancellationToken);
    public Task<StartSessionResult> ResumeAuthorizedSessionAsync(
        string gameHash, string roomAccessToken, GameSettings settings, CancellationToken cancellationToken = default) =>
        StartSessionCoreAsync(gameHash, settings, _credentials.ComputeCredentialHash(roomAccessToken), true, cancellationToken);

}
