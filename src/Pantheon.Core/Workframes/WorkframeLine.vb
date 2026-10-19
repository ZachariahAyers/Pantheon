Imports System.Globalization
Imports Pantheon.Core.Geometry

Namespace Workframes

    ''' <summary>What a workframe line represents, which sets its colour in the drawing.</summary>
    Public Enum LineKind
        ''' <summary>The building footprint.</summary>
        Base
        ''' <summary>Columns and rafters of a frame line.</summary>
        Frame
        ''' <summary>The inside of the purlins and girts.</summary>
        Offset
        ''' <summary>Grid bubble leaders.</summary>
        Annotation
    End Enum

    ''' <summary>One straight line of a workframe, in model coordinates relative to the insertion point.</summary>
    Public NotInheritable Class WorkframeLine

        Public Sub New(startPoint As Point3, endPoint As Point3, kind As LineKind)
            Me.StartPoint = startPoint
            Me.EndPoint = endPoint
            Me.Kind = kind
        End Sub

        Public ReadOnly Property StartPoint As Point3

        Public ReadOnly Property EndPoint As Point3

        Public ReadOnly Property Kind As LineKind

        ''' <summary>Identifies the line regardless of direction, so coincident lines are drawn once.</summary>
        Friend Function Key() As String
            Dim a = PointKey(StartPoint)
            Dim b = PointKey(EndPoint)
            Return If(String.CompareOrdinal(a, b) <= 0, a & "|" & b, b & "|" & a)
        End Function

        Private Shared Function PointKey(point As Point3) As String
            Return Math.Round(point.X, 6).ToString("R", CultureInfo.InvariantCulture) & "," &
                Math.Round(point.Y, 6).ToString("R", CultureInfo.InvariantCulture) & "," &
                Math.Round(point.Z, 6).ToString("R", CultureInfo.InvariantCulture)
        End Function

    End Class

End Namespace
