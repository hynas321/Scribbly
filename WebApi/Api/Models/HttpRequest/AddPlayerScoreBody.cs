using System.ComponentModel.DataAnnotations;

namespace WebApi.Api.Models.HttpRequest;

public class AddPlayerScoreBody
{
    [Required]
    [MinLength(1)]
    public string Username { get; set; }

    [Required]
    public int Score { get; set; }
}
