Imports System.Globalization
Imports Pantheon.Core.Geometry

Namespace Workframes

    ''' <summary>Collects workframe lines, dropping exact repeats, and places grid bubbles.</summary>
    Friend NotInheritable Class WorkframeBuilder

        ''' <summary>Leader length beyond the building, in text heights.</summary>
        Private Const LeaderReach As Double = 2

        ''' <summary>Bubble diameter, in text heights.</summary>
        Private Const BubbleSize As Double = 1.5

        Private ReadOnly _lines As New List(Of WorkframeLine)
        Private ReadOnly _keys As New HashSet(Of String)(StringComparer.Ordinal)
        Private ReadOnly _bubbles As New List(Of GridBubble)
        Private ReadOnly _textHeight As Double

        Friend Sub New(textHeight As Double)
            Guard.Positive(textHeight, NameOf(textHeight))
            _textHeight = textHeight
        End Sub

        Friend Sub AddLine(line As WorkframeLine)
            If _keys.Add(line.Key()) Then
                _lines.Add(line)
            End If
        End Sub

        ''' <summary>The footprint rectangle from y = <paramref name="fromY"/> to <paramref name="toY"/>.</summary>
        Friend Sub AddBase(fromY As Double, toY As Double, length As Double)
            AddLine(Base(0, fromY, length, fromY))
            AddLine(Base(0, fromY, 0, toY))
            AddLine(Base(0, toY, length, toY))
            AddLine(Base(length, toY, length, fromY))
        End Sub

        Friend Sub AddFrames(profile As FrameProfile, positions As List(Of Double))
            For Each x In positions
                For Each line In profile.LinesAt(x)
                    AddLine(line)
                Next
            Next
        End Sub

        ''' <summary>A numbered sidewall grid line at <paramref name="x"/>, labelled past the far side at <paramref name="outerY"/>.</summary>
        Friend Sub AddNumberedLine(number As Integer, x As Double, fromY As Double, outerY As Double)
            Dim anchor = New Point3(x, outerY + LeaderReach * _textHeight, 0)
            Dim center = New Point3(x, anchor.Y + Radius, 0)
            AddBubble(number.ToString(CultureInfo.InvariantCulture), center, New Point3(x, fromY, 0), anchor)
        End Sub

        ''' <summary>A lettered endwall grid line at <paramref name="y"/>, labelled in front of the first frame.</summary>
        Friend Sub AddLetteredLine(position As Integer, y As Double, fromX As Double)
            Dim anchor = New Point3(-LeaderReach * _textHeight, y, 0)
            Dim center = New Point3(anchor.X - Radius, y, 0)
            AddBubble(GridLabels.Letter(position), center, New Point3(fromX, y, 0), anchor)
        End Sub

        ''' <summary>
        ''' Lays out a rectangular building of one frame shape: footprint, a frame at every sidewall
        ''' grid line, then numbered and lettered grid lines.
        ''' </summary>
        Friend Shared Function Rectangular(width As Double, length As Double, profile As FrameProfile,
                                           bays As BayLayout, textHeight As Double) As WorkframeGeometry
            Dim frames = GridLayout.Lines(bays.SideBays, length, "length")
            Dim across = GridLayout.Lines(bays.EndBays, width, "width")

            Dim builder = New WorkframeBuilder(textHeight)
            builder.AddBase(0, width, length)
            builder.AddFrames(profile, frames)
            For i = 0 To frames.Count - 1
                builder.AddNumberedLine(i + 1, frames(i), 0, width)
            Next
            For i = 0 To across.Count - 1
                builder.AddLetteredLine(i + 1, width - across(i), length)
            Next
            Return builder.Build()
        End Function

        Friend Function Build() As WorkframeGeometry
            Return New WorkframeGeometry(_lines, _bubbles)
        End Function

        Private ReadOnly Property Radius As Double
            Get
                Return BubbleSize * _textHeight / 2
            End Get
        End Property

        Private Sub AddBubble(label As String, center As Point3, leaderStart As Point3, leaderEnd As Point3)
            AddLine(New WorkframeLine(leaderStart, leaderEnd, LineKind.Annotation))
            _bubbles.Add(New GridBubble(label, center, Radius))
        End Sub

        Private Shared Function Base(x1 As Double, y1 As Double, x2 As Double, y2 As Double) As WorkframeLine
            Return New WorkframeLine(New Point3(x1, y1, 0), New Point3(x2, y2, 0), LineKind.Base)
        End Function

    End Class

End Namespace
