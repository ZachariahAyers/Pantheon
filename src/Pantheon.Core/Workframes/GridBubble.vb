Imports Pantheon.Core.Geometry

Namespace Workframes

    ''' <summary>The circle and label at the end of a grid line leader. The leader itself is an annotation line.</summary>
    Public NotInheritable Class GridBubble

        Public Sub New(label As String, center As Point3, radius As Double)
            ArgumentException.ThrowIfNullOrEmpty(label, NameOf(label))
            Me.Label = label
            Me.Center = center
            Me.Radius = radius
        End Sub

        Public ReadOnly Property Label As String

        Public ReadOnly Property Center As Point3

        Public ReadOnly Property Radius As Double

    End Class

End Namespace
