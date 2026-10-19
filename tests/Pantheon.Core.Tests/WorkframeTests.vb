Imports Pantheon.Core.Geometry
Imports Pantheon.Core.Workframes
Imports Xunit

Public NotInheritable Class WorkframeTests

    Private Const TextHeight As Double = 6

    ' 40 ft wide, 75 ft long, 16 ft eave, 1:12, 8 inch purlins and girts, three 25 ft bays.
    Private Shared Function Gable(attachment As GirtAttachment, Optional sideBays As Double() = Nothing) As SymmetricalWorkframe
        Return New SymmetricalWorkframe(480, 900, 192, 1.0 / 12, New Framing(8, 8, attachment),
                                        New BayLayout(If(sideBays, {300.0, 300.0, 300.0}), Array.Empty(Of Double)()))
    End Function

    Private Shared Function LinesOf(geometry As WorkframeGeometry, kind As LineKind) As WorkframeLine()
        Return geometry.Lines.Where(Function(line) line.Kind = kind).ToArray()
    End Function

    Private Shared Function AtX(lines As IEnumerable(Of WorkframeLine), x As Double) As WorkframeLine()
        Return lines.Where(Function(line) Math.Abs(line.StartPoint.X - x) < 0.000001 AndAlso Math.Abs(line.EndPoint.X - x) < 0.000001).ToArray()
    End Function

    <Fact>
    Public Sub GableDrawsOneFrameAtEachGridLine()
        Dim frames = LinesOf(Gable(GirtAttachment.Flush).Build(TextHeight), LineKind.Frame)
        Assert.Equal(16, frames.Length)
        For Each x In {0.0, 300.0, 600.0, 900.0}
            Assert.Equal(4, AtX(frames, x).Length)
        Next
    End Sub

    <Fact>
    Public Sub GableNumbersEveryFrameAndLettersBothSidewalls()
        Dim labels = Gable(GirtAttachment.Flush).Build(TextHeight).Bubbles.Select(Function(bubble) bubble.Label).ToArray()
        SameLabels({"1", "2", "3", "4", "A", "B"}, labels)
    End Sub

    <Fact>
    Public Sub GridBubblesSitPastTheBuildingByTwoTextHeights()
        Dim bubbles = Gable(GirtAttachment.Flush).Build(TextHeight).Bubbles
        SamePoint(New Point3(300, 480 + 12 + 4.5, 0), bubbles(1).Center)
        SamePoint(New Point3(-12 - 4.5, 480, 0), bubbles(4).Center)
        Assert.Equal(4.5, bubbles(0).Radius, 9)
    End Sub

    <Fact>
    Public Sub FlushPurlinLineDropsSquareToTheRoof()
        Dim drop = 8 * Math.Sqrt(1 + 1.0 / 144)
        Dim offsets = AtX(LinesOf(Gable(GirtAttachment.Flush).Build(TextHeight), LineKind.Offset), 0)
        Assert.Equal(2, offsets.Length)
        SamePoint(New Point3(0, 480, 192 - drop), offsets(0).StartPoint)
        SamePoint(New Point3(0, 240, 212 - drop), offsets(0).EndPoint)
        SamePoint(New Point3(0, 0, 192 - drop), offsets(1).EndPoint)
    End Sub

    <Fact>
    Public Sub BypassGirtsPullThePurlinLineInsideTheWalls()
        Dim drop = 8 * Math.Sqrt(1 + 1.0 / 144)
        Dim offsets = AtX(LinesOf(Gable(GirtAttachment.Bypass).Build(TextHeight), LineKind.Offset), 0)
        Assert.Equal(4, offsets.Length)
        SamePoint(New Point3(0, 472, 192 + 8.0 / 12 - drop), offsets(0).StartPoint)
        SamePoint(New Point3(0, 472, 0), offsets(2).StartPoint)
    End Sub

    <Fact>
    Public Sub BaysLongerThanTheBuildingAreRejected()
        Assert.Throws(Of ArgumentException)(Function() Gable(GirtAttachment.Flush, {300.0, 300.0, 400.0}).Build(TextHeight))
    End Sub

    <Fact>
    Public Sub NoBaysStillDrawsBothEndwalls()
        Dim geometry = Gable(GirtAttachment.Flush, Array.Empty(Of Double)()).Build(TextHeight)
        Assert.Equal(8, LinesOf(geometry, LineKind.Frame).Length)
        SameLabels({"1", "2", "A", "B"}, geometry.Bubbles.Select(Function(bubble) bubble.Label).ToArray())
    End Sub

    <Fact>
    Public Sub SingleSlopeTakesItsPitchFromTheEaves()
        Dim building = New SingleSlopeWorkframe(360, 600, 144, 174, New Framing(8, 8, GirtAttachment.Flush), BayLayout.None)
        Assert.Equal(30.0 / 360, building.Pitch, 9)
    End Sub

    <Fact>
    Public Sub SingleSlopeBypassDropsTheHighEndBelowTheRoof()
        ' The girt rise is not added at the high eave, so the line stays below the roof.
        Dim pitch = 30.0 / 360
        Dim drop = 8 * Math.Sqrt(1 + pitch * pitch)
        Dim building = New SingleSlopeWorkframe(360, 600, 144, 174, New Framing(8, 8, GirtAttachment.Bypass), BayLayout.None)
        Dim purlin = AtX(LinesOf(building.Build(TextHeight), LineKind.Offset), 0)(0)
        SamePoint(New Point3(0, 352, 144 + 8 * pitch - drop), purlin.StartPoint)
        SamePoint(New Point3(0, 8, 174 - 8 * pitch - drop), purlin.EndPoint)
    End Sub

    <Fact>
    Public Sub OffsetRidgePlacesTheFarSideBeyondTheRidge()
        Dim near = New RoofSide(240, 600, 168, 2.0 / 12, New Framing(8, 8, GirtAttachment.Flush))
        Dim far = New RoofSide(240, 900, 168, 2.0 / 12, New Framing(8, 8, GirtAttachment.Flush))
        Dim geometry = New OffsetRidgeWorkframe(far, near, New BayLayout({300.0, 300.0, 300.0}, Array.Empty(Of Double)())).Build(TextHeight)

        Dim farEaveColumn = geometry.Lines.Where(Function(line) line.Kind = LineKind.Frame AndAlso
                                                       line.StartPoint = New Point3(900, 480, 0)).ToArray()
        Assert.Single(farEaveColumn)
        SamePoint(New Point3(900, 480, 168), farEaveColumn(0).EndPoint)
        Assert.Empty(AtX(LinesOf(geometry, LineKind.Frame), 900).Where(Function(line) line.StartPoint.Y < 240))
    End Sub

    <Fact>
    Public Sub OffsetRidgeNumbersEveryFrameOfEitherSide()
        Dim near = New RoofSide(240, 600, 168, 2.0 / 12, New Framing(8, 8, GirtAttachment.Flush))
        Dim far = New RoofSide(240, 900, 168, 2.0 / 12, New Framing(8, 8, GirtAttachment.Flush))
        Dim geometry = New OffsetRidgeWorkframe(far, near, New BayLayout({300.0, 300.0, 300.0}, Array.Empty(Of Double)())).Build(TextHeight)
        Dim numbers = geometry.Bubbles.Where(Function(bubble) Char.IsDigit(bubble.Label(0))).Select(Function(bubble) bubble.Center.X).ToArray()
        SameValues({0.0, 300.0, 600.0, 900.0}, numbers)
    End Sub

End Class
