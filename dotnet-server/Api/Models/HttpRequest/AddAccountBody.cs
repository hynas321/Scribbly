using System.ComponentModel.DataAnnotations;
using dotnet_server.Domain.Entities;

namespace dotnet_server.Api.Models.HttpRequest;

public class AddAccountBody
{
    [Required]
    public Account Account { get; set; }
}
