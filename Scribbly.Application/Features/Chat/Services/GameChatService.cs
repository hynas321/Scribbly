using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Abstractions.Security;
using Scribbly.Application.Features.Chat.Configuration;
using Scribbly.Application.Persistence;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Chat;

public class GameChatService : IGameChatService
{
    private readonly IGameStateStore _store;
    private readonly IRoomCredentialService _credentials;
    private readonly ChatOptions _options;

    public GameChatService(
        IGameStateStore store,
        IRoomCredentialService credentials,
        ChatOptions options)
    {
        _store = store;
        _credentials = credentials;
        _options = options;
    }

    public async Task<SendChatMessageResult> SendAsync(
        string gameHash,
        string roomAccessToken,
        string text,
        CancellationToken cancellationToken = default)
    {
        string normalized = (text ?? string.Empty).Trim();

        if (normalized.Length == 0)
        {
            return new(SendChatMessageStatus.EmptyText);
        }

        if (normalized.Length > _options.MaxMessageLength)
        {
            return new(SendChatMessageStatus.MessageTooLong);
        }

        if (string.IsNullOrWhiteSpace(roomAccessToken))
        {
            return new(SendChatMessageStatus.PlayerNotFound);
        }
        string hash = _credentials.ComputeCredentialHash(roomAccessToken);

        GameStoreResult<SendChatMessageResult> result = await _store.MutateAsync(gameHash,
            game => Send(game, hash, normalized), cancellationToken);
        return result.Found ? result.Value! : new(SendChatMessageStatus.GameNotFound);
    }

    public async Task<IReadOnlyCollection<ChatMessage>?> GetMessagesAsync(
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

        if (game is null || game.GameState.Players.All(p => p.CredentialHash != hash))
        {
            return null;
        }
        return game.ChatMessages.Select(Copy).ToArray();
    }

    private GameMutation<SendChatMessageResult> Send(Game game, string hash, string text)
    {
        Player? player = game.GameState.Players.FirstOrDefault(p => p.CredentialHash == hash);

        if (player is null)
        {
            return GameMutation<SendChatMessageResult>.NoChange(new(SendChatMessageStatus.PlayerNotFound));
        }

        if (game.GameState.IsGameStarted && game.GameState.DisconnectedPlayers.Count > 0)
        {
            return GameMutation<SendChatMessageResult>.NoChange(new(SendChatMessageStatus.WaitingForPlayers));
        }

        if (game.GameState.NoChatPermissionCredentialHashes.Contains(hash))
        {
            return GameMutation<SendChatMessageResult>.NoChange(new(SendChatMessageStatus.ChatForbidden));
        }

        if (hash == game.GameState.DrawingCredentialHash)
        {
            return GameMutation<SendChatMessageResult>.NoChange(new(SendChatMessageStatus.DrawingPlayerCannotChat));
        }

        bool correct = game.GameState.DrawingCredentialHash.Length > 0 &&
                       string.Equals(text, game.GameState.ActualPromptText.Trim(), StringComparison.OrdinalIgnoreCase);

        if (correct)
        {
            game.GameState.NoChatPermissionCredentialHashes.Add(hash);
            game.GameState.CorrectGuessPlayerUsernames.Add(player.Username);
            int score = Score(game.GameSettings.DrawingTimeSeconds, game.GameState.CurrentDrawingTimeSeconds);
            player.Score += score;
            game.GameState.Players = game.GameState.Players.OrderByDescending(p => p.Score).ToList();
            game.GameState.CorrectAnswerCount++;
            return GameMutation<SendChatMessageResult>.Persist(new(
                SendChatMessageStatus.CorrectGuess,
                Username: player.Username,
                AwardedScore: score,
                Scores: game.GameState.Players.Select(p => new PlayerScore { Username = p.Username, Score = p.Score }).ToArray(),
                CorrectGuessPlayerUsernames: game.GameState.CorrectGuessPlayerUsernames.ToArray()));
        }

        var message = new ChatMessage { Username = player.Username, Text = text };

        if (game.ChatMessages.Count >= _options.MaxMessageCount)
        {
            game.ChatMessages.RemoveAt(0);
        }
        game.ChatMessages.Add(message);
        return GameMutation<SendChatMessageResult>.Persist(new(SendChatMessageStatus.Sent, Copy(message), player.Username));
    }

    private static int Score(int totalSeconds, int remainingSeconds)
    {
        if (totalSeconds <= 0)
        {
            return 2;
        }
        double percentage = Math.Clamp((double)remainingSeconds / totalSeconds * 100.0, 0, 100);

        if (percentage > 80)
        {
            return 10;
        }

        if (percentage > 70)
        {
            return 9;
        }

        if (percentage > 60)
        {
            return 8;
        }

        if (percentage > 50)
        {
            return 7;
        }

        if (percentage > 40)
        {
            return 6;
        }

        if (percentage > 30)
        {
            return 5;
        }

        if (percentage > 20)
        {
            return 4;
        }

        if (percentage > 10)
        {
            return 3;
        }
        return 2;
    }

    private static ChatMessage Copy(ChatMessage message) => new()
    {
        Username = message.Username,
        Text = message.Text,
        BootstrapBackgroundColor = message.BootstrapBackgroundColor
    };
}
