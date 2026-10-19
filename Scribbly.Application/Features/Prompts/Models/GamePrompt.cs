namespace Scribbly.Application.Features.Prompts;

public record GamePrompt(string Text, string Language, string Category, string Kind)
{
    public string HiddenHint => PromptHintMask.FullMask(Text);
}
