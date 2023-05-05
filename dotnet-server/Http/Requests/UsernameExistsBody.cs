using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

public class UsernameExistsBody
{
    [Required]
    [MinLength(1)]
    public string Username { get; set; }
}