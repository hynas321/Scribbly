using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;
public class StartGameBody
{   
    [Required]
    public string Token { get; set; }
}