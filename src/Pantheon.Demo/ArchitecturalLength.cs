using System.Globalization;
using System.Text.RegularExpressions;

namespace Pantheon.Demo;

/// <summary>
/// Reads a length the way the add-in's dialogs accept one in architectural units, in inches:
/// <c>40'</c>, <c>25'6"</c>, <c>25'-6 1/2"</c>, <c>3-1/2</c>, <c>1/2"</c> or a bare number of inches.
/// The add-in hands this to AutoCAD's own parser; the browser has none, so this covers the forms
/// a drafter types for building dimensions.
/// </summary>
public static partial class ArchitecturalLength
{
    [GeneratedRegex("""^(?:(?<feet>\d+(?:\.\d+)?)'\s*-?\s*)?(?:(?<inches>\d+(?:\.\d+)?)(?:(?:\s+|-)(?<num>\d+)/(?<den>\d+))?|(?<num2>\d+)/(?<den2>\d+))?\s*"?$""")]
    private static partial Regex Notation();

    /// <summary>Returns <paramref name="text"/> in inches.</summary>
    /// <exception cref="FormatException">The text is not a length greater than zero.</exception>
    public static double Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var trimmed = text.Trim();
        var match = Notation().Match(trimmed);
        if (trimmed.Length == 0 || !match.Success || (!match.Groups["feet"].Success && !match.Groups["inches"].Success && !match.Groups["num2"].Success))
        {
            throw new FormatException($"\"{text}\" is not a length. Write it like 40', 25'6\" or 480.");
        }

        var inches = Number(match, "feet") * 12 + Number(match, "inches");
        if (match.Groups["num"].Success)
        {
            inches += Fraction(match.Groups["num"].Value, match.Groups["den"].Value, text);
        }

        if (match.Groups["num2"].Success)
        {
            inches += Fraction(match.Groups["num2"].Value, match.Groups["den2"].Value, text);
        }

        if (!(inches > 0))
        {
            throw new FormatException($"\"{text}\" must be greater than zero.");
        }

        return inches;
    }

    private static double Number(Match match, string group) =>
        match.Groups[group].Success ? double.Parse(match.Groups[group].Value, CultureInfo.InvariantCulture) : 0;

    private static double Fraction(string numerator, string denominator, string text)
    {
        var bottom = int.Parse(denominator, CultureInfo.InvariantCulture);
        if (bottom == 0)
        {
            throw new FormatException($"\"{text}\" has a fraction over zero.");
        }

        return int.Parse(numerator, CultureInfo.InvariantCulture) / (double)bottom;
    }
}
