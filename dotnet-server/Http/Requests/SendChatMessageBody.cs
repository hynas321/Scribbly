using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

public class SendChatMessageBody
{
    [Required]
    public string GameHash { get; set; }

    [Required]
    public string Token { get; set; }

    [Required]
    [MinLength(1)]
    public string Text { get; set; }
}