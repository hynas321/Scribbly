using System.ComponentModel.DataAnnotations;
using Dotnet.Server.Models;

namespace Dotnet.Server.Http.Requests;

public class AddAccountBody
{   
    [Required]
    public Account Account { get; set; }
}
