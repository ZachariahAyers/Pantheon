Imports System.Globalization

Namespace Roofs

    ''' <summary>Conversions for roof pitch, written by drafters as rise in inches per 12 of run.</summary>
    Public Module Pitch

        ''' <summary>Turns a rise per 12 (for example 4 for 4:12) into a rise over run ratio.</summary>
        Public Function FromRisePerTwelve(risePerTwelve As Double) As Double
            Return risePerTwelve / 12
        End Function

        ''' <summary>The angle of a pitch above horizontal, in degrees.</summary>
        Public Function AngleDegrees(ratio As Double) As Double
            Return Math.Atan(ratio) * 180 / Math.PI
        End Function

        ''' <summary>
        ''' The AutoCAD command string that rotates the UCS to a pitch, with the angle written
        ''' culture-invariant so the command line always reads a decimal point.
        ''' </summary>
        Public Function UcsRotationCommand(risePerTwelve As Double) As String
            Dim angle = AngleDegrees(FromRisePerTwelve(risePerTwelve))
            Return "_ucs _world _ucs _z " & angle.ToString("G7", CultureInfo.InvariantCulture) & " "
        End Function

    End Module

End Namespace
