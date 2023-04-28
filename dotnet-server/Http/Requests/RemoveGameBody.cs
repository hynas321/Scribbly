using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

public class RemoveGameBody
{
    [Required]
    public string? GameHash { get; set; }

    [Required]
    public string? HostToken { get; set; }
}