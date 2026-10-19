Imports Pantheon.Core.Geometry
Imports Pantheon.Core.Roofs

Namespace Workframes

    ''' <summary>One side of an off-centre ridge building: its own width, length, eave, pitch and framing.</summary>
    Public NotInheritable Class RoofSide

        Public Sub New(width As Double, length As Double, eaveHeight As Double, pitch As Double, framing As Framing)
            Guard.Positive(width, NameOf(width))
            Guard.Positive(length, NameOf(length))
            Guard.Positive(eaveHeight, NameOf(eaveHeight))
            Guard.NotNegative(pitch, NameOf(pitch))
            ArgumentNullException.ThrowIfNull(framing, NameOf(framing))
            Me.Width = width
            Me.Length = length
            Me.EaveHeight = eaveHeight
            Me.Pitch = pitch
            Me.Framing = framing
        End Sub

        Public ReadOnly Property Width As Double

        Public ReadOnly Property Length As Double

        Public ReadOnly Property EaveHeight As Double

        ''' <summary>Roof pitch as rise over run, rising from the eave to the ridge.</summary>
        Public ReadOnly Property Pitch As Double

        Public ReadOnly Property Framing As Framing

        Public ReadOnly Property RidgeHeight As Double
            Get
                Return EaveHeight + RightTriangle.FromRun(Width, Pitch).Rise
            End Get
        End Property

    End Class

End Namespace
