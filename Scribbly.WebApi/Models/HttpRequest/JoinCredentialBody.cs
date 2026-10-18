using System.ComponentModel.DataAnnotations;

namespace Scribbly.WebApi.Models.HttpRequest;

public class JoinCredentialBody
{
    [Required, StringLength(18, MinimumLength = 1)]
    public string Username { get; set; } = string.Empty;
}
