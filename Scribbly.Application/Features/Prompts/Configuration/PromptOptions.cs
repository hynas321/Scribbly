namespace Scribbly.Application.Features.Prompts.Configuration;

public sealed class PromptOptions
{
    public const string SectionName = "Prompt";

    public int MaxTextLength { get; set; }
    public int MaxCategoryLength { get; set; }
}
