using Scribbly.Application.Features.Chat.Configuration;

namespace Scribbly.WebApi.UnitTests.TestDoubles;

public static class ChatTestOptions
{
    public static ChatOptions Create() => new()
    {
        MaxMessageCount = 25,
        MaxMessageLength = 280
    };
}
