using System.ComponentModel.DataAnnotations;

namespace dotnet_server.Api.Models.HttpRequest;

public class WordBody
{
    [Required]
    public string Text { get; set; }

    [Required]
    public string Language { get; set; }
}