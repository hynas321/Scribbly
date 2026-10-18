using System.ComponentModel.DataAnnotations;

namespace Scribbly.WebApi.Models.HttpRequest;

public class CreateGameBody
{
    [Required]
    [MinLength(1)]
    [MaxLength(18)]
    public string? Username { get; set; }
}
