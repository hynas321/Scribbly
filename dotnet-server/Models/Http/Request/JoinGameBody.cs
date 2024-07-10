using System.ComponentModel.DataAnnotations;

namespace dotnet_server.Models.Http.Request;

public class JoinGameBody
{
    [Required]
    [MinLength(1)]
    [MaxLength(18)]
    public string Username { get; set; }
}