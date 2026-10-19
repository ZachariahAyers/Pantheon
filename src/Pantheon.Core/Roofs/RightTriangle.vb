Namespace Roofs

    ''' <summary>
    ''' The run, rise and sloped length of a line laid at a roof pitch.
    ''' Pitch is the ratio rise over run, so 4:12 is 0.3333.
    ''' </summary>
    Public Structure RightTriangle

        Private Sub New(run As Double, rise As Double, slope As Double)
            Me.Run = run
            Me.Rise = rise
            Me.Slope = slope
        End Sub

        Public ReadOnly Property Run As Double

        Public ReadOnly Property Rise As Double

        ''' <summary>The sloped (hypotenuse) length.</summary>
        Public ReadOnly Property Slope As Double

        ''' <summary>A triangle with a known horizontal run.</summary>
        Public Shared Function FromRun(run As Double, pitch As Double) As RightTriangle
            Dim rise = run * pitch
            Return New RightTriangle(run, rise, Math.Sqrt(run * run + rise * rise))
        End Function

        ''' <summary>A triangle with a known sloped length. Rise is returned as a positive length.</summary>
        Public Shared Function FromSlope(slope As Double, pitch As Double) As RightTriangle
            Dim run = slope / Math.Sqrt(pitch * pitch + 1)
            Return New RightTriangle(run, Math.Sqrt(Math.Max(0, slope * slope - run * run)), slope)
        End Function

    End Structure

End Namespace
