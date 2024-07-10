using System.ComponentModel.DataAnnotations;

namespace dotnet_server.Models.Http.Request;

public class WordBody
{
    [Required]
    public string Text { get; set; }

    [Required]
    public string Language { get; set; }
}