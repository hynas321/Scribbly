namespace Scribbly.Application.Features.Chat.Configuration;

public sealed class ChatOptions
{
    public const string SectionName = "Chat";

    public int MaxMessageCount { get; set; }
    public int MaxMessageLength { get; set; }
}
