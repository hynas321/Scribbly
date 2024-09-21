namespace dotnet_server.Domain.Entities;

public class DrawnLine
{
    public Point CurrentPoint { get; set; } = new Point();
    public Point PreviousPoint { get; set; } = new Point();
    public string Color { get; set; } = string.Empty;
    public int Thickness { get; set; } = 5;
    public int CurrentLine { get; set; } = 0;
}