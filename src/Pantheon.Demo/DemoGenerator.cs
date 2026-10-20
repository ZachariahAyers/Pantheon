using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pantheon.Core.Roofs;
using Pantheon.Core.Workframes;

namespace Pantheon.Demo;

/// <summary>
/// Builds the workframe the browser desk shows and downloads, from the same entries the add-in's
/// workframe dialogs take, turned into Pantheon.Core models the way the add-in's member catalogue
/// turns them.
/// </summary>
public static class DemoGenerator
{
    private const double SteepestRisePerTwelve = 12;
    private const double LargestLength = 400 * 12;
    private const int MostBays = 40;

    /// <summary>Grid text height in inches; 9 in reads as 3/32 in on a sheet at 1/8 in = 1 ft.</summary>
    private const double TextHeight = 9;

    /// <summary>Returns a stable JSON response for one dialog's entries.</summary>
    public static string GenerateJson(string requestJson)
    {
        try
        {
            var frame = Build(Parse(requestJson));
            var lines = frame.Geometry.Lines.Select(line => new DemoLine(
                line.Kind.ToString().ToLowerInvariant(),
                Point(line.StartPoint),
                Point(line.EndPoint))).ToArray();
            var bubbles = frame.Geometry.Bubbles
                .Select(bubble => new DemoBubble(bubble.Label, Point(bubble.Center), bubble.Radius))
                .ToArray();
            return Serialize(new DemoResponse(true, null, frame.Readouts, lines, bubbles));
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or FormatException)
        {
            return Failure(exception is JsonException ? "Fill in every field of the dialog." : exception.Message);
        }
    }

    /// <summary>Returns the workframe as an AutoCAD R12 DXF drawing in inches, or an empty string for refused entries.</summary>
    public static string GenerateDxf(string requestJson)
    {
        try
        {
            return WorkframeDxf.Write(Build(Parse(requestJson)).Geometry);
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or FormatException)
        {
            return "";
        }
    }

    /// <summary>A length in inches as feet and inches to the nearest sixteenth, the way a drafter writes it.</summary>
    public static string FeetAndInches(double inches)
    {
        var sixteenths = (long)Math.Round(inches * 16, MidpointRounding.AwayFromZero);
        var feet = sixteenths / 192;
        var remainder = sixteenths % 192;
        var whole = remainder / 16;
        var fraction = remainder % 16;
        if (fraction == 0)
        {
            return $"{feet}′ {whole}″";
        }

        var denominator = 16L;
        while (fraction % 2 == 0)
        {
            fraction /= 2;
            denominator /= 2;
        }

        return $"{feet}′ {whole} {fraction}/{denominator}″";
    }

    /// <summary>A pitch as rise per 12, with at most two decimals.</summary>
    public static string RisePerTwelve(double ratio) =>
        Math.Round(ratio * 12, 2).ToString("0.##", CultureInfo.InvariantCulture) + ":12";

    private static WorkframeDialog Parse(string requestJson) =>
        JsonSerializer.Deserialize(requestJson, DemoJsonContext.Default.WorkframeDialog)
            ?? throw new ArgumentException("Fill in the dialog.");

    private static DemoFrame Build(WorkframeDialog dialog)
    {
        var bays = new BayLayout(Bays(dialog.SideBays, "Sidewall bays"), Bays(dialog.EndBays, "Endwall bays"));
        return dialog.Roof switch
        {
            "symmetrical" => Symmetrical(dialog, bays),
            "singleslope" => SingleSlope(dialog, bays),
            "offcenter" => OffCenter(dialog, bays),
            _ => throw new ArgumentException("Choose Symmetrical, Single Slope or Off-Center Ridge."),
        };
    }

    private static DemoFrame Symmetrical(WorkframeDialog dialog, BayLayout bays)
    {
        var width = Length(dialog.Width, "Width");
        var length = Length(dialog.Length, "Length");
        var eave = Length(dialog.Eave, "Eave height");
        var pitch = Pitch.FromRisePerTwelve(RiseNumber(dialog.Pitch, "Pitch"));
        var building = new SymmetricalWorkframe(width, length, eave, pitch, FramingOf(dialog.Purlin, dialog.Girt, dialog.Girts), bays);
        return new DemoFrame(building.Build(TextHeight),
        [
            new("Width", FeetAndInches(width)),
            new("Length", FeetAndInches(length)),
            new("Eave", FeetAndInches(eave)),
            new("Ridge", FeetAndInches(building.RidgeHeight)),
            new("Rafter", $"{FeetAndInches(RightTriangle.FromRun(width / 2, pitch).Slope)} · {RisePerTwelve(pitch)}"),
            BayReadout(bays),
        ]);
    }

    private static DemoFrame SingleSlope(WorkframeDialog dialog, BayLayout bays)
    {
        var width = Length(dialog.Width, "Width");
        var length = Length(dialog.Length, "Length");
        var lowEave = Length(dialog.LowEave, "Low eave height");
        var highEave = Length(dialog.HighEave, "High eave height");
        var building = new SingleSlopeWorkframe(width, length, lowEave, highEave, FramingOf(dialog.Purlin, dialog.Girt, dialog.Girts), bays);
        Steepness(building.Pitch, "The roof");
        return new DemoFrame(building.Build(TextHeight),
        [
            new("Width", FeetAndInches(width)),
            new("Length", FeetAndInches(length)),
            new("Low eave", FeetAndInches(lowEave)),
            new("High eave", FeetAndInches(highEave)),
            new("Rafter", $"{FeetAndInches(RightTriangle.FromRun(width, building.Pitch).Slope)} · {RisePerTwelve(building.Pitch)}"),
            BayReadout(bays),
        ]);
    }

