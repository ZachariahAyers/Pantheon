Namespace Workframes

    ''' <summary>Letters for endwall grid lines: A to Z, then AA, AB and on.</summary>
    Public Module GridLabels

        Public Function Letter(position As Integer) As String
            If position < 1 Then
                Throw New ArgumentException("Grid positions start at 1.", NameOf(position))
            End If

            Dim label = ""
            Dim remaining = position
            Do While remaining > 0
                remaining -= 1
                label = ChrW(AscW("A"c) + remaining Mod 26) & label
                remaining \= 26
            Loop
            Return label
        End Function

    End Module

End Namespace
