using Pantheon.Demo;
using Xunit;

namespace Pantheon.Demo.Tests;

public sealed class ArchitecturalLengthTests
{
    [Theory]
    [InlineData("40'", 480)]
    [InlineData("25'6\"", 306)]
    [InlineData("25'-6\"", 306)]
    [InlineData("25' 6", 306)]
    [InlineData("12'6-1/2\"", 150.5)]
    [InlineData("12'-6 1/2\"", 150.5)]
    [InlineData("6\"", 6)]
    [InlineData("1/2\"", 0.5)]
    [InlineData("3-1/2", 3.5)]
    [InlineData("480", 480)]
    [InlineData("8.5", 8.5)]
    [InlineData(" 30' ", 360)]
    public void ReadsTheLengthsADrafterTypes(string text, double inches)
    {
        Assert.Equal(inches, ArchitecturalLength.Parse(text));
    }

    [Theory]
    [InlineData("")]
    [InlineData("forty")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("3/0")]
    [InlineData("12'x")]
    public void RefusesWhatIsNotALength(string text)
    {
        Assert.Throws<FormatException>(() => ArchitecturalLength.Parse(text));
    }
}
