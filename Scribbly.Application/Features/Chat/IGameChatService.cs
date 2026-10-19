using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Chat;

public interface IGameChatService
{
    Task<SendChatMessageResult> SendAsync(
        string gameHash,
        string roomAccessToken,
        string text,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ChatMessage>?> GetMessagesAsync(
        string gameHash,
        string roomAccessToken,
        CancellationToken cancellationToken = default);
}
