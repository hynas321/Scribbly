using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

public class LeaveGameBody
{
    [Required]
    public string? GameHash { get; set; }

    [Required]
    public string? Token { get; set; }
}