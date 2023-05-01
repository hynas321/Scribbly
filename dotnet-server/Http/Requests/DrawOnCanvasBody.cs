using System.ComponentModel.DataAnnotations;

namespace Dotnet.Server.Http.Requests;

public class DrawOnCanvasBody
{   
    [Required]
    public string DrawnLineSerialized { get; set; }
}
