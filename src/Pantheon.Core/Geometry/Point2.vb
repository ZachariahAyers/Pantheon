Imports System.Globalization

Namespace Geometry

    ''' <summary>A point or offset in the drawing plane, in drawing units.</summary>
    Public Structure Point2
        Implements IEquatable(Of Point2)

        Public Sub New(x As Double, y As Double)
            Me.X = x
            Me.Y = y
        End Sub

        Public ReadOnly Property X As Double

        Public ReadOnly Property Y As Double

        ''' <summary>Rotates the point about the origin, counterclockwise for a positive angle.</summary>
        Public Function Rotate(angleRadians As Double) As Point2
            Dim cos = Math.Cos(angleRadians)
            Dim sin = Math.Sin(angleRadians)
            Return New Point2(X * cos - Y * sin, X * sin + Y * cos)
        End Function

        ''' <summary>Reflects the point across the Y axis.</summary>
        Public Function MirrorX() As Point2
            Return New Point2(-X, Y)
        End Function

        ''' <summary>Reflects the point across the X axis.</summary>
        Public Function MirrorY() As Point2
            Return New Point2(X, -Y)
        End Function

        Public Shared Operator +(left As Point2, right As Point2) As Point2
            Return New Point2(left.X + right.X, left.Y + right.Y)
        End Operator

        Public Shared Operator -(left As Point2, right As Point2) As Point2
            Return New Point2(left.X - right.X, left.Y - right.Y)
        End Operator

        Public Shared Operator =(left As Point2, right As Point2) As Boolean
            Return left.Equals(right)
        End Operator

        Public Shared Operator <>(left As Point2, right As Point2) As Boolean
            Return Not left.Equals(right)
        End Operator

        Public Overloads Function Equals(other As Point2) As Boolean Implements IEquatable(Of Point2).Equals
            Return X.Equals(other.X) AndAlso Y.Equals(other.Y)
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Return TypeOf obj Is Point2 AndAlso Equals(DirectCast(obj, Point2))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return HashCode.Combine(X, Y)
        End Function

        Public Overrides Function ToString() As String
            Return "(" & X.ToString("G", CultureInfo.InvariantCulture) & ", " & Y.ToString("G", CultureInfo.InvariantCulture) & ")"
        End Function

    End Structure

End Namespace
