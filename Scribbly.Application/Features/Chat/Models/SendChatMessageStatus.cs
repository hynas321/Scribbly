namespace Scribbly.Application.Features.Chat;

public enum SendChatMessageStatus
{
    Sent,
    CorrectGuess,
    GameNotFound,
    PlayerNotFound,
    EmptyText,
    MessageTooLong,
    WaitingForPlayers,
    ChatForbidden,
    DrawingPlayerCannotChat
}
