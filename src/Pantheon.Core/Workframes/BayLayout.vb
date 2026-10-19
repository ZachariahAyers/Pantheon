Namespace Workframes

    ''' <summary>
    ''' Bay sizes along the length (sidewall bays, numbered grid lines) and across the
    ''' width (endwall bays, lettered grid lines). Either list may be empty.
    ''' </summary>
    Public NotInheritable Class BayLayout

        Public Sub New(sideBays As IEnumerable(Of Double), endBays As IEnumerable(Of Double))
            ArgumentNullException.ThrowIfNull(sideBays, NameOf(sideBays))
            ArgumentNullException.ThrowIfNull(endBays, NameOf(endBays))
            Me.SideBays = sideBays.ToArray()
            Me.EndBays = endBays.ToArray()
        End Sub

        Public Shared ReadOnly Property None As New BayLayout(Array.Empty(Of Double)(), Array.Empty(Of Double)())

        Public ReadOnly Property SideBays As IReadOnlyList(Of Double)

        Public ReadOnly Property EndBays As IReadOnlyList(Of Double)

    End Class

End Namespace
