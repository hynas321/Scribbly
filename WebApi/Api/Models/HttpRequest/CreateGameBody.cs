using System.ComponentModel.DataAnnotations;

namespace WebApi.Api.Models.HttpRequest;

public class CreateGameBody
{
    [Required]
    [MinLength(1)]
    [MaxLength(18)]
    public string Username { get; set; }
}