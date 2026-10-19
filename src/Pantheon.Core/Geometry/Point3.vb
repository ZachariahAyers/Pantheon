Imports System.Globalization

Namespace Geometry

    ''' <summary>A point in model space, in drawing units.</summary>
    Public Structure Point3
        Implements IEquatable(Of Point3)

        Public Sub New(x As Double, y As Double, z As Double)
            Me.X = x
            Me.Y = y
            Me.Z = z
        End Sub

        Public ReadOnly Property X As Double

        Public ReadOnly Property Y As Double

        Public ReadOnly Property Z As Double

        Public Shared Operator =(left As Point3, right As Point3) As Boolean
            Return left.Equals(right)
        End Operator

        Public Shared Operator <>(left As Point3, right As Point3) As Boolean
            Return Not left.Equals(right)
        End Operator

        Public Overloads Function Equals(other As Point3) As Boolean Implements IEquatable(Of Point3).Equals
            Return X.Equals(other.X) AndAlso Y.Equals(other.Y) AndAlso Z.Equals(other.Z)
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Return TypeOf obj Is Point3 AndAlso Equals(DirectCast(obj, Point3))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return HashCode.Combine(X, Y, Z)
        End Function

        Public Overrides Function ToString() As String
            Return "(" & X.ToString("G", CultureInfo.InvariantCulture) & ", " &
                Y.ToString("G", CultureInfo.InvariantCulture) & ", " &
                Z.ToString("G", CultureInfo.InvariantCulture) & ")"
        End Function

    End Structure

End Namespace
