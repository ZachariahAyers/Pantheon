Imports System.Globalization

Namespace Geometry

    ''' <summary>Argument checks shared by the member models.</summary>
    Friend Module Guard

        Friend Sub Positive(value As Double, name As String)
            If Not (value > 0) OrElse Double.IsInfinity(value) Then
                Throw New ArgumentException(name & " must be greater than zero.", name)
            End If
        End Sub

        Friend Sub NotNegative(value As Double, name As String)
            If Not (value >= 0) OrElse Double.IsInfinity(value) Then
                Throw New ArgumentException(name & " must be zero or more.", name)
            End If
        End Sub

        Friend Sub Finite(value As Double, name As String)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                Throw New ArgumentException(name & " must be a number.", name)
            End If
        End Sub

        Friend Function Format(value As Double) As String
            Return value.ToString("0.###", CultureInfo.InvariantCulture)
        End Function

    End Module

End Namespace
