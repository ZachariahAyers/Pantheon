Namespace Members

    ''' <summary>The anchor rods offered for a column base plate, by catalogue label.</summary>
    Public Module AnchorRods

        Private ReadOnly Catalogue As (Label As String, Diameter As Double)() = {
            ("1/2"" x 8""", 0.5),
            ("5/8"" x 12""", 0.625),
            ("3/4"" x 15""", 0.75),
            ("5/8"" x 6"" Titen HD", 0.625),
            ("5/8"" x 8"" Titen HD", 0.625)}

        Public ReadOnly Property Labels As IReadOnlyList(Of String) = Catalogue.Select(Function(rod) rod.Label).ToArray()

        Public Function DiameterOf(label As String) As Double
            For Each rod In Catalogue
                If String.Equals(rod.Label, label, StringComparison.Ordinal) Then
                    Return rod.Diameter
                End If
            Next
            Throw New ArgumentException("""" & label & """ is not a listed anchor rod.", NameOf(label))
        End Function

    End Module

End Namespace
