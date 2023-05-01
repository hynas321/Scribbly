using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

public class SendChatMessageBody
{
    [Required]
    [MinLength(1)]
    public string Text { get; set; }
}