using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

public class CreateGameBody
{
    [Required]
    [MinLength(1)]
    [MaxLength(18)]
    public string Username { get; set; }
}