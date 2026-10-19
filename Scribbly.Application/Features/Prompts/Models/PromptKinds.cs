namespace Scribbly.Application.Features.Prompts;

public static class PromptKinds
{
    public const string Word = "word";
    public const string Phrase = "phrase";
    public const string Saying = "saying";
    public static bool IsSupported(string? value) => value is Word or Phrase or Saying;
}
