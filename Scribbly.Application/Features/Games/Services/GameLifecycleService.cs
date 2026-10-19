using Scribbly.Application.Abstractions.Canvas;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Abstractions.Security;
using Scribbly.Application.Persistence;
using Scribbly.Application.Features.Settings;
using Scribbly.Application.Features.Settings.Configuration;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Games;

public partial class GameLifecycleService : IGameLifecycleService
{
    private readonly IGameStateStore _store;
    private readonly IGameConnectionIndex _connectionIndex;
    private readonly ICanvasStateStore _canvas;
    private readonly IRoomCredentialService _credentials;
    private readonly GameLifecycleOptions _options;
    private readonly GameSettingsOptions _settingsOptions;

    public GameLifecycleService(
        IGameStateStore store,
        IGameConnectionIndex connectionIndex,
        ICanvasStateStore canvas,
        IRoomCredentialService credentials,
        GameLifecycleOptions? options,
        GameSettingsOptions settingsOptions)
    {
        _store = store;
        _connectionIndex = connectionIndex;
        _canvas = canvas;
        _credentials = credentials;
        _options = options ?? GameLifecycleOptions.Default;
        _settingsOptions = settingsOptions;
    }

    public Task<CreateGameResult> CreateAsync(string hostUsername, CancellationToken cancellationToken = default) =>
        CreateAsync(hostUsername, "en", cancellationToken);

    public async Task<CreateGameResult> CreateAsync(
        string hostUsername,
        string promptLanguage,
        CancellationToken cancellationToken = default)
    {
        string normalizedUsername = NormalizeUsername(hostUsername);
        for (int attempt = 0; attempt < 10; attempt++)
        {
            string gameHash = _credentials.GenerateRoomCode();
            string accessToken = _credentials.GenerateAccessToken();
            var game = new Game { HostCredentialHash = _credentials.ComputeCredentialHash(accessToken) };
            game.GameState.HostPlayerUsername = normalizedUsername;
            game.GameSettings.PromptLanguage = GameSettingsPolicy.IsPromptLanguageValid(promptLanguage) ? promptLanguage : "en";

            if (await _store.CreateAsync(gameHash, game, cancellationToken))
            {
                return new(gameHash, accessToken, _options.MaxPlayers);
            }
        }
        throw new InvalidOperationException("Could not allocate a unique room code.");
    }

    public async Task<JoinCredentialResult> IssueJoinCredentialAsync(
        string gameHash,
        string username,
        CancellationToken cancellationToken = default)
    {
        string normalizedUsername = NormalizeUsername(username);
        string token = _credentials.GenerateAccessToken();
        string hash = _credentials.ComputeCredentialHash(token);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset expires = now + _options.JoinCredentialLifetime;

        GameStoreResult<JoinCredentialStatus> result = await _store.MutateAsync(gameHash,
            game => IssueJoinCredential(game, hash, normalizedUsername, now, expires), cancellationToken);

        if (!result.Found)
        {
            return new(JoinCredentialStatus.GameNotFound, MaxPlayers: _options.MaxPlayers);
        }
        return result.Value == JoinCredentialStatus.Issued
            ? new(JoinCredentialStatus.Issued, token, expires, _options.MaxPlayers)
            : new(result.Value!, MaxPlayers: _options.MaxPlayers);
    }

    public async Task<bool> ExistsAsync(string gameHash, CancellationToken cancellationToken = default) =>
        await _store.GetSnapshotAsync(gameHash, cancellationToken) is not null;

    public async Task<bool> DeleteAsync(string gameHash, CancellationToken cancellationToken = default)
    {
        bool deleted = await _store.DeleteAsync(gameHash, cancellationToken);

        await _canvas.DeleteAsync(gameHash, cancellationToken);
        return deleted;
    }

    public Task<Game?> GetSnapshotAsync(string gameHash, CancellationToken cancellationToken = default) =>
        _store.GetSnapshotAsync(gameHash, cancellationToken);

