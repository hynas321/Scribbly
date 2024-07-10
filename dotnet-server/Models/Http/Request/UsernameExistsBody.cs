using System.ComponentModel.DataAnnotations;

namespace dotnet_server.Models.Http.Request;

public class UsernameExistsBody
{
    [Required]
    [MinLength(1)]
    public string Username { get; set; }
}