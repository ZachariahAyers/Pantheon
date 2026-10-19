Imports Pantheon.Core.Geometry

Namespace Workframes

    ''' <summary>A single slope building: a low eave on the far side and a high eave on the near side.</summary>
    Public NotInheritable Class SingleSlopeWorkframe
        Implements IWorkframe

        Public Sub New(width As Double, length As Double, lowEaveHeight As Double, highEaveHeight As Double,
                       framing As Framing, bays As BayLayout)
            Guard.Positive(width, NameOf(width))
            Guard.Positive(length, NameOf(length))
            Guard.Positive(lowEaveHeight, NameOf(lowEaveHeight))
            Guard.Positive(highEaveHeight, NameOf(highEaveHeight))
            If highEaveHeight < lowEaveHeight Then
                Throw New ArgumentException("The high eave must be at least as tall as the low eave.", NameOf(highEaveHeight))
            End If
            ArgumentNullException.ThrowIfNull(framing, NameOf(framing))
            ArgumentNullException.ThrowIfNull(bays, NameOf(bays))
            Me.Width = width
            Me.Length = length
            Me.LowEaveHeight = lowEaveHeight
            Me.HighEaveHeight = highEaveHeight
            Me.Framing = framing
            Me.Bays = bays
        End Sub

        Public ReadOnly Property Width As Double

        Public ReadOnly Property Length As Double

        Public ReadOnly Property LowEaveHeight As Double

        Public ReadOnly Property HighEaveHeight As Double

        Public ReadOnly Property Framing As Framing

        Public ReadOnly Property Bays As BayLayout

        ''' <summary>Roof pitch as rise over run, set by the two eave heights.</summary>
        Public ReadOnly Property Pitch As Double
            Get
                Return (HighEaveHeight - LowEaveHeight) / Width
            End Get
        End Property

        Public Function Build(textHeight As Double) As WorkframeGeometry Implements IWorkframe.Build
            Dim profile = New FrameProfile({
                New Point2(Width, LowEaveHeight),
                New Point2(0, HighEaveHeight)}, Framing)
            Return WorkframeBuilder.Rectangular(Width, Length, profile, Bays, textHeight)
        End Function

    End Class

End Namespace
