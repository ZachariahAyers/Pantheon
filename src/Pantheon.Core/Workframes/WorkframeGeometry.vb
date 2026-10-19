Namespace Workframes

    ''' <summary>Everything a workframe draws, relative to its bottom-left insertion point.</summary>
    Public NotInheritable Class WorkframeGeometry

        Friend Sub New(lines As List(Of WorkframeLine), bubbles As List(Of GridBubble))
            Me.Lines = lines.ToArray()
            Me.Bubbles = bubbles.ToArray()
        End Sub

        Public ReadOnly Property Lines As IReadOnlyList(Of WorkframeLine)

        Public ReadOnly Property Bubbles As IReadOnlyList(Of GridBubble)

    End Class

End Namespace
