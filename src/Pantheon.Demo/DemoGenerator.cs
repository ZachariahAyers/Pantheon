using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pantheon.Core.Roofs;
using Pantheon.Core.Workframes;

namespace Pantheon.Demo;

/// <summary>Builds the workframes the browser demo shows and downloads.</summary>
public static class DemoGenerator
{
    private const double InchesPerFoot = 12;
    private const double SteepestRisePerTwelve = 12;

    /// <summary>Grid text height in inches; 9 in reads as 3/32 in on a sheet at 1/8 in = 1 ft.</summary>
    private const double TextHeight = 9;

    /// <summary>Returns a stable JSON response for one browser request.</summary>
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
            var summary = new DemoSummary(frame.BayCount + 1, frame.BayCount, lines.Length);
            return Serialize(new DemoResponse(true, null, "inches", frame.Readouts, summary, lines, bubbles));
        }
        catch (JsonException)
        {
            return Failure("Enter numbers for every building dimension.");
        }
        catch (ArgumentException exception)
        {
            return Failure(exception.Message);
        }
    }

    /// <summary>Returns the workframe as an AutoCAD R12 DXF drawing in inches, or an empty string for a refused request.</summary>
    public static string GenerateDxf(string requestJson)
    {
        try
        {
            return WorkframeDxf.Write(Build(Parse(requestJson)).Geometry);
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
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

    private static DemoRequest Parse(string requestJson) =>
        JsonSerializer.Deserialize(requestJson, DemoJsonContext.Default.DemoRequest)
            ?? throw new ArgumentException("Enter the building dimensions.");

    private static DemoFrame Build(DemoRequest request)
    {
        InRange(request.SpanFeet, 20, 200, "Span", "feet");
        InRange(request.LengthFeet, 20, 400, "Length", "feet");
        InRange(request.EaveFeet, 8, 60, "Eave height", "feet");
        if (request.BayCount is < 1 or > 20)
        {
            throw new ArgumentException("Bays must be a whole number from 1 to 20.");
        }

        var span = request.SpanFeet * InchesPerFoot;
        var length = request.LengthFeet * InchesPerFoot;
        var eave = request.EaveFeet * InchesPerFoot;
        var baySize = length / request.BayCount;
        var framing = new Framing(8, 8, GirtAttachment.Flush);
        var bays = new BayLayout(Enumerable.Repeat(baySize, request.BayCount), []);
        var baySpacing = new DemoReadout("Bay spacing", FeetAndInches(baySize));

        return request.Roof switch
        {
            "gable" => Gable(request, span, length, eave, framing, bays, baySpacing),
            "single" => SingleSlope(request, span, length, eave, framing, bays, baySpacing),
            "offset" => OffsetRidge(request, span, length, eave, framing, bays, baySpacing),
            _ => throw new ArgumentException("Choose a gable, single slope or off-center ridge roof."),
        };
    }

    private static DemoFrame Gable(DemoRequest request, double span, double length, double eave,
        Framing framing, BayLayout bays, DemoReadout baySpacing)
    {
        InRange(request.PitchRise, 0, SteepestRisePerTwelve, "Pitch", "in 12");
        var pitch = Pitch.FromRisePerTwelve(request.PitchRise);
        var building = new SymmetricalWorkframe(span, length, eave, pitch, framing, bays);
        var rafter = RightTriangle.FromRun(span / 2, pitch).Slope;
        return new DemoFrame(building.Build(TextHeight), request.BayCount,
        [
            new("Span", FeetAndInches(span)),
            new("Length", FeetAndInches(length)),
            new("Eave", FeetAndInches(eave)),
            new("Ridge", FeetAndInches(building.RidgeHeight)),
            new("Roof slope", $"{FeetAndInches(rafter)} · {RisePerTwelve(pitch)}"),
            baySpacing,
        ]);
    }

    private static DemoFrame SingleSlope(DemoRequest request, double span, double length, double eave,
        Framing framing, BayLayout bays, DemoReadout baySpacing)
    {
        InRange(request.HighEaveFeet, request.EaveFeet, 60, "High eave", "feet");
        var highEave = request.HighEaveFeet * InchesPerFoot;
        var building = new SingleSlopeWorkframe(span, length, eave, highEave, framing, bays);
        if (building.Pitch * 12 > SteepestRisePerTwelve)
        {
            throw new ArgumentException("That roof is steeper than 12:12. Lower the high eave or widen the span.");
        }

        var rafter = RightTriangle.FromRun(span, building.Pitch).Slope;
        return new DemoFrame(building.Build(TextHeight), request.BayCount,
        [
            new("Span", FeetAndInches(span)),
            new("Length", FeetAndInches(length)),
            new("Low eave", FeetAndInches(eave)),
            new("High eave", FeetAndInches(highEave)),
            new("Roof slope", $"{FeetAndInches(rafter)} · {RisePerTwelve(building.Pitch)}"),
            baySpacing,
        ]);
    }

    /// <summary>
    /// Both eaves stand at one height, so the left side's pitch follows from where the ridge sits:
    /// the two rafters must meet at the same ridge height.
    /// </summary>
    private static DemoFrame OffsetRidge(DemoRequest request, double span, double length, double eave,
        Framing framing, BayLayout bays, DemoReadout baySpacing)
    {
        InRange(request.PitchRise, 0, SteepestRisePerTwelve, "Pitch", "in 12");
        InRange(request.RidgeFeet, 1, request.SpanFeet - 1, "Ridge from right eave", "feet");
        var rightWidth = request.RidgeFeet * InchesPerFoot;
        var leftWidth = span - rightWidth;
        var rightPitch = Pitch.FromRisePerTwelve(request.PitchRise);
        var leftPitch = rightWidth * rightPitch / leftWidth;
        if (leftPitch * 12 > SteepestRisePerTwelve)
        {
            throw new ArgumentException("The left side would be steeper than 12:12. Move the ridge toward the center or lower the pitch.");
        }

        var right = new RoofSide(rightWidth, length, eave, rightPitch, framing);
        var left = new RoofSide(leftWidth, length, eave, leftPitch, framing);
        var building = new OffsetRidgeWorkframe(left, right, bays);
        return new DemoFrame(building.Build(TextHeight), request.BayCount,
        [
            new("Span", FeetAndInches(span)),
            new("Eave", FeetAndInches(eave)),
            new("Ridge", FeetAndInches(right.RidgeHeight)),
            new("Right slope", $"{FeetAndInches(RightTriangle.FromRun(rightWidth, rightPitch).Slope)} · {RisePerTwelve(rightPitch)}"),
            new("Left slope", $"{FeetAndInches(RightTriangle.FromRun(leftWidth, leftPitch).Slope)} · {RisePerTwelve(leftPitch)}"),
            baySpacing,
        ]);
    }

    private static void InRange(double value, double minimum, double maximum, string name, string unit)
    {
        if (!double.IsFinite(value) || value < minimum || value > maximum)
        {
            throw new ArgumentException($"{name} must be from {minimum:g} to {maximum:g} {unit}.");
        }
    }

    private static DemoPoint Point(Pantheon.Core.Geometry.Point3 point) => new(point.X, point.Y, point.Z);

    private static string Failure(string message) =>
        Serialize(new DemoResponse(false, message, "inches", [], null, [], []));

    private static string Serialize(DemoResponse response) =>
        JsonSerializer.Serialize(response, DemoJsonContext.Default.DemoResponse);

    private sealed record DemoFrame(WorkframeGeometry Geometry, int BayCount, DemoReadout[] Readouts);
}

/// <summary>
/// The dimensions editable in the browser, in feet. <see cref="Roof"/> is gable, single or offset;
/// a single slope reads <see cref="HighEaveFeet"/>, an off-center ridge reads <see cref="RidgeFeet"/>.
/// </summary>
public sealed record DemoRequest(
    string Roof,
    double SpanFeet,
    double LengthFeet,
    double EaveFeet,
    double PitchRise,
    int BayCount,
    double HighEaveFeet = 0,
    double RidgeFeet = 0);

/// <summary>The result returned across the .NET-to-JavaScript boundary.</summary>
public sealed record DemoResponse(
    bool Ok,
    string? Error,
    string Unit,
    DemoReadout[] Readouts,
    DemoSummary? Summary,
    DemoLine[] Lines,
    DemoBubble[] Bubbles);

/// <summary>One calculated dimension, already written as a drafter reads it.</summary>
public sealed record DemoReadout(string Label, string Value);

/// <summary>The counts shown under the drawing.</summary>
public sealed record DemoSummary(int FrameCount, int BayCount, int LineCount);

/// <summary>One Pantheon.Core workframe line.</summary>
public sealed record DemoLine(string Kind, DemoPoint Start, DemoPoint End);

/// <summary>One grid bubble: its label and the circle around it.</summary>
public sealed record DemoBubble(string Label, DemoPoint Center, double Radius);

/// <summary>One model-space point in inches.</summary>
public sealed record DemoPoint(double X, double Y, double Z);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(DemoRequest))]
[JsonSerializable(typeof(DemoResponse))]
public sealed partial class DemoJsonContext : JsonSerializerContext
{
}
