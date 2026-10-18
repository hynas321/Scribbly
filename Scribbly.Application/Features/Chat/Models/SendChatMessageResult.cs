using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Chat;

public record SendChatMessageResult(
    SendChatMessageStatus Status,
    ChatMessage? Message = null,
    string? Username = null,
    int AwardedScore = 0,
    IReadOnlyCollection<PlayerScore>? Scores = null,
    IReadOnlyCollection<string>? CorrectGuessPlayerUsernames = null);
