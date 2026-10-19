Imports Pantheon.Core.Geometry

Namespace Members

    ''' <summary>A rectangular foundation slab, optionally with a notch (brick ledge) around its edge.</summary>
    Public NotInheritable Class Slab

        ''' <param name="notchDepth">Width of the notch outside the slab edge; zero for none.</param>
        Public Sub New(width As Double, length As Double, notchDepth As Double)
            Guard.Positive(width, NameOf(width))
            Guard.Positive(length, NameOf(length))
            Guard.NotNegative(notchDepth, NameOf(notchDepth))
            Me.Width = width
            Me.Length = length
            Me.NotchDepth = notchDepth
        End Sub

        Public ReadOnly Property Width As Double

        Public ReadOnly Property Length As Double

        Public ReadOnly Property NotchDepth As Double

        Public ReadOnly Property HasNotch As Boolean
            Get
                Return NotchDepth > 0
            End Get
        End Property

        ''' <summary>The slab edge, from the bottom-left corner at the origin.</summary>
        Public Function Outline() As Outline2
            Return Rectangle(0, Width, Length)
        End Function

        ''' <summary>The notch line, <see cref="NotchDepth"/> outside the slab edge on every side.</summary>
        Public Function NotchOutline() As Outline2
            If Not HasNotch Then
                Throw New InvalidOperationException("This slab has no notch.")
            End If
            Return Rectangle(NotchDepth, Width, Length)
        End Function

        Private Shared Function Rectangle(margin As Double, width As Double, length As Double) As Outline2
            Return New Outline2({
                New Point2(-margin, -margin),
                New Point2(-margin, width + margin),
                New Point2(length + margin, width + margin),
                New Point2(length + margin, -margin)}, isClosed:=True)
        End Function

    End Class

End Namespace
