Namespace Geometry

    ''' <summary>An ordered run of vertices for a polyline, open or closed.</summary>
    Public NotInheritable Class Outline2

        Public Sub New(points As IEnumerable(Of Point2), isClosed As Boolean)
            ArgumentNullException.ThrowIfNull(points, NameOf(points))
            Me.Points = points.ToArray()
            Me.IsClosed = isClosed
        End Sub

        Public ReadOnly Property Points As IReadOnlyList(Of Point2)

        Public ReadOnly Property IsClosed As Boolean

        ''' <summary>Returns the outline moved so its origin lands on <paramref name="origin"/>.</summary>
        Public Function MovedTo(origin As Point2) As Outline2
            Return New Outline2(Points.Select(Function(p) p + origin), IsClosed)
        End Function

    End Class

End Namespace