    /// <summary>The add-in's off-center dialog takes each side on its own: the left is the far side, the right the near side.</summary>
    private static DemoFrame OffCenter(WorkframeDialog dialog, BayLayout bays)
    {
        var left = SideOf(dialog.Left, "Left side");
        var right = SideOf(dialog.Right, "Right side");
        var building = new OffsetRidgeWorkframe(left, right, bays);
        return new DemoFrame(building.Build(TextHeight),
        [
            new("Width", FeetAndInches(left.Width + right.Width)),
            new("Left ridge", FeetAndInches(left.RidgeHeight)),
            new("Right ridge", FeetAndInches(right.RidgeHeight)),
            new("Left rafter", $"{FeetAndInches(RightTriangle.FromRun(left.Width, left.Pitch).Slope)} · {RisePerTwelve(left.Pitch)}"),
            new("Right rafter", $"{FeetAndInches(RightTriangle.FromRun(right.Width, right.Pitch).Slope)} · {RisePerTwelve(right.Pitch)}"),
            BayReadout(bays),
        ]);
    }

    private static RoofSide SideOf(DialogSide? side, string group)
    {
        if (side is null)
        {
            throw new ArgumentException($"{group}: fill in its fields.");
        }

        return new RoofSide(
            Length(side.Width, $"{group} width"),
            Length(side.Length, $"{group} length"),
            Length(side.Eave, $"{group} eave height"),
            Pitch.FromRisePerTwelve(RiseNumber(side.Pitch, $"{group} pitch")),
            FramingOf(side.Purlin, side.Girt, side.Girts));
    }

    private static Framing FramingOf(string purlin, string girt, string girts) =>
        new(Length(purlin, "Purlin depth"), Length(girt, "Girt depth"), girts == "Bypass" ? GirtAttachment.Bypass : GirtAttachment.Flush);

    private static DemoReadout BayReadout(BayLayout bays) =>
        new("Bays", bays.SideBays.Count == 0 ? "Ends only" : $"{bays.SideBays.Count} · {FeetAndInches(bays.SideBays.Max())} max");

    private static double Length(string text, string caption)
    {
        double inches;
        try
        {
            inches = ArchitecturalLength.Parse(text);
        }
        catch (FormatException exception)
        {
            throw new FormatException($"{caption}: {exception.Message}", exception);
        }

        if (inches > LargestLength)
        {
            throw new ArgumentException($"{caption} must be 400' or less in this demo.");
        }

        return inches;
    }

    private static double RiseNumber(string text, string caption)
    {
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var rise) || rise < 0 || rise > SteepestRisePerTwelve)
        {
            throw new ArgumentException($"{caption} must be a rise per 12 from 0 to 12, for example 1 for 1:12.");
        }

        return rise;
    }

    private static void Steepness(double ratio, string what)
    {
        if (ratio * 12 > SteepestRisePerTwelve)
        {
            throw new ArgumentException($"{what} is steeper than 12:12 in this demo.");
        }
    }

    private static List<double> Bays(string text, string caption)
    {
        try
        {
            var bays = BayList.Parse(text ?? "", ArchitecturalLength.Parse).ToList();
            if (bays.Count > MostBays)
            {
                throw new ArgumentException($"{caption}: {MostBays} bays at most in this demo.");
            }

            return bays;
        }
        catch (FormatException exception)
        {
            throw new FormatException($"{caption}: {exception.Message}", exception);
        }
    }

    private static DemoPoint Point(Pantheon.Core.Geometry.Point3 point) => new(point.X, point.Y, point.Z);

    private static string Failure(string message) =>
        Serialize(new DemoResponse(false, message, [], [], []));

    private static string Serialize(DemoResponse response) =>
        JsonSerializer.Serialize(response, DemoJsonContext.Default.DemoResponse);

    private sealed record DemoFrame(WorkframeGeometry Geometry, DemoReadout[] Readouts);
}

/// <summary>
/// One workframe dialog's entries, as typed. <see cref="Roof"/> is symmetrical, singleslope or
/// offcenter; an off-center frame reads <see cref="Left"/> and <see cref="Right"/> and the bays.
/// </summary>
public sealed record WorkframeDialog(
    string Roof,
    string Width = "",
    string Eave = "",
    string Length = "",
    string Pitch = "",
    string LowEave = "",
    string HighEave = "",
    string Purlin = "8\"",
    string Girt = "8\"",
    string Girts = "Flush",
    string SideBays = "",
    string EndBays = "",
    DialogSide? Left = null,
    DialogSide? Right = null);

/// <summary>One side of the off-center dialog.</summary>
public sealed record DialogSide(
    string Width,
    string Eave,
    string Length,
    string Pitch,
    string Purlin = "8\"",
    string Girt = "8\"",
    string Girts = "Flush");

/// <summary>The result returned across the .NET-to-JavaScript boundary.</summary>
public sealed record DemoResponse(bool Ok, string? Error, DemoReadout[] Readouts, DemoLine[] Lines, DemoBubble[] Bubbles);

/// <summary>One calculated dimension, already written as a drafter reads it.</summary>
public sealed record DemoReadout(string Label, string Value);

/// <summary>One Pantheon.Core workframe line.</summary>
public sealed record DemoLine(string Kind, DemoPoint Start, DemoPoint End);

/// <summary>One grid bubble: its label and the circle around it.</summary>
public sealed record DemoBubble(string Label, DemoPoint Center, double Radius);

/// <summary>One model-space point in inches.</summary>
public sealed record DemoPoint(double X, double Y, double Z);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(WorkframeDialog))]
[JsonSerializable(typeof(DemoResponse))]
public sealed partial class DemoJsonContext : JsonSerializerContext
{
}
