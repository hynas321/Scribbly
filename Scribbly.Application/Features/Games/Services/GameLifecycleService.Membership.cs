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
private GameMutation<JoinCredentialStatus> IssueJoinCredential(
        Game game, string hash, string username, DateTimeOffset now, DateTimeOffset expires)
    {
        bool pruned = PrunePending(game, now);

        if (game.GameState.IsGameStarted)
        {
            return MutationFor(pruned, JoinCredentialStatus.GameAlreadyStarted);
        }
        bool usernameTaken = AllPlayers(game).Any(p => EqualsName(p.Username, username)) ||
            game.PendingJoinCredentials.Any(p => EqualsName(p.Username, username)) || EqualsName(game.GameState.HostPlayerUsername, username);

        if (usernameTaken)
        {
            return MutationFor(pruned, JoinCredentialStatus.DuplicateUsername);
        }

        if (ReservedSlots(game) >= Math.Min(game.GameSettings.MaxPlayers, _options.MaxPlayers))
        {
            return MutationFor(pruned, JoinCredentialStatus.RoomFull);
        }
        game.PendingJoinCredentials.Add(new()
        {
            CredentialHash = hash,
            Username = username,
            ExpiresAtUtc = expires
        });
        return GameMutation<JoinCredentialStatus>.Persist(JoinCredentialStatus.Issued);
    }

    private GameMutation<JoinGameResult> Join(Game game, string hash, string connectionId, DateTimeOffset now)
    {
        PrunePending(game, now);

        if (game.GameState.IsGameStarted)
        {
            return GameMutation<JoinGameResult>.NoChange(new(JoinGameStatus.GameAlreadyStarted));
        }

        if (game.GameState.KickedCredentialHashes.Contains(hash))
        {
            return GameMutation<JoinGameResult>.NoChange(new(JoinGameStatus.InvalidCredential));
        }

        if (AllPlayers(game).Any(p => p.CredentialHash == hash))
        {
            return GameMutation<JoinGameResult>.NoChange(new(JoinGameStatus.InvalidCredential));
        }

        bool host = hash == game.HostCredentialHash;
        PendingJoinCredential? pending = host ? null : game.PendingJoinCredentials.FirstOrDefault(p => p.CredentialHash == hash);

        if (!host && pending is null)
        {
            return GameMutation<JoinGameResult>.NoChange(new(JoinGameStatus.InvalidCredential));
        }
        string username = host ? game.GameState.HostPlayerUsername : pending!.Username;

        if (pending is not null)
        {
            game.PendingJoinCredentials.Remove(pending);
        }
        var player = new Player { Username = username, CredentialHash = hash, ConnectionId = connectionId };
        game.GameState.Players.Add(player);
        return GameMutation<JoinGameResult>.Persist(new(JoinGameStatus.Joined, BuildSnapshot(game, player)));
    }

    private GameMutation<JoinGameResult> Reconnect(
        Game game,
        string hash,
        string connectionId,
        DateTimeOffset now)
    {
        PrunePending(game, now);

        if (game.GameState.KickedCredentialHashes.Contains(hash))
        {
            return GameMutation<JoinGameResult>.NoChange(new(JoinGameStatus.InvalidCredential));
        }
        Player? active = game.GameState.Players.FirstOrDefault(p => p.CredentialHash == hash);

        if (active is not null)
        {
            active.ConnectionId = connectionId;
            active.DisconnectedAtUtc = null;
            return GameMutation<JoinGameResult>.Persist(new(JoinGameStatus.Resumed, BuildSnapshot(game, active)));
        }
        Player? disconnected = game.GameState.DisconnectedPlayers.FirstOrDefault(p => p.CredentialHash == hash);

        if (disconnected is null)
        {
            return GameMutation<JoinGameResult>.NoChange(new(JoinGameStatus.InvalidCredential));
        }

        if (disconnected.DisconnectedAtUtc is null || disconnected.DisconnectedAtUtc.Value + _options.ReconnectGracePeriod <= now)
        {
            game.GameState.DisconnectedPlayers.Remove(disconnected);

            if (disconnected.CredentialHash == game.HostCredentialHash)
            {
                PromoteNextInLineAsHost(game);
            }
            bool completionPending = HasPendingCompletion(game);
            bool empty = ReconnectableCount(game) == 0;
            var expired = new JoinGameResult(JoinGameStatus.ResumeExpired);
            return empty && !completionPending ? GameMutation<JoinGameResult>.Delete(expired) : GameMutation<JoinGameResult>.Persist(expired);
        }
        game.GameState.DisconnectedPlayers.Remove(disconnected);
        disconnected.ConnectionId = connectionId;
        disconnected.DisconnectedAtUtc = null;
        game.GameState.Players.Add(disconnected);
        return GameMutation<JoinGameResult>.Persist(new(JoinGameStatus.Resumed, BuildSnapshot(game, disconnected)));
    }

    private GameMutation<LeaveGameResult> Leave(Game game, string hash, DateTimeOffset now)
    {
        Player? player = game.GameState.Players.FirstOrDefault(p => p.CredentialHash == hash);

        if (player is null)
        {
            return GameMutation<LeaveGameResult>.NoChange(new(LeaveGameStatus.PlayerNotFound));
        }
        bool wasHost = hash == game.HostCredentialHash;
        game.GameState.Players.Remove(player);
        game.GameState.DisconnectedPlayers.RemoveAll(p =>
            p.DisconnectedAtUtc is null || p.DisconnectedAtUtc.Value + _options.ReconnectGracePeriod <= now);
        PrunePending(game, now);
        Player? promotedHost = wasHost ? PromoteNextInLineAsHost(game) : null;
        int reconnectable = ReconnectableCount(game);
        bool completionPending = HasPendingCompletion(game);
        bool notEnough = !completionPending && game.GameState.IsGameStarted && reconnectable < 2;
        bool empty = reconnectable == 0;

        if (notEnough)
        {
            game.GameState.IsGameStarted = false;
        }
        var value = new LeaveGameResult(LeaveGameStatus.Left, player.Username, ToScores(game), reconnectable,
            empty, false, notEnough, promotedHost?.Username);
        return (empty || notEnough) && !completionPending
            ? GameMutation<LeaveGameResult>.Delete(value)
            : GameMutation<LeaveGameResult>.Persist(value);
    }

    private static Player? PromoteNextInLineAsHost(Game game)
    {
        Player? successor = game.GameState.Players.FirstOrDefault() ?? game.GameState.DisconnectedPlayers.FirstOrDefault();

        if (successor is null)
        {
            return null;
        }
        game.HostCredentialHash = successor.CredentialHash;
        game.GameState.HostPlayerUsername = successor.Username;
        return successor;
    }

    private static GameMutation<DisconnectedPlayerResult> Disconnect(Game game, string connectionId, DateTimeOffset now)
    {
        Player? player = game.GameState.Players.FirstOrDefault(p => p.ConnectionId == connectionId);

        if (player is null)
        {
            return GameMutation<DisconnectedPlayerResult>.NoChange(new(null, game.GameHash, ToScores(game),
                ReconnectableCount(game), false));
        }
        game.GameState.Players.Remove(player);
        game.GameState.DisconnectedPlayers.RemoveAll(p => p.CredentialHash == player.CredentialHash);
        player.ConnectionId = string.Empty;
        player.DisconnectedAtUtc = now;
        game.GameState.DisconnectedPlayers.Add(player);
        return GameMutation<DisconnectedPlayerResult>.Persist(new(player.Username, game.GameHash, ToScores(game), ReconnectableCount(game), false));
    }

    private GameMutation<ExpiredDisconnectedPlayersResult> RemoveExpired(Game game, DateTimeOffset now)
    {
        bool pruned = PrunePending(game, now);
        DateTimeOffset cutoff = now - _options.ReconnectGracePeriod;
        Player[] expired = game.GameState.DisconnectedPlayers
            .Where(p => p.DisconnectedAtUtc is not null && p.DisconnectedAtUtc.Value <= cutoff).ToArray();

        if (expired.Length == 0)
        {
            return MutationFor(pruned, new ExpiredDisconnectedPlayersResult(
                game.GameHash, Array.Empty<string>(), ToScores(game), ReconnectableCount(game), false));
        }
        foreach (Player p in expired)
        {
            game.GameState.DisconnectedPlayers.Remove(p);
        }

        if (expired.Any(p => p.CredentialHash == game.HostCredentialHash))
        {
            PromoteNextInLineAsHost(game);
        }
        bool completionPending = HasPendingCompletion(game);
        int reconnectable = ReconnectableCount(game);
        bool notEnough = !completionPending && game.GameState.IsGameStarted && reconnectable < 2;

        if (notEnough)
        {
            game.GameState.IsGameStarted = false;
        }
        bool removed = (reconnectable == 0 || notEnough) && !completionPending;
        var value = new ExpiredDisconnectedPlayersResult(
            game.GameHash, expired.Select(p => p.Username).ToArray(), ToScores(game), reconnectable, removed, false, notEnough);
        return removed ? GameMutation<ExpiredDisconnectedPlayersResult>.Delete(value) : GameMutation<ExpiredDisconnectedPlayersResult>.Persist(value);
    }
}
