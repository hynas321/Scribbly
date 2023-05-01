
using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

public class SetSettingBody
{   
    [Required]
    public object Setting { get; set; }
}