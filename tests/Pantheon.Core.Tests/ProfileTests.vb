Imports Pantheon.Core.Geometry
Imports Pantheon.Core.Profiles
Imports Xunit

Public NotInheritable Class ProfileTests

    ' Vertices worked from the v1 drawing code for an 8 inch web and 3.5 inch flange about the origin.
    Private Shared ReadOnly CeeFlangesUp As Point2() = {
        P(-3, 3.5), P(-4, 3.5), P(-4, 0), P(4, 0), P(4, 3.5), P(3, 3.5),
        P(3, 3.25), P(3.75, 3.25), P(3.75, 0.25), P(-3.75, 0.25), P(-3.75, 3.25), P(-3, 3.25)}

    Private Shared ReadOnly CeeFlangesRight As Point2() = {
        P(3.5, 3), P(3.5, 4), P(0, 4), P(0, -4), P(3.5, -4), P(3.5, -3),
        P(3.25, -3), P(3.25, -3.75), P(0.25, -3.75), P(0.25, 3.75), P(3.25, 3.75), P(3.25, 3)}

    <Fact>
    Public Sub CeeWithFlangesUpMatchesTheOriginalDrawing()
        Dim outline = CeeProfile.Outline(8, 3.5, FlangeDirection.Up)
        Assert.True(outline.IsClosed)
        SamePoints(CeeFlangesUp, outline.Points)
    End Sub

    <Fact>
    Public Sub CeeWithFlangesRightMatchesTheOriginalDrawing()
        SamePoints(CeeFlangesRight, CeeProfile.Outline(8, 3.5, FlangeDirection.Right).Points)
    End Sub

    <Fact>
    Public Sub CeeWithFlangesDownAndLeftAreMirrorImages()
        SamePoints(CeeFlangesUp.Select(Function(v) v.MirrorY()).ToArray(), CeeProfile.Outline(8, 3.5, FlangeDirection.Down).Points)
        SamePoints(CeeFlangesRight.Select(Function(v) v.MirrorX()).ToArray(), CeeProfile.Outline(8, 3.5, FlangeDirection.Left).Points)
    End Sub

    <Fact>
    Public Sub CeeRejectsAWebTooShortForItsLips()
        Assert.Throws(Of ArgumentException)(Function() CeeProfile.Outline(2, 3.5, FlangeDirection.Up))
    End Sub

    <Fact>
    Public Sub LeftGirtMatchesTheOriginalDrawing()
        Dim expected = {P(7.3125, 3.1875), P(8, 2.5), P(8, 0), P(0, 0), P(0, -2.5), P(0.6875, -3.1875)}
        SamePoints(expected, ZeeProfile.Outline(ZeeMember.Girt, Hand.Left, 8, 2.5, 0).Points)
    End Sub

    <Fact>
    Public Sub RightGirtMatchesTheOriginalDrawing()
        Dim expected = {P(-7.3125, 3.1875), P(-8, 2.5), P(-8, 0), P(0, 0), P(0, -2.5), P(-0.6875, -3.1875)}
        SamePoints(expected, ZeeProfile.Outline(ZeeMember.Girt, Hand.Right, 8, 2.5, 0).Points)
    End Sub

    <Fact>
    Public Sub GirtIgnoresPitch()
        SamePoints(ZeeProfile.Outline(ZeeMember.Girt, Hand.Left, 8, 2.5, 0).Points,
                   ZeeProfile.Outline(ZeeMember.Girt, Hand.Left, 8, 2.5, 0.5).Points)
    End Sub

    <Fact>
    Public Sub FlatLeftPurlinMatchesTheOriginalDrawing()
        Dim expected = {P(-3.1875, -7.3125), P(-2.5, -8), P(0, -8), P(0, 0), P(2.5, 0), P(3.1875, -0.6875)}
        Dim outline = ZeeProfile.Outline(ZeeMember.Purlin, Hand.Left, 8, 2.5, 0)
        Assert.False(outline.IsClosed)
        SamePoints(expected, outline.Points)
    End Sub

    <Fact>
    Public Sub FlatRightPurlinFacesRight()
        ' The original drew this case with the left-hand shape.
        Dim expected = {P(3.1875, -7.3125), P(2.5, -8), P(0, -8), P(0, 0), P(-2.5, 0), P(-3.1875, -0.6875)}
        SamePoints(expected, ZeeProfile.Outline(ZeeMember.Purlin, Hand.Right, 8, 2.5, 0).Points)
    End Sub

    <Fact>
    Public Sub PitchedPurlinTurnsWithTheRoofIncludingItsLips()
        ' 4:12 is a 3-4-5 slope scaled: cos = 12 / sqrt(160), sin = 4 / sqrt(160).
        Dim cos = 12 / Math.Sqrt(160)
        Dim sin = 4 / Math.Sqrt(160)
        Dim points = ZeeProfile.Outline(ZeeMember.Purlin, Hand.Left, 8, 2.5, 4.0 / 12).Points
        SamePoint(P(2.5 * cos, 2.5 * sin), points(4))
        SamePoint(P(8 * sin, -8 * cos), points(2))
        Dim lip = points(5) - points(4)
        SamePoint(P(0.6875 * (cos + sin), 0.6875 * (sin - cos)), lip)
    End Sub

    <Fact>
    Public Sub EaveStrutDoubleSlopeUpFollowsThePickedPitch()
        ' Web from (0, 0) to (0, 8); pitch point (12, 12) is a 1:3 slope: cos = 3 / sqrt(10), sin = 1 / sqrt(10).
        Dim root = Math.Sqrt(10)
        Dim points = EaveStrutProfile.Outline(P(0, 0), P(0, 8), P(12, 12), EaveStrutStyle.DoubleSlopeUp, 3, 5).Points
        SamePoints({P(15 / root, 5 / root + 1), P(15 / root, 5 / root), P(0, 0), P(0, 8),
                    P(9 / root, 8 + 3 / root), P(9 / root, 8 + 3 / root - 1)}, points)
    End Sub

    <Fact>
    Public Sub EaveStrutSingleSlopeKeepsTheBottomFlangeLevel()
        Dim points = EaveStrutProfile.Outline(P(0, 0), P(0, 8), P(-12, 4), EaveStrutStyle.SingleSlopeDown, 3, 5).Points
        SamePoint(P(-5, 0), points(1))
        SamePoint(P(-5, 1), points(0))
    End Sub

    <Fact>
    Public Sub EaveStrutDrawsFlatForALevelPitchPoint()
        Dim points = EaveStrutProfile.Outline(P(0, 0), P(0, 8), P(12, 8), EaveStrutStyle.DoubleSlopeUp, 3, 5).Points
        SamePoint(P(3, 8), points(4))
    End Sub

    <Theory>
    <InlineData(12.0, 4.0, EaveStrutStyle.DoubleSlopeUp)>
    <InlineData(12.0, 12.0, EaveStrutStyle.SingleSlopeDown)>
    <InlineData(0.0, 12.0, EaveStrutStyle.DoubleSlopeUp)>
    Public Sub EaveStrutRefusesAPitchPointThatDisagrees(x As Double, y As Double, style As EaveStrutStyle)
        Assert.Throws(Of ArgumentException)(Function() EaveStrutProfile.Outline(P(0, 0), P(0, 8), P(x, y), style, 3, 5))
    End Sub

End Class
