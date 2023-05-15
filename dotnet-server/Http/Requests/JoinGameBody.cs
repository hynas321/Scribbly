using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

public class JoinGameBody
{
    [Required]
    [MinLength(1)]
    [MaxLength(18)]
    public string Username { get; set; }
}