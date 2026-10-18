using Scribbly.Application.Abstractions.Canvas;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Abstractions.Security;
using Scribbly.Application.Persistence;
using Scribbly.Application.Features.Settings;
using Scribbly.Application.Features.Settings.Configuration;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Games;

public partial class GameLifecycleService
{
private static GameMutation<KickPlayerResult> Kick(Game game, string hostHash, string username)
    {
        if (game.HostCredentialHash != hostHash || game.GameState.Players.All(p => p.CredentialHash != hostHash))
        {
            return GameMutation<KickPlayerResult>.NoChange(new(KickPlayerStatus.Unauthorized));
        }
        Player? target = game.GameState.Players.FirstOrDefault(p => EqualsName(p.Username, username))
            ?? game.GameState.DisconnectedPlayers.FirstOrDefault(p => EqualsName(p.Username, username));

        if (target is null)
        {
            return GameMutation<KickPlayerResult>.NoChange(new(KickPlayerStatus.PlayerNotFound));
        }

        if (target.CredentialHash == hostHash)
        {
            return GameMutation<KickPlayerResult>.NoChange(new(KickPlayerStatus.CannotKickHost));
        }
        game.GameState.Players.Remove(target);
        game.GameState.DisconnectedPlayers.Remove(target);
        game.GameState.KickedCredentialHashes.Add(target.CredentialHash);
        game.PendingJoinCredentials.RemoveAll(p => p.CredentialHash == target.CredentialHash);
        game.GameState.DrawingPlayerCredentialHashes.RemoveAll(fp => fp == target.CredentialHash);
        long canvasEpoch = 0;

        if (game.GameState.DrawingCredentialHash == target.CredentialHash)
        {
            game.GameState.DrawingCredentialHash = string.Empty;
            game.GameState.DrawingPlayerUsername = string.Empty;
            game.GameState.PendingDrawingCredentialHash = string.Empty;
            game.GameState.CanvasEpoch++;
            canvasEpoch = game.GameState.CanvasEpoch;
        }
        else if (game.GameState.PendingDrawingCredentialHash == target.CredentialHash)
        {
            game.GameState.PendingDrawingCredentialHash = string.Empty;
        }
        bool gameFinished = game.GameState.IsGameStarted && ReconnectableCount(game) == 1;

        if (gameFinished)
        {
            game.GameState.IsGameStarted = false;
            game.GameState.IsTimerVisible = false;
            game.GameState.DrawingCredentialHash = string.Empty;
            game.GameState.PendingDrawingCredentialHash = string.Empty;
            game.GameState.DrawingPlayerUsername = string.Empty;
            game.GameState.CurrentDrawingTimeSeconds = 0;
            game.GameState.CurrentRound = game.GameSettings.RoundsCount + 1;
            game.FinalScores = AllPlayers(game)
                .Select(player => new FinalPlayerScore(
                    player.CredentialHash,
                    player.Score,
                    player.Username))
                .ToList();
        }
        return GameMutation<KickPlayerResult>.Persist(new(
            KickPlayerStatus.Applied,
            target.Username,
            canvasEpoch,
            gameFinished));
    }

    public async Task<DisconnectedPlayerResult> RemoveByConnectionIdAsync(
        string connectionId,
        CancellationToken cancellationToken = default)
    {
        string? gameHash = await _connectionIndex.FindGameHashAsync(connectionId, cancellationToken);

        if (gameHash is null)
        {
            return EmptyDisconnected();
        }
        GameStoreResult<DisconnectedPlayerResult> result = await _store.MutateAsync(gameHash,
            game => Disconnect(game, connectionId, DateTimeOffset.UtcNow), cancellationToken);

        await _connectionIndex.RemoveAsync(connectionId, gameHash, cancellationToken);
        return result.Found ? result.Value! : EmptyDisconnected();
    }

    public async Task<ExpiredDisconnectedPlayersResult> RemoveExpiredDisconnectedPlayersAsync(
        string gameHash, DateTimeOffset nowUtc, CancellationToken cancellationToken = default)
    {
        GameStoreResult<ExpiredDisconnectedPlayersResult> result = await _store.MutateAsync(gameHash,
            game => RemoveExpired(game, nowUtc), cancellationToken);

        if (!result.Found)
        {
            return EmptyExpired(gameHash);
        }

        if (result.Value!.GameRemoved)
        {
            await _canvas.DeleteAsync(gameHash, cancellationToken);
        }
        return result.Value;
    }
}
