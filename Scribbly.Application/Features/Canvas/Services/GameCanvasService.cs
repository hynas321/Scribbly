using Scribbly.Application.Abstractions.Canvas;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Abstractions.Security;
using Scribbly.Application.Features.Canvas.Configuration;
using Scribbly.Application.Persistence;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Canvas;

public class GameCanvasService : IGameCanvasService
{
    private readonly IGameStateStore _games;
    private readonly ICanvasStateStore _canvas;
    private readonly IRoomCredentialService _credentials;
    private readonly CanvasOptions _options;

    public GameCanvasService(
        IGameStateStore games,
        ICanvasStateStore canvas,
        IRoomCredentialService credentials,
        CanvasOptions options)
    {
        _games = games;
        _canvas = canvas;
        _credentials = credentials;
        _options = options;
    }

    public async Task<CanvasCommandResult> AppendChunkAsync(
        string gameHash,
        string roomAccessToken,
        CanvasStrokeChunk chunk,
        CancellationToken cancellationToken = default)
    {
        if (!IsChunkValid(chunk))
        {
            return new(CanvasCommandStatus.InvalidStroke);
        }

        AuthorizationSnapshot auth = await AuthorizeAsync(gameHash, roomAccessToken, true, cancellationToken);

        if (auth.Status != CanvasCommandStatus.Applied)
        {
            return new(auth.Status, Epoch: auth.Epoch);
        }

        if (chunk.Epoch != auth.Epoch)
        {
            return new(CanvasCommandStatus.WrongEpoch, Epoch: auth.Epoch);
        }

        CanvasStoreAppendResult result = await _canvas.AppendChunkAsync(gameHash, chunk, cancellationToken);
        return new(result.Status switch
        {
            CanvasStoreAppendStatus.Applied => CanvasCommandStatus.Applied,
            CanvasStoreAppendStatus.Duplicate => CanvasCommandStatus.Duplicate,
            CanvasStoreAppendStatus.WrongEpoch => CanvasCommandStatus.WrongEpoch,
            CanvasStoreAppendStatus.OutOfOrder => CanvasCommandStatus.OutOfOrder,
            CanvasStoreAppendStatus.LimitExceeded => CanvasCommandStatus.LimitExceeded,
            _ => CanvasCommandStatus.InvalidStroke
        }, Epoch: auth.Epoch, Revision: result.Revision);
    }

    public async Task<CanvasCommandResult> ClearAsync(
        string gameHash,
        string roomAccessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roomAccessToken))
        {
            return new(CanvasCommandStatus.NotAuthorized);
        }

        string hash = _credentials.ComputeCredentialHash(roomAccessToken);

        GameStoreResult<long> result = await _games.MutateAsync(gameHash, game =>
        {
            CanvasCommandStatus status = Authorize(game, hash, true);

            if (status != CanvasCommandStatus.Applied)
            {
                return GameMutation<long>.NoChange(-(long)status);
            }

            game.GameState.CanvasEpoch++;
            return GameMutation<long>.Persist(game.GameState.CanvasEpoch);
        }, cancellationToken);

        if (!result.Found)
        {
            return new(CanvasCommandStatus.GameNotFound);
        }

        if (result.Value <= 0)
        {
            return new((CanvasCommandStatus)(-result.Value));
        }

        await _canvas.ClearAsync(gameHash, result.Value, cancellationToken);
        return new(CanvasCommandStatus.Applied, Epoch: result.Value, Revision: 0);
    }

    public async Task<CanvasCommandResult> UndoAsync(
        string gameHash,
        string roomAccessToken,
        CancellationToken cancellationToken = default)
    {
        AuthorizationSnapshot auth = await AuthorizeAsync(gameHash, roomAccessToken, true, cancellationToken);

        if (auth.Status != CanvasCommandStatus.Applied)
        {
            return new(auth.Status, Epoch: auth.Epoch);
        }

        CanvasStoreUndoResult result = await _canvas.UndoAsync(gameHash, auth.Epoch, cancellationToken);
        return result.Applied
            ? new(CanvasCommandStatus.Applied, result.RemovedStrokeId, auth.Epoch, result.Revision)
            : new(CanvasCommandStatus.NothingToUndo, Epoch: auth.Epoch, Revision: result.Revision);
    }

    public async Task<CanvasSnapshot?> GetSnapshotAsync(
        string gameHash,
        string roomAccessToken,
        CancellationToken cancellationToken = default)
    {
        AuthorizationSnapshot auth = await AuthorizeAsync(gameHash, roomAccessToken, false, cancellationToken);

        if (auth.Status != CanvasCommandStatus.Applied)
        {
            return null;
        }

        return await _canvas.GetSnapshotAsync(gameHash, auth.Epoch, cancellationToken);
    }

    private async Task<AuthorizationSnapshot> AuthorizeAsync(
        string gameHash,
        string roomAccessToken,
        bool requireDrawer,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(roomAccessToken))
        {
            return new(CanvasCommandStatus.NotAuthorized, 0);
        }

        string hash = _credentials.ComputeCredentialHash(roomAccessToken);

        Game? game = await _games.GetSnapshotAsync(gameHash, cancellationToken);

        if (game is null)
        {
            return new(CanvasCommandStatus.GameNotFound, 0);
        }

        return new(Authorize(game, hash, requireDrawer), game.GameState.CanvasEpoch);
    }

    private static CanvasCommandStatus Authorize(Game game, string hash, bool requireDrawer)
    {
        if (game.GameState.Players.All(player => player.CredentialHash != hash))
        {
            return CanvasCommandStatus.NotAuthorized;
        }

        if (requireDrawer && game.GameState.IsGameStarted && game.GameState.DisconnectedPlayers.Count > 0)
        {
            return CanvasCommandStatus.WaitingForPlayers;
        }

        if (requireDrawer && game.GameState.DrawingCredentialHash != hash)
        {
            return CanvasCommandStatus.NotAuthorized;
        }
        return CanvasCommandStatus.Applied;
    }

    private bool IsChunkValid(CanvasStrokeChunk chunk)
    {
        if (chunk.Epoch <= 0 || string.IsNullOrWhiteSpace(chunk.StrokeId) ||
            chunk.StrokeId.Length > _options.MaxStrokeIdLength || chunk.Sequence < 0)
        {
            return false;
        }

        if (chunk.Thickness < _options.MinThickness || chunk.Thickness > _options.MaxThickness ||
            chunk.Coordinates is null || chunk.Coordinates.Length == 0 || chunk.Coordinates.Length % 2 != 0)
        {
            return false;
        }

        if (chunk.Coordinates.Length / 2 > _options.MaxPointsPerChunk)
        {
            return false;
        }

        if (chunk.Color.Length != 7 || chunk.Color[0] != '#' || !chunk.Color.AsSpan(1).ToString().All(Uri.IsHexDigit))
        {
            return false;
        }
        return true;
    }

    private record AuthorizationSnapshot(CanvasCommandStatus Status, long Epoch);
}
