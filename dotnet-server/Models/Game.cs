namespace Dotnet.Server.Models;

public class Game
{
    public string? Id { get; set; }
    public string? HostUsername { get; set; }
    public bool NonAbstractNounsOnly { get; set; }
    public int DrawingTimespanSeconds { get; set; }
    public int RoundsCount { get; set; }
}
