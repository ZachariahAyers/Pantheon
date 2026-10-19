Imports Pantheon.Core.Geometry

Namespace Members

    ''' <summary>
    ''' A straight column's base plate with four anchor holes. The plate runs the full depth of the
    ''' column (outer flange, web, inner flange) and is centred on the origin.
    ''' </summary>
    Public NotInheritable Class BasePlate

        ''' <summary>Centre-to-centre spacing of the two hole columns, across the plate.</summary>
        Public Const AnchorGauge As Double = 4

        Public Sub New(width As Double, thickness As Double, webDepth As Double,
                       outerFlangeThickness As Double, innerFlangeThickness As Double,
                       anchorDiameter As Double, anchorFromOuterFlange As Double, anchorSpacing As Double)
            Guard.Positive(width, NameOf(width))
            Guard.Positive(thickness, NameOf(thickness))
            Guard.Positive(webDepth, NameOf(webDepth))
            Guard.Positive(outerFlangeThickness, NameOf(outerFlangeThickness))
            Guard.Positive(innerFlangeThickness, NameOf(innerFlangeThickness))
            Guard.Positive(anchorDiameter, NameOf(anchorDiameter))
            Guard.NotNegative(anchorFromOuterFlange, NameOf(anchorFromOuterFlange))
            Guard.NotNegative(anchorSpacing, NameOf(anchorSpacing))

            Me.Width = width
            Me.Thickness = thickness
            Me.Length = webDepth + outerFlangeThickness + innerFlangeThickness
            Me.HoleDiameter = anchorDiameter

            Dim firstRow = Me.Length / 2 - anchorFromOuterFlange
            Dim secondRow = firstRow - anchorSpacing
            Dim half = AnchorGauge / 2
            Holes = {New Point2(-half, firstRow), New Point2(half, firstRow),
                     New Point2(-half, secondRow), New Point2(half, secondRow)}

            Dim radius = anchorDiameter / 2
            For Each hole In Holes
                If Math.Abs(hole.X) + radius > width / 2 OrElse Math.Abs(hole.Y) + radius > Me.Length / 2 Then
                    Throw New ArgumentException("The anchor holes fall outside a " & Guard.Format(width) & " by " &
                                                Guard.Format(Me.Length) & " plate. Check the anchor spacing and plate width.", NameOf(anchorSpacing))
                End If
            Next
        End Sub

        ''' <summary>Across the column flanges.</summary>
        Public ReadOnly Property Width As Double

        ''' <summary>Along the column depth.</summary>
        Public ReadOnly Property Length As Double

        Public ReadOnly Property Thickness As Double

        Public ReadOnly Property HoleDiameter As Double

        ''' <summary>Hole centres relative to the plate centre.</summary>
        Public ReadOnly Property Holes As IReadOnlyList(Of Point2)

    End Class

End Namespace
