Imports Pantheon.Core.Geometry

Namespace Workframes

    ''' <summary>
    ''' One frame line seen from the end: a column at each side and a roof between them.
    ''' Roof vertices are (y across the building, z height) from the first wall to the second.
    ''' </summary>
    Friend NotInheritable Class FrameProfile

        Private ReadOnly _roof As Point2()
        Private ReadOnly _framing As Framing

        Friend Sub New(roof As Point2(), framing As Framing)
            If roof.Length < 2 Then
                Throw New ArgumentException("A frame needs at least two roof points.", NameOf(roof))
            End If
            For i = 0 To roof.Length - 2
                If roof(i).X = roof(i + 1).X Then
                    Throw New ArgumentException("Each roof segment must span some width.", NameOf(roof))
                End If
            Next
            For Each point In roof
                Guard.Positive(point.Y, "Eave and ridge heights")
            Next
            If framing.Attachment = GirtAttachment.Bypass AndAlso
               (framing.GirtDepth >= Math.Abs(roof(1).X - roof(0).X) OrElse
                framing.GirtDepth >= Math.Abs(roof(roof.Length - 1).X - roof(roof.Length - 2).X)) Then
                Throw New ArgumentException("The girt depth must be less than the width of the end roof segments.", NameOf(framing))
            End If

            _roof = roof
            _framing = framing
        End Sub

        Friend Iterator Function LinesAt(x As Double) As IEnumerable(Of WorkframeLine)
            Dim first = _roof(0)
            Dim last = _roof(_roof.Length - 1)

            Yield Frame(x, New Point2(first.X, 0), first)
            For i = 0 To _roof.Length - 2
                Yield Frame(x, _roof(i), _roof(i + 1))
            Next
            Yield Frame(x, last, New Point2(last.X, 0))

            Dim purlin = PurlinLine()
            For i = 0 To purlin.Length - 2
                Yield Offset(x, purlin(i), purlin(i + 1))
            Next

            If _framing.Attachment = GirtAttachment.Bypass Then
                Yield Offset(x, New Point2(purlin(0).X, 0), purlin(0))
                Yield Offset(x, New Point2(purlin(purlin.Length - 1).X, 0), purlin(purlin.Length - 1))
            End If
        End Function

        ''' <summary>
        ''' The underside of the purlins: each roof segment dropped by the purlin depth measured square
        ''' to the roof, joined where neighbouring segments meet. With bypass girts the line starts and
        ''' ends a girt depth inside each wall, on the roof slope.
        ''' </summary>
        Private Function PurlinLine() As Point2()
            Dim count = _roof.Length
            Dim slopes(count - 2) As Double
            Dim drops(count - 2) As Double
            For i = 0 To count - 2
                slopes(i) = (_roof(i + 1).Y - _roof(i).Y) / (_roof(i + 1).X - _roof(i).X)
                drops(i) = _framing.PurlinDepth * Math.Sqrt(1 + slopes(i) * slopes(i))
            Next

            Dim line(count - 1) As Point2
            line(0) = WallEnd(0, 1, slopes(0), drops(0))
            For i = 1 To count - 2
                line(i) = Corner(i, slopes(i - 1), drops(i - 1), slopes(i), drops(i))
            Next
            line(count - 1) = WallEnd(count - 1, count - 2, slopes(count - 2), drops(count - 2))
            Return line
        End Function

        Private Function WallEnd(wall As Integer, neighbour As Integer, slope As Double, drop As Double) As Point2
            Dim y = _roof(wall).X
            If _framing.Attachment = GirtAttachment.Bypass Then
                y += Math.Sign(_roof(neighbour).X - _roof(wall).X) * _framing.GirtDepth
            End If
            Return New Point2(y, _roof(wall).Y + (y - _roof(wall).X) * slope - drop)
        End Function

        Private Function Corner(vertex As Integer, slopeBefore As Double, dropBefore As Double,
                                slopeAfter As Double, dropAfter As Double) As Point2
            Dim apex = _roof(vertex)
            If slopeBefore = slopeAfter Then
                Return New Point2(apex.X, apex.Y - dropAfter)
            End If
            ' Both offset lines pass below the apex; solve z = apex.Y - drop + slope * (y - apex.X) for each.
            Dim y = apex.X + (dropAfter - dropBefore) / (slopeAfter - slopeBefore)
            Return New Point2(y, apex.Y - dropBefore + slopeBefore * (y - apex.X))
        End Function

        Private Shared Function Frame(x As Double, a As Point2, b As Point2) As WorkframeLine
            Return New WorkframeLine(New Point3(x, a.X, a.Y), New Point3(x, b.X, b.Y), LineKind.Frame)
        End Function

        Private Shared Function Offset(x As Double, a As Point2, b As Point2) As WorkframeLine
            Return New WorkframeLine(New Point3(x, a.X, a.Y), New Point3(x, b.X, b.Y), LineKind.Offset)
        End Function

    End Class

End Namespace
