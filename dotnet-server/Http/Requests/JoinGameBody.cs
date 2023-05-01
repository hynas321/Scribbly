using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

public class JoinGameBody
{
    [Required]
    public string GameHash { get; set; }

    [Required]
    [MinLength(5)]
    public string Username { get; set; }
}