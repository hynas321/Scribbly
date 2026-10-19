using System.ComponentModel.DataAnnotations;

namespace Scribbly.WebApi.Models.HttpRequest;

public class PromptBody
{
    [Required, StringLength(120, MinimumLength = 1)]
    public string Text { get; set; } = string.Empty;
    [Required, StringLength(5, MinimumLength = 2)]
    public string Language { get; set; } = string.Empty;
    [Required, StringLength(64, MinimumLength = 1)]
    public string Category { get; set; } = "general";
    [Required, StringLength(16, MinimumLength = 4)]
    public string Kind { get; set; } = "word";
}
