Imports Pantheon.Core.Geometry
Imports Xunit

''' <summary>Point comparisons with a tolerance for floating point geometry.</summary>
Friend Module Expect

    Private Const Places As Integer = 9

    Friend Sub SamePoint(expected As Point2, actual As Point2)
        Assert.Equal(expected.X, actual.X, Places)
        Assert.Equal(expected.Y, actual.Y, Places)
    End Sub

    Friend Sub SamePoint(expected As Point3, actual As Point3)
        Assert.Equal(expected.X, actual.X, Places)
        Assert.Equal(expected.Y, actual.Y, Places)
        Assert.Equal(expected.Z, actual.Z, Places)
    End Sub

    Friend Sub SamePoints(expected As IReadOnlyList(Of Point2), actual As IReadOnlyList(Of Point2))
        Assert.Equal(expected.Count, actual.Count)
        For i = 0 To expected.Count - 1
            SamePoint(expected(i), actual(i))
        Next
    End Sub

    Friend Sub SameValues(expected As IReadOnlyList(Of Double), actual As IReadOnlyList(Of Double))
        Assert.Equal(expected.Count, actual.Count)
        For i = 0 To expected.Count - 1
            Assert.Equal(expected(i), actual(i), Places)
        Next
    End Sub

    Friend Sub SameLabels(expected As IReadOnlyList(Of String), actual As IReadOnlyList(Of String))
        Assert.Equal(expected.Count, actual.Count)
        For i = 0 To expected.Count - 1
            Assert.Equal(expected(i), actual(i))
        Next
    End Sub

    Friend Function P(x As Double, y As Double) As Point2
        Return New Point2(x, y)
    End Function

End Module
