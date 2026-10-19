using System.Text.Json;
using System.Text.Json.Serialization;
using Pantheon.Core.Roofs;
using Pantheon.Core.Workframes;

namespace Pantheon.Demo;

/// <summary>Builds the small gable-frame example shown by the browser demo.</summary>
public static class DemoGenerator
{
    private const double InchesPerFoot = 12;
    private const double TextHeight = 3;

    /// <summary>Returns a stable JSON response for one browser request.</summary>
    public static string GenerateJson(string requestJson)
    {
        try
        {
            var request = JsonSerializer.Deserialize(requestJson, DemoJsonContext.Default.DemoRequest)
                ?? throw new ArgumentException("Enter the building dimensions.");
            Validate(request);

            var span = request.SpanFeet * InchesPerFoot;
            var length = request.LengthFeet * InchesPerFoot;
            var eave = request.EaveFeet * InchesPerFoot;
            var pitch = Pitch.FromRisePerTwelve(request.PitchRise);
            var baySize = length / request.BayCount;
            var building = new SymmetricalWorkframe(
                span,
                length,
                eave,
                pitch,
                new Framing(8, 8, GirtAttachment.Flush),
                new BayLayout(Enumerable.Repeat(baySize, request.BayCount), []));
            var geometry = building.Build(TextHeight);
            var roofSlope = RightTriangle.FromRun(span / 2, pitch).Slope;
            var lines = geometry.Lines.Select(line => new DemoLine(
                line.Kind.ToString().ToLowerInvariant(),
                new DemoPoint(line.StartPoint.X, line.StartPoint.Y, line.StartPoint.Z),
                new DemoPoint(line.EndPoint.X, line.EndPoint.Y, line.EndPoint.Z))).ToArray();
            var dimensions = new DemoDimensions(
                span,
                length,
                eave,
                building.RidgeHeight,
                roofSlope,
                Pitch.AngleDegrees(pitch),
                baySize,
                request.BayCount,
                request.BayCount + 1,
                lines.Length);

            return Serialize(new DemoResponse(true, null, "inches", dimensions, lines));
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

    private static void Validate(DemoRequest request)
    {
        InRange(request.SpanFeet, 20, 200, "Span", "feet");
        InRange(request.LengthFeet, 20, 400, "Length", "feet");
        InRange(request.EaveFeet, 8, 60, "Eave height", "feet");
        InRange(request.PitchRise, 0, 12, "Pitch", "in 12");
        if (request.BayCount is < 1 or > 20)
        {
            throw new ArgumentException("Bays must be a whole number from 1 to 20.");
        }
    }

    private static void InRange(double value, double minimum, double maximum, string name, string unit)
    {
        if (!double.IsFinite(value) || value < minimum || value > maximum)
        {
            throw new ArgumentException($"{name} must be from {minimum:g} to {maximum:g} {unit}.");
        }
    }

    private static string Failure(string message) =>
        Serialize(new DemoResponse(false, message, "inches", null, []));

    private static string Serialize(DemoResponse response) =>
        JsonSerializer.Serialize(response, DemoJsonContext.Default.DemoResponse);
}

/// <summary>The dimensions editable in the browser's supported gable-frame example.</summary>
public sealed record DemoRequest(double SpanFeet, double LengthFeet, double EaveFeet, double PitchRise, int BayCount);

/// <summary>The result returned across the .NET-to-JavaScript boundary.</summary>
public sealed record DemoResponse(
    bool Ok,
    string? Error,
    string Unit,
    DemoDimensions? Dimensions,
    DemoLine[] Lines);

/// <summary>Calculated values shown in the drawing desk's readout.</summary>
public sealed record DemoDimensions(
    double Span,
    double Length,
    double Eave,
    double Ridge,
    double RoofSlope,
    double PitchDegrees,
    double BaySize,
    int BayCount,
    int FrameCount,
    int LineCount);

/// <summary>One Pantheon.Core workframe line.</summary>
public sealed record DemoLine(string Kind, DemoPoint Start, DemoPoint End);

/// <summary>One model-space point in inches.</summary>
public sealed record DemoPoint(double X, double Y, double Z);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(DemoRequest))]
[JsonSerializable(typeof(DemoResponse))]
public sealed partial class DemoJsonContext : JsonSerializerContext
{
}
