Imports Pantheon.Core.Geometry
Imports Pantheon.Core.Roofs

Namespace Workframes

    ''' <summary>A gable building: equal eaves and a ridge on the centre line.</summary>
    Public NotInheritable Class SymmetricalWorkframe
        Implements IWorkframe

        Public Sub New(width As Double, length As Double, eaveHeight As Double, pitch As Double,
                       framing As Framing, bays As BayLayout)
            Guard.Positive(width, NameOf(width))
            Guard.Positive(length, NameOf(length))
            Guard.Positive(eaveHeight, NameOf(eaveHeight))
            Guard.NotNegative(pitch, NameOf(pitch))
            ArgumentNullException.ThrowIfNull(framing, NameOf(framing))
            ArgumentNullException.ThrowIfNull(bays, NameOf(bays))
            Me.Width = width
            Me.Length = length
            Me.EaveHeight = eaveHeight
            Me.Pitch = pitch
            Me.Framing = framing
            Me.Bays = bays
        End Sub

        Public ReadOnly Property Width As Double

        Public ReadOnly Property Length As Double

        Public ReadOnly Property EaveHeight As Double

        ''' <summary>Roof pitch as rise over run.</summary>
        Public ReadOnly Property Pitch As Double

        Public ReadOnly Property Framing As Framing

        Public ReadOnly Property Bays As BayLayout

        Public ReadOnly Property RidgeHeight As Double
            Get
                Return EaveHeight + RightTriangle.FromRun(Width / 2, Pitch).Rise
            End Get
        End Property

        Public Function Build(textHeight As Double) As WorkframeGeometry Implements IWorkframe.Build
            Dim profile = New FrameProfile({
                New Point2(Width, EaveHeight),
                New Point2(Width / 2, RidgeHeight),
                New Point2(0, EaveHeight)}, Framing)
            Return WorkframeBuilder.Rectangular(Width, Length, profile, Bays, textHeight)
        End Function

    End Class

End Namespace
