using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Models;

public class WordBody
{
    [Required]
    public string Text { get; set; }

    [Required]
    public string Language { get; set; }
}