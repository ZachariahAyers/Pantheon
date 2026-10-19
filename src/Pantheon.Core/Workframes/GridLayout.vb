Imports Pantheon.Core.Geometry

Namespace Workframes

    ''' <summary>Turns bay sizes into grid line positions.</summary>
    Friend Module GridLayout

        Friend Const Tolerance As Double = 0.000001

        ''' <summary>Positions from 0 to <paramref name="total"/>: both ends plus every bay line between them.</summary>
        Friend Function Lines(bays As IReadOnlyList(Of Double), total As Double, dimension As String) As List(Of Double)
            Dim positions = New List(Of Double) From {0}
            Dim running = 0.0
            For Each bay In bays
                running += bay
                If running > total + Tolerance Then
                    Throw New ArgumentException("The " & dimension & " bays add up to " & Guard.Format(running) &
                                                " but the building " & dimension & " is " & Guard.Format(total) & ".", NameOf(bays))
                End If
                If running < total - Tolerance Then
                    positions.Add(running)
                End If
            Next
            positions.Add(total)
            Return positions
        End Function

    End Module

End Namespace
