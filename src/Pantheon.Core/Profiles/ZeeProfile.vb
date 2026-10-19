Imports Pantheon.Core.Geometry

Namespace Profiles

    ''' <summary>Whether a zee spans a wall (girt) or a roof (purlin).</summary>
    Public Enum ZeeMember
        Girt
        Purlin
    End Enum

    ''' <summary>The side a section faces.</summary>
    Public Enum Hand
        Left
        Right
    End Enum

    ''' <summary>The cold-formed zee section drawn for girts and purlins.</summary>
    Public Module ZeeProfile

        Public Const DefaultWeb As Double = 8

        Public Const DefaultFlange As Double = 2.5

        ''' <summary>Each leg of the 45 degree stiffening lip.</summary>
        Public Const LipLeg As Double = 0.6875

        ''' <summary>
        ''' The open outline of the section from the bottom lip to the top lip. The origin is the
        ''' entry point: the top of the web. A purlin is rotated to <paramref name="pitch"/>
        ''' (rise over run, positive rising to the right); a girt ignores pitch.
        ''' </summary>
        Public Function Outline(member As ZeeMember, side As Hand, web As Double, flange As Double, pitch As Double) As Outline2
            Guard.Positive(web, NameOf(web))
            Guard.Positive(flange, NameOf(flange))
            Guard.Finite(pitch, NameOf(pitch))

            ' Left-hand purlin on a flat roof: web hangs down from the entry point,
            ' top flange runs right, bottom flange runs left.
            Dim flat As Point2() = {
                New Point2(-flange - LipLeg, -web + LipLeg),
                New Point2(-flange, -web),
                New Point2(0, -web),
                New Point2(0, 0),
                New Point2(flange, 0),
                New Point2(flange + LipLeg, -LipLeg)}

            Dim shaped As IEnumerable(Of Point2)
            If member = ZeeMember.Girt Then
                ' A girt is the flat purlin turned onto the wall: reflected across the line y = -x.
                shaped = flat.Select(Function(p) New Point2(-p.Y, -p.X))
            Else
                Dim angle = Math.Atan(pitch)
                shaped = flat.Select(Function(p) p.Rotate(angle))
            End If

            If side = Hand.Right Then
                shaped = shaped.Select(Function(p) p.MirrorX())
            End If

            Return New Outline2(shaped, isClosed:=False)
        End Function

    End Module

End Namespace
