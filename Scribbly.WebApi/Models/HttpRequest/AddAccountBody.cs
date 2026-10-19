using System.ComponentModel.DataAnnotations;

namespace Scribbly.WebApi.Models.HttpRequest;

public class AddAccountBody
{
    [Required, StringLength(16384, MinimumLength = 1)]
    public string IdToken { get; set; } = string.Empty;
}
