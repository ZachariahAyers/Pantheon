Namespace Drawing

    ''' <summary>A layer Pantheon creates on first use and puts a kind of member on.</summary>
    Friend NotInheritable Class MemberLayer

        Private Sub New(name As String, description As String, colorIndex As Short)
            Me.Name = name
            Me.Description = description
            Me.ColorIndex = colorIndex
        End Sub

        Friend Shared ReadOnly Property Workframes As New MemberLayer("WorkFrames", "Pantheon workframes", 3)

        Friend Shared ReadOnly Property Foundations As New MemberLayer("SLAB", "Pantheon foundation slabs", 8)

        Friend Shared ReadOnly Property Plates As New MemberLayer("Plates", "Pantheon base plates", 2)

        Friend ReadOnly Property Name As String

        Friend ReadOnly Property Description As String

        Friend ReadOnly Property ColorIndex As Short

    End Class

End Namespace
