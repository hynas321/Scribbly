using System.ComponentModel.DataAnnotations;
using WebApi.Domain.Entities;

namespace WebApi.Api.Models.HttpRequest;

public class AddAccountBody
{
    [Required]
    public Account Account { get; set; }
}
