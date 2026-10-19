Imports System.Globalization
Imports Pantheon.Core.Roofs
Imports Xunit

Public NotInheritable Class RoofTests

    <Theory>
    <InlineData(1.0, "4.763642")>
    <InlineData(2.0, "9.462322")>
    <InlineData(3.0, "14.03624")>
    <InlineData(4.0, "18.43495")>
    <InlineData(5.0, "22.61986")>
    <InlineData(6.0, "26.56505")>
    <InlineData(-1.0, "-4.763642")>
    <InlineData(-6.0, "-26.56505")>
    Public Sub UcsRotationCommandReproducesThePalettePresets(risePerTwelve As Double, angle As String)
        Assert.Equal("_ucs _world _ucs _z " & angle & " ", Pitch.UcsRotationCommand(risePerTwelve))
    End Sub

    <Fact>
    Public Sub UcsRotationCommandWritesADecimalPointInEveryCulture()
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE")
            Assert.Equal("_ucs _world _ucs _z 18.43495 ", Pitch.UcsRotationCommand(4))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <Fact>
    Public Sub FromRunGivesRiseAndSlopedLength()
        Dim triangle = RightTriangle.FromRun(12, 4.0 / 12)
        Assert.Equal(4.0, triangle.Rise, 9)
        Assert.Equal(Math.Sqrt(160), triangle.Slope, 9)
    End Sub

    <Fact>
    Public Sub FromSlopeGivesRunAndRise()
        Dim triangle = RightTriangle.FromSlope(Math.Sqrt(160), 4.0 / 12)
        Assert.Equal(12.0, triangle.Run, 9)
        Assert.Equal(4.0, triangle.Rise, 9)
    End Sub

End Class