    public async Task<Player?> GetPlayerAsync(
        string gameHash,
        string roomAccessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roomAccessToken))
        {
            return null;
        }
        string hash = _credentials.ComputeCredentialHash(roomAccessToken);

        Game? game = await _store.GetSnapshotAsync(gameHash, cancellationToken);
        Player? player = game?.GameState.Players.FirstOrDefault(candidate => candidate.CredentialHash == hash);
        return player is null ? null : Copy(player);
    }

    public async Task<bool> PlayerExistsAsync(
        string gameHash,
        string roomAccessToken,
        CancellationToken cancellationToken = default) =>
        await GetPlayerAsync(gameHash, roomAccessToken, cancellationToken) is not null;

    public async Task<JoinGameResult> JoinAsync(
        string gameHash, string roomAccessToken, string connectionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roomAccessToken))
        {
            return new(JoinGameStatus.InvalidCredential);
        }
        string hash = _credentials.ComputeCredentialHash(roomAccessToken);

        GameStoreResult<JoinGameResult> result = await _store.MutateAsync(gameHash,
            game => Join(game, hash, connectionId, DateTimeOffset.UtcNow), cancellationToken);
        return result.Found ? result.Value! : new(JoinGameStatus.GameNotFound);
    }

    public async Task<JoinGameResult> ReconnectAsync(
        string gameHash, string roomAccessToken, string connectionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roomAccessToken))
        {
            return new(JoinGameStatus.InvalidCredential);
        }
        string hash = _credentials.ComputeCredentialHash(roomAccessToken);

        GameStoreResult<JoinGameResult> result = await _store.MutateAsync(gameHash,
            game => Reconnect(game, hash, connectionId, DateTimeOffset.UtcNow), cancellationToken);

        if (!result.Found)
        {
            return new(JoinGameStatus.GameNotFound);
        }

        if (result.Value!.Status == JoinGameStatus.ResumeExpired &&
            await _store.GetSnapshotAsync(gameHash, cancellationToken) is null)
        {
            await _canvas.DeleteAsync(gameHash, cancellationToken);
        }
        return result.Value;
    }

    public async Task<LeaveGameResult> LeaveAsync(
        string gameHash,
        string roomAccessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roomAccessToken))
        {
            return new(LeaveGameStatus.PlayerNotFound);
        }
        string hash = _credentials.ComputeCredentialHash(roomAccessToken);

        GameStoreResult<LeaveGameResult> result = await _store.MutateAsync(gameHash,
            game => Leave(game, hash, DateTimeOffset.UtcNow), cancellationToken);

        if (!result.Found)
        {
            return new(LeaveGameStatus.GameNotFound);
        }

        if (result.Value!.GameRemovedBecauseEmpty || result.Value.HostLeftLobby || result.Value.NotEnoughPlayers)
        {
            await _canvas.DeleteAsync(gameHash, cancellationToken);
        }
        return result.Value;
    }

    public async Task<KickPlayerResult> KickAsync(
        string gameHash, string roomAccessToken, string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roomAccessToken))
        {
            return new(KickPlayerStatus.Unauthorized);
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            return new(KickPlayerStatus.PlayerNotFound);
        }
        string hash = _credentials.ComputeCredentialHash(roomAccessToken);
        string normalizedUsername = username.Trim();

        GameStoreResult<KickPlayerResult> result = await _store.MutateAsync(gameHash,
            game => Kick(game, hash, normalizedUsername), cancellationToken);

        if (!result.Found)
        {
            return new(KickPlayerStatus.GameNotFound);
        }

        if (result.Value!.Status == KickPlayerStatus.Applied && result.Value.CanvasEpoch > 0)
        {
            await _canvas.ClearAsync(gameHash, result.Value.CanvasEpoch, cancellationToken);
        }
        return result.Value;
    }

}
