using System.ComponentModel.DataAnnotations;

namespace HttpRequests;

public class AddPlayerScoreRequestBody
{   
    [Required]
    [MinLength(5)]
    public string? Username { get; set; }

    [Required]
    public int Score { get; set; }
}