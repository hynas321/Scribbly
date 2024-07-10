using System.ComponentModel.DataAnnotations;
using Dotnet.Server.Models;

namespace dotnet_server.Models.Http.Request;

public class AddAccountBody
{
    [Required]
    public Account Account { get; set; }
}
