using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

public class AddPlayerScoreBody
{   
    [Required]
    [MinLength(5)]
    public string Username { get; set; }

    [Required]
    public int Score { get; set; }
}
