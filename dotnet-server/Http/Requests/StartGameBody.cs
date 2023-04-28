using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;
public class StartGameBody
{   
    [Required]
    [MinLength(5)]
    public string? HostUsername { get; set; }

    [Required]
    public bool NonAbstractNounsOnly { get; set; }

    [Required]
    [Range(30, 120)]
    public int DrawingTimeSeconds { get; set; }

    [Required]
    [Range(1, 6)]
    public int RoundsCount { get; set; }

    [Required]
    public string? WordLanguage { get; set; }
}