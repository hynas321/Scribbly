using Scribbly.Application.Features.Canvas.Configuration;

namespace Scribbly.WebApi.UnitTests.TestDoubles;

public static class CanvasTestOptions
{
    public static CanvasOptions Create() => new()
    {
        Width = 670,
        Height = 470,
        MaxPointsPerChunk = 64,
        MaxPointsPerStroke = 4096,
        MaxStrokes = 128,
        MaxTotalPoints = 100_000,
        MaxStrokeIdLength = 64,
        MinThickness = 1,
        MaxThickness = 64,
        DefaultThickness = 5,
        ThicknessStep = 3,
        SampleIntervalMilliseconds = 25,
        FlushIntervalMilliseconds = 50
    };
}
