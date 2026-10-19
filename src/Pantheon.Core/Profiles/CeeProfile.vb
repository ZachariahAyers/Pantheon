Imports Pantheon.Core.Geometry

Namespace Profiles

    ''' <summary>Which way the flanges of a section point from its web.</summary>
    Public Enum FlangeDirection
        Up
        Down
        Right
        Left
    End Enum

    ''' <summary>The cold-formed cee section drawn for jambs and posts.</summary>
    Public Module CeeProfile

        Public Const DefaultWeb As Double = 8

        Public Const JambFlange As Double = 2.5

        Public Const PostFlange As Double = 3.5

        Public Const LipLength As Double = 1

        Public Const WallThickness As Double = 0.25

        ''' <summary>
        ''' The closed outline of the section, centred on the middle of the web,
        ''' with the flanges pointing in <paramref name="direction"/>.
        ''' </summary>
        Public Function Outline(web As Double, flange As Double, direction As FlangeDirection) As Outline2
            Guard.Positive(web, NameOf(web))
            Guard.Positive(flange, NameOf(flange))
            If web <= 2 * LipLength Then
                Throw New ArgumentException("The web must be deeper than " & Guard.Format(2 * LipLength) & " so the lips do not overlap.", NameOf(web))
            End If
            If flange <= WallThickness Then
                Throw New ArgumentException("The flange must be longer than the wall thickness.", NameOf(flange))
            End If

            Dim half = web / 2
            Dim t = WallThickness
            Dim upward As Point2() = {
                New Point2(-half + LipLength, flange),
                New Point2(-half, flange),
                New Point2(-half, 0),
                New Point2(half, 0),
                New Point2(half, flange),
                New Point2(half - LipLength, flange),
                New Point2(half - LipLength, flange - t),
                New Point2(half - t, flange - t),
                New Point2(half - t, t),
                New Point2(-half + t, t),
                New Point2(-half + t, flange - t),
                New Point2(-half + LipLength, flange - t)}

            Return New Outline2(upward.Select(Function(p) Orient(p, direction)), isClosed:=True)
        End Function

        Private Function Orient(point As Point2, direction As FlangeDirection) As Point2
            Select Case direction
                Case FlangeDirection.Down
                    Return point.MirrorY()
                Case FlangeDirection.Right
                    Return New Point2(point.Y, -point.X)
                Case FlangeDirection.Left
                    Return New Point2(-point.Y, -point.X)
                Case Else
                    Return point
            End Select
        End Function

    End Module

End Namespace
