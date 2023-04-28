using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

public class GameExistsBody 
{
    [Required]
    public string? GameHash { get; set; }
}