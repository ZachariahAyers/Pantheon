Imports Pantheon.Core.Geometry

Namespace Workframes

    ''' <summary>
    ''' A building whose ridge is off the centre line, laid out as two single slope frames back to back.
    ''' The near side runs from y = 0 to the ridge; the far side from the ridge to the far eave.
    ''' Each side keeps its own length, so the two can end at different grid lines.
    ''' </summary>
    Public NotInheritable Class OffsetRidgeWorkframe
        Implements IWorkframe

        Public Sub New(farSide As RoofSide, nearSide As RoofSide, bays As BayLayout)
            ArgumentNullException.ThrowIfNull(farSide, NameOf(farSide))
            ArgumentNullException.ThrowIfNull(nearSide, NameOf(nearSide))
            ArgumentNullException.ThrowIfNull(bays, NameOf(bays))
            Me.FarSide = farSide
            Me.NearSide = nearSide
            Me.Bays = bays
        End Sub

        ''' <summary>The side away from the insertion point (the left side looking at the front endwall).</summary>
        Public ReadOnly Property FarSide As RoofSide

        ''' <summary>The side at the insertion point (the right side looking at the front endwall).</summary>
        Public ReadOnly Property NearSide As RoofSide

        Public ReadOnly Property Bays As BayLayout

        Public Function Build(textHeight As Double) As WorkframeGeometry Implements IWorkframe.Build
            Dim ridge = NearSide.Width
            Dim width = NearSide.Width + FarSide.Width
            Dim longest = Math.Max(NearSide.Length, FarSide.Length)

            Dim nearProfile = New FrameProfile({
                New Point2(ridge, NearSide.RidgeHeight),
                New Point2(0, NearSide.EaveHeight)}, NearSide.Framing)
            Dim farProfile = New FrameProfile({
                New Point2(width, FarSide.EaveHeight),
                New Point2(ridge, FarSide.RidgeHeight)}, FarSide.Framing)

            Dim sidewallLines = GridLayout.Lines(Bays.SideBays, longest, "length")
            Dim endwallLines = GridLayout.Lines(Bays.EndBays, width, "width")

            Dim builder = New WorkframeBuilder(textHeight)
            builder.AddBase(0, ridge, NearSide.Length)
            builder.AddBase(ridge, width, FarSide.Length)
            builder.AddFrames(nearProfile, FramesWithin(sidewallLines, NearSide.Length))
            builder.AddFrames(farProfile, FramesWithin(sidewallLines, FarSide.Length))

            Dim ends = {NearSide.Length, FarSide.Length}
            Dim numbered = sidewallLines.Concat(ends).Select(Function(position) Math.Round(position, 6)).Distinct().OrderBy(Function(position) position).ToList()
            For i = 0 To numbered.Count - 1
                Dim x = numbered(i)
                builder.AddNumberedLine(i + 1, x, If(x <= NearSide.Length + GridLayout.Tolerance, 0, ridge), width)
            Next

            For i = 0 To endwallLines.Count - 1
                Dim y = width - endwallLines(i)
                Dim reach As Double
                If y > ridge + GridLayout.Tolerance Then
                    reach = FarSide.Length
                ElseIf y < ridge - GridLayout.Tolerance Then
                    reach = NearSide.Length
                Else
                    reach = longest
                End If
                builder.AddLetteredLine(i + 1, y, reach)
            Next

            Return builder.Build()
        End Function

        Private Shared Function FramesWithin(lines As List(Of Double), length As Double) As List(Of Double)
            Dim frames = lines.Where(Function(x) x < length - GridLayout.Tolerance).ToList()
            frames.Add(length)
            Return frames
        End Function

    End Class

End Namespace
