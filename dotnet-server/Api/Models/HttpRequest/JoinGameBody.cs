using System.ComponentModel.DataAnnotations;

namespace dotnet_server.Api.Models.HttpRequest;

public class JoinGameBody
{
    [Required]
    [MinLength(1)]
    [MaxLength(18)]
    public string Username { get; set; }
}