using System.ComponentModel.DataAnnotations;

namespace WebApi.Api.Models.HttpRequest;

public class UsernameExistsBody
{
    [Required]
    [MinLength(1)]
    public string Username { get; set; }
}