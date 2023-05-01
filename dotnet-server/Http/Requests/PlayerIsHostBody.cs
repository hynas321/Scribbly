using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

public class PlayerIsHostBody
{
    [Required]
    public string GameHash { get; set; }

    [Required]
    public string Token { get; set; }
}