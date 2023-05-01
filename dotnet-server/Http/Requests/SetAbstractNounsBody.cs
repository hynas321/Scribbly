
using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

public class SetSettingBody
{   
    [Required]
    public string GameHash { get; set; }

    [Required]
    public string Token { get; set; }

    [Required]
    public object Setting { get; set; }
}