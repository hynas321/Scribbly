using Scribbly.Application.Features.Prompts.Configuration;

namespace Scribbly.Application.Features.Prompts;

public static class GamePromptPolicy
{
    public static bool IsValid(GamePrompt prompt, PromptOptions options)
    {
        string text = prompt.Text.Trim();
        string category = prompt.Category.Trim();

        if (text.Length < 1 || text.Length > options.MaxTextLength ||
            category.Length < 1 || category.Length > options.MaxCategoryLength)
        {
            return false;
        }

        if (!PromptKinds.IsSupported(prompt.Kind) || prompt.Language is not ("en" or "pl"))
        {
            return false;
        }

        bool hasWhitespace = text.Any(char.IsWhiteSpace);
        return prompt.Kind == PromptKinds.Word ? !hasWhitespace : hasWhitespace;
    }
}
