public class DrawnLine
{
    public Point? CurrentRelativePoint { get; set; }
    public Point? PreviousRelativePoint { get; set; }
    public string? Color { get; set; }
}

public class Point
{
    public int X { get; set; }
    public int Y { get; set; }
}