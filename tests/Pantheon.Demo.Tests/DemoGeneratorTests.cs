using System.Text.Json;
using Pantheon.Demo;
using Xunit;

namespace Pantheon.Demo.Tests;

public sealed class DemoGeneratorTests
{
    private static readonly WorkframeDialog Symmetrical =
        new("symmetrical", Width: "60'", Eave: "18'", Length: "120'", Pitch: "4", SideBays: "4*30'");

    [Fact]
    public void SymmetricalDialogBuildsTheGableTheAddInWould()
    {
        var response = Generate(Symmetrical);

        Assert.True(response.Ok);
        // Ridge: 18 ft + 30 ft x 4/12 = 28 ft. Rafter: hypotenuse of 360 in by 120 in = 379.47 in.
        Assert.Equal(
            [
                new DemoReadout("Width", "60′ 0″"),
                new DemoReadout("Length", "120′ 0″"),
                new DemoReadout("Eave", "18′ 0″"),
                new DemoReadout("Ridge", "28′ 0″"),
                new DemoReadout("Rafter", "31′ 7 1/2″ · 4:12"),
                new DemoReadout("Bays", "4 · 30′ 0″ max"),
            ],
            response.Readouts);
        Assert.Contains(response.Lines, line =>
            line.Kind == "frame" &&
            line.Start == new DemoPoint(0, 720, 216) &&
            line.End == new DemoPoint(0, 360, 336));
        Assert.NotEmpty(response.Bubbles);
    }

    [Fact]
    public void SingleSlopeDialogRunsFromTheLowEaveToTheHighEave()
    {
        var response = Generate(new WorkframeDialog("singleslope", Width: "40'", Length: "100'", LowEave: "14'", HighEave: "18'", SideBays: "5*20'"));

        Assert.True(response.Ok);
        // Pitch: 48 in of rise over 480 in of run = 1.2:12. Rafter: hypotenuse of 480 by 48 = 482.39 in.
        Assert.Equal(new DemoReadout("Rafter", "40′ 2 3/8″ · 1.2:12"), response.Readouts[4]);
        Assert.Contains(response.Lines, line =>
            line.Kind == "frame" &&
            line.Start == new DemoPoint(0, 480, 168) &&
            line.End == new DemoPoint(0, 0, 216));
    }

    [Fact]
    public void OffCenterDialogTakesEachSideOnItsOwn()
    {
        var response = Generate(new WorkframeDialog(
            "offcenter",
            SideBays: "4*30'",
            Left: new DialogSide("20'", "16'", "120'", "6"),
            Right: new DialogSide("40'", "16'", "120'", "3")));

        Assert.True(response.Ok);
        // Left: 20 ft at 6:12 rises 10 ft; right: 40 ft at 3:12 rises 10 ft, so both ridges are 26 ft.
        Assert.Equal(
            [
                new DemoReadout("Width", "60′ 0″"),
                new DemoReadout("Left ridge", "26′ 0″"),
                new DemoReadout("Right ridge", "26′ 0″"),
                new DemoReadout("Left rafter", "22′ 4 5/16″ · 6:12"),
                new DemoReadout("Right rafter", "41′ 2 3/4″ · 3:12"),
                new DemoReadout("Bays", "4 · 30′ 0″ max"),
            ],
            response.Readouts);
        // At the ridge meet the two rafters and the interior column both sides share.
        var ridgePoint = new DemoPoint(0, 480, 312);
        Assert.Equal(3, response.Lines.Count(line =>
            line.Kind == "frame" && (line.Start == ridgePoint || line.End == ridgePoint)));
    }

    [Theory]
    [InlineData("symmetrical", "forty", "4", "4*30'", "Width: \"forty\" is not a length. Write it like 40', 25'6\" or 480.")]
    [InlineData("symmetrical", "60'", "13", "4*30'", "Pitch must be a rise per 12 from 0 to 12, for example 1 for 1:12.")]
    [InlineData("symmetrical", "60'", "4", "x*30'", "Sidewall bays: \"x*30'\" needs a whole number before the *, for example 3*25'.")]
    [InlineData("mansard", "60'", "4", "4*30'", "Choose Symmetrical, Single Slope or Off-Center Ridge.")]
    public void RefusesEntriesTheDialogWouldRefuse(string roof, string width, string pitch, string sideBays, string message)
    {
        var dialog = new WorkframeDialog(roof, Width: width, Eave: "18'", Length: "120'", Pitch: pitch, SideBays: sideBays);
        var response = Generate(dialog);

        Assert.False(response.Ok);
        Assert.Equal(message, response.Error);
        Assert.Empty(response.Lines);
        Assert.Equal("", DemoGenerator.GenerateDxf(Json(dialog)));
    }

    [Fact]
    public void BaysLongerThanTheBuildingAreRefused()
    {
        var response = Generate(Symmetrical with { SideBays = "5*30'" });

        Assert.False(response.Ok);
        Assert.False(string.IsNullOrEmpty(response.Error));
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
        var response = Generate(Symmetrical);
        var dxf = DemoGenerator.GenerateDxf(Json(Symmetrical));

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

    private static string Json(WorkframeDialog dialog) =>
        JsonSerializer.Serialize(dialog, DemoJsonContext.Default.WorkframeDialog);

    private static DemoResponse Generate(WorkframeDialog dialog) =>
        JsonSerializer.Deserialize(DemoGenerator.GenerateJson(Json(dialog)), DemoJsonContext.Default.DemoResponse)!;
}
