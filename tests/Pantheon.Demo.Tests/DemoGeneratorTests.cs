using System.Text.Json;
using Pantheon.Demo;
using Xunit;

namespace Pantheon.Demo.Tests;

public sealed class DemoGeneratorTests
{
    [Fact]
    public void GeneratesCoreWorkframeEndpointsAndCalculatedDimensions()
    {
        var response = Generate(new DemoRequest(60, 120, 18, 4, 4));

        Assert.True(response.Ok);
        Assert.Null(response.Error);
        Assert.Equal("inches", response.Unit);
        Assert.NotNull(response.Dimensions);
        Assert.Equal(720, response.Dimensions.Span);
        Assert.Equal(1_440, response.Dimensions.Length);
        Assert.Equal(216, response.Dimensions.Eave);
        Assert.Equal(336, response.Dimensions.Ridge);
        Assert.Equal(5, response.Dimensions.FrameCount);
        Assert.Equal(360, response.Dimensions.BaySize);
        Assert.Contains(response.Lines, line =>
            line.Kind == "frame" &&
            line.Start == new DemoPoint(0, 720, 216) &&
            line.End == new DemoPoint(0, 360, 336));
    }

    [Theory]
    [InlineData(19, 120, 18, 4, 4, "Span must be from 20 to 200 feet.")]
    [InlineData(60, 120, 18, 4, 0, "Bays must be a whole number from 1 to 20.")]
    public void RefusesDimensionsOutsideTheDemoScope(
        double span,
        double length,
        double eave,
        double pitch,
        int bays,
        string message)
    {
        var response = Generate(new DemoRequest(span, length, eave, pitch, bays));

        Assert.False(response.Ok);
        Assert.Equal(message, response.Error);
        Assert.Null(response.Dimensions);
        Assert.Empty(response.Lines);
    }

    private static DemoResponse Generate(DemoRequest request)
    {
        var requestJson = JsonSerializer.Serialize(request, DemoJsonContext.Default.DemoRequest);
        var responseJson = DemoGenerator.GenerateJson(requestJson);
        return JsonSerializer.Deserialize(responseJson, DemoJsonContext.Default.DemoResponse)!;
    }
}
