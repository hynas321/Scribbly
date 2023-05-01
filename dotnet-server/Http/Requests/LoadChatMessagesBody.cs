using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

class LoadChatMessagesBody
{
    [Required]
    public string GameHash { get; set; }

    [Required]
    public string Token { get; set; }
}