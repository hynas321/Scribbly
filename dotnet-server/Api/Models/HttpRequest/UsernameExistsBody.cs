using System.ComponentModel.DataAnnotations;

namespace dotnet_server.Api.Models.HttpRequest;

public class UsernameExistsBody
{
    [Required]
    [MinLength(1)]
    public string Username { get; set; }
}