namespace Scribbly.Application.Features.Canvas.Configuration;

public sealed class CanvasOptions
{
    public const string SectionName = "Canvas";

    public int Width { get; set; }
    public int Height { get; set; }
    public int MaxPointsPerChunk { get; set; }
    public int MaxPointsPerStroke { get; set; }
    public int MaxStrokes { get; set; }
    public int MaxTotalPoints { get; set; }
    public int MaxStrokeIdLength { get; set; }
    public int MinThickness { get; set; }
    public int MaxThickness { get; set; }
    public int DefaultThickness { get; set; }
    public int ThicknessStep { get; set; }
    public int SampleIntervalMilliseconds { get; set; }
    public int FlushIntervalMilliseconds { get; set; }
}
