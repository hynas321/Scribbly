using Scribbly.Application.Features.Prompts.Configuration;

namespace Scribbly.WebApi.UnitTests.TestDoubles;

public static class PromptTestOptions
{
    public static PromptOptions Create() => new()
    {
        MaxTextLength = 120,
        MaxCategoryLength = 64
    };
}
