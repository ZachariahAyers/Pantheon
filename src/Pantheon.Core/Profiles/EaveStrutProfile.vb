Imports Pantheon.Core.Geometry
Imports Pantheon.Core.Roofs

Namespace Profiles

    ''' <summary>
    ''' Eave strut shapes. Double slope struts follow the roof with both flanges;
    ''' single slope struts keep the bottom flange level. Up and down name the roof direction.
    ''' </summary>
    Public Enum EaveStrutStyle
        DoubleSlopeUp
        DoubleSlopeDown
        SingleSlopeUp
        SingleSlopeDown
    End Enum

    ''' <summary>The eave strut drawn from three picked points: web bottom, web top and a point on the roof line.</summary>
    Public Module EaveStrutProfile

        Public Const DefaultTopFlange As Double = 3

        Public Const DefaultBottomFlange As Double = 5

        Public Const LipLength As Double = 1

        ''' <summary>
        ''' The open outline from the bottom lip to the top lip. The roof pitch is read from the line
        ''' between the top of the web and <paramref name="pitchPoint"/>; its direction must agree with the style.
        ''' </summary>
        Public Function Outline(bottomOfWeb As Point2, topOfWeb As Point2, pitchPoint As Point2,
                                style As EaveStrutStyle, topFlange As Double, bottomFlange As Double) As Outline2
            Guard.Positive(topFlange, NameOf(topFlange))
            Guard.Positive(bottomFlange, NameOf(bottomFlange))

            Dim run = pitchPoint.X - topOfWeb.X
            Dim rise = pitchPoint.Y - topOfWeb.Y
            If run = 0 Then
                Throw New ArgumentException("Pick the pitch point to one side of the top of the web, not directly above or below it.", NameOf(pitchPoint))
            End If

            Dim slopesUp = style = EaveStrutStyle.DoubleSlopeUp OrElse style = EaveStrutStyle.SingleSlopeUp
            If slopesUp AndAlso rise < 0 Then
                Throw New ArgumentException("This strut slopes up: pick the pitch point above the top of the web.", NameOf(pitchPoint))
            End If
            If Not slopesUp AndAlso rise > 0 Then
                Throw New ArgumentException("This strut slopes down: pick the pitch point below the top of the web.", NameOf(pitchPoint))
            End If

            Dim pitch = Math.Abs(rise) / Math.Abs(run)
            Dim across = Math.Sign(run)
            Dim vertical = If(slopesUp, 1, -1)

            Dim top = RightTriangle.FromSlope(topFlange, pitch)
            Dim topEnd = topOfWeb + New Point2(across * top.Run, vertical * top.Rise)

            Dim bottomEnd As Point2
            If style = EaveStrutStyle.DoubleSlopeUp OrElse style = EaveStrutStyle.DoubleSlopeDown Then
                Dim bottom = RightTriangle.FromSlope(bottomFlange, pitch)
                bottomEnd = bottomOfWeb + New Point2(across * bottom.Run, vertical * bottom.Rise)
            Else
                bottomEnd = bottomOfWeb + New Point2(across * bottomFlange, 0)
            End If

            Return New Outline2({
                bottomEnd + New Point2(0, LipLength),
                bottomEnd,
                bottomOfWeb,
                topOfWeb,
                topEnd,
                topEnd - New Point2(0, LipLength)}, isClosed:=False)
        End Function

    End Module

End Namespace
