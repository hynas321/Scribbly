using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

public class CreateGameBody
{   
    [Required]
    [MinLength(5)]
    public string? HostUsername { get; set; }
}
