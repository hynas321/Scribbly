using System.ComponentModel.DataAnnotations;

namespace dotnet_server.Models.Http.Request;

public class AddPlayerScoreBody
{
    [Required]
    [MinLength(1)]
    public string Username { get; set; }

    [Required]
    public int Score { get; set; }
}
