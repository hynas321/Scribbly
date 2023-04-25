public class DrawnLine
{
    public Point? CurrentPoint { get; set; }
    public Point? PreviousPoint { get; set; }
    public string? Color { get; set; }
}

public class Point
{
    public double X { get; set; }
    public double Y { get; set; }
}