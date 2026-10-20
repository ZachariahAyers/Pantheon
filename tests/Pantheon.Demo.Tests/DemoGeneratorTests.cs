using System.Text.Json;
using Pantheon.Demo;
using Xunit;

namespace Pantheon.Demo.Tests;

public sealed class DemoGeneratorTests
{
    private static readonly DemoRequest Gable = new("gable", 60, 120, 18, 4, 4);

    [Fact]
    public void GableReadsTheRidgeAndRafterFromTheSpanAndPitch()
    {
        var response = Generate(Gable);

        Assert.True(response.Ok);
        Assert.Null(response.Error);
        Assert.Equal("inches", response.Unit);
        // Ridge: 18 ft + 30 ft x 4/12 = 28 ft. Rafter: hypotenuse of 360 in by 120 in = 379.47 in.
        Assert.Equal(
            [
                new DemoReadout("Span", "60′ 0″"),
                new DemoReadout("Length", "120′ 0″"),
                new DemoReadout("Eave", "18′ 0″"),
                new DemoReadout("Ridge", "28′ 0″"),
                new DemoReadout("Roof slope", "31′ 7 1/2″ · 4:12"),
                new DemoReadout("Bay spacing", "30′ 0″"),
            ],
            response.Readouts);
        Assert.Equal(new DemoSummary(5, 4, response.Lines.Length), response.Summary);
        Assert.Contains(response.Lines, line =>
            line.Kind == "frame" &&
            line.Start == new DemoPoint(0, 720, 216) &&
            line.End == new DemoPoint(0, 360, 336));
        Assert.NotEmpty(response.Bubbles);
    }

    [Fact]
    public void SingleSlopeRunsFromTheLowEaveToTheHighEave()
    {
        var response = Generate(new DemoRequest("single", 40, 100, 14, 0, 5, HighEaveFeet: 18));

        Assert.True(response.Ok);
        // Pitch: 48 in of rise over 480 in of run = 1.2:12. Rafter: hypotenuse of 480 by 48 = 482.39 in.
        Assert.Equal(new DemoReadout("Low eave", "14′ 0″"), response.Readouts[2]);
        Assert.Equal(new DemoReadout("High eave", "18′ 0″"), response.Readouts[3]);
        Assert.Equal(new DemoReadout("Roof slope", "40′ 2 3/8″ · 1.2:12"), response.Readouts[4]);
        Assert.Contains(response.Lines, line =>
            line.Kind == "frame" &&
            line.Start == new DemoPoint(0, 480, 168) &&
            line.End == new DemoPoint(0, 0, 216));
    }

    [Fact]
    public void OffCenterRidgeSetsTheLeftPitchSoBothRaftersMeet()
    {
        var response = Generate(new DemoRequest("offset", 60, 120, 16, 3, 4, RidgeFeet: 40));

        Assert.True(response.Ok);
        // Right: 40 ft at 3:12 rises 10 ft, so the ridge is 26 ft. Left: 20 ft rising 10 ft is 6:12.
        Assert.Equal(
            [
                new DemoReadout("Span", "60′ 0″"),
                new DemoReadout("Eave", "16′ 0″"),
                new DemoReadout("Ridge", "26′ 0″"),
                new DemoReadout("Right slope", "41′ 2 3/4″ · 3:12"),
                new DemoReadout("Left slope", "22′ 4 5/16″ · 6:12"),
                new DemoReadout("Bay spacing", "30′ 0″"),
            ],
            response.Readouts);
        // At the ridge meet the two rafters and the interior column both sides share.
        var ridgePoint = new DemoPoint(0, 480, 312);
        Assert.Equal(3, response.Lines.Count(line =>
            line.Kind == "frame" && (line.Start == ridgePoint || line.End == ridgePoint)));
    }

    [Theory]
    [InlineData("gable", 19, 18, 4, 4, 0, 0, "Span must be from 20 to 200 feet.")]
    [InlineData("gable", 60, 18, 4, 0, 0, 0, "Bays must be a whole number from 1 to 20.")]
    [InlineData("single", 60, 18, 0, 4, 12, 0, "High eave must be from 18 to 60 feet.")]
    [InlineData("offset", 60, 18, 4, 4, 0, 60, "Ridge from right eave must be from 1 to 59 feet.")]
    [InlineData("offset", 40, 18, 12, 4, 0, 38,
        "The left side would be steeper than 12:12. Move the ridge toward the center or lower the pitch.")]
    [InlineData("mansard", 60, 18, 4, 4, 0, 0, "Choose a gable, single slope or off-center ridge roof.")]
    public void RefusesDimensionsOutsideTheDemoScope(
        string roof,
        double span,
        double eave,
        double pitch,
        int bays,
        double highEave,
        double ridge,
        string message)
    {
        var request = new DemoRequest(roof, span, 120, eave, pitch, bays, highEave, ridge);
        var response = Generate(request);

        Assert.False(response.Ok);
        Assert.Equal(message, response.Error);
        Assert.Null(response.Summary);
        Assert.Empty(response.Readouts);
        Assert.Empty(response.Lines);
        Assert.Equal("", DemoGenerator.GenerateDxf(Json(request)));
    }

    [Theory]
    [InlineData(0, "0′ 0″")]
    [InlineData(7.5, "0′ 7 1/2″")]
    [InlineData(12.0625, "1′ 0 1/16″")]
    [InlineData(191.99, "16′ 0″")]
    public void WritesFeetAndInchesToTheNearestSixteenth(double inches, string expected)
    {
        Assert.Equal(expected, DemoGenerator.FeetAndInches(inches));
    }

    [Fact]
    public void DxfHoldsEveryLineAndBubbleAsWellFormedGroupPairs()
    {
        var response = Generate(Gable);
        var dxf = DemoGenerator.GenerateDxf(Json(Gable));

        Assert.StartsWith("0\r\nSECTION\r\n2\r\nHEADER\r\n9\r\n$ACADVER\r\n1\r\nAC1009\r\n", dxf);
        Assert.EndsWith("0\r\nENDSEC\r\n0\r\nEOF\r\n", dxf);
        var tokens = dxf.Split("\r\n")[..^1];
        Assert.Equal(0, tokens.Length % 2);
        for (var i = 0; i < tokens.Length; i += 2)
        {
            Assert.True(int.TryParse(tokens[i], out _), $"Group code expected at line {i + 1}: {tokens[i]}");
        }

        var entities = Enumerable.Range(0, tokens.Length / 2)
            .Where(pair => tokens[pair * 2] == "0")
            .Select(pair => tokens[(pair * 2) + 1])
            .ToList();
        Assert.Equal(response.Lines.Length, entities.Count(name => name == "LINE"));
        Assert.Equal(response.Bubbles.Length, entities.Count(name => name == "CIRCLE"));
        Assert.Equal(response.Bubbles.Length, entities.Count(name => name == "TEXT"));
        Assert.Equal(4, entities.Count(name => name == "LAYER"));
        Assert.Contains("2\r\nPANTHEON-FRAME\r\n70\r\n0\r\n62\r\n3\r\n6\r\nCONTINUOUS\r\n", dxf);
    }

    private static string Json(DemoRequest request) =>
        JsonSerializer.Serialize(request, DemoJsonContext.Default.DemoRequest);

    private static DemoResponse Generate(DemoRequest request) =>
        JsonSerializer.Deserialize(DemoGenerator.GenerateJson(Json(request)), DemoJsonContext.Default.DemoResponse)!;
}
