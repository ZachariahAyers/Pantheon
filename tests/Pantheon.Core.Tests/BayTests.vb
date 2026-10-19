Imports System.Globalization
Imports Pantheon.Core.Workframes
Imports Xunit

Public NotInheritable Class BayTests

    ''' <summary>Stand-in for the AutoCAD unit parser: feet with a trailing ', otherwise inches.</summary>
    Private Shared Function Inches(text As String) As Double
        If text.EndsWith("'"c) Then
            Return Double.Parse(text.TrimEnd("'"c), CultureInfo.InvariantCulture) * 12
        End If
        Return Double.Parse(text, CultureInfo.InvariantCulture)
    End Function

    <Fact>
    Public Sub RepeatedAndSingleBaysExpandInOrder()
        SameValues({300.0, 300.0, 300.0, 240.0}, BayList.Parse("3*25',20'", AddressOf Inches))
    End Sub

    <Fact>
    Public Sub SpacesAndATrailingCommaAreIgnored()
        SameValues({120.0, 120.0, 96.0}, BayList.Parse(" 2 * 10' , 96 ,", AddressOf Inches))
    End Sub

    <Fact>
    Public Sub BlankTextMeansNoBays()
        Assert.Empty(BayList.Parse("  ", AddressOf Inches))
    End Sub

    <Theory>
    <InlineData("x*25'")>
    <InlineData("0*25'")>
    <InlineData("2*3*25'")>
    <InlineData("0")>
    Public Sub MalformedBaysAreRejected(text As String)
        Assert.Throws(Of FormatException)(Function() BayList.Parse(text, AddressOf Inches))
    End Sub

    <Theory>
    <InlineData(1, "A")>
    <InlineData(26, "Z")>
    <InlineData(27, "AA")>
    <InlineData(52, "AZ")>
    <InlineData(53, "BA")>
    Public Sub GridLettersRunPastZ(position As Integer, expected As String)
        Assert.Equal(expected, GridLabels.Letter(position))
    End Sub

    <Fact>
    Public Sub GridLettersStartAtOne()
        Assert.Throws(Of ArgumentException)(Function() GridLabels.Letter(0))
    End Sub

End Class
