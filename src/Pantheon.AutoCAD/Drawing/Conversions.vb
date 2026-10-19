Imports Autodesk.AutoCAD.Colors
Imports Autodesk.AutoCAD.Geometry
Imports Pantheon.Core.Geometry

Namespace Drawing

    ''' <summary>Moves values between Pantheon.Core and the AutoCAD API.</summary>
    Friend Module Conversions

        Friend Function ToAcad(point As Point2) As Point2d
            Return New Point2d(point.X, point.Y)
        End Function

        Friend Function ToAcad(point As Point3) As Point3d
            Return New Point3d(point.X, point.Y, point.Z)
        End Function

        ''' <summary>A picked point in the plane of the current UCS.</summary>
        Friend Function ToPlane(point As Point3d) As Point2
            Return New Point2(point.X, point.Y)
        End Function

        ''' <summary>An AutoCAD Color Index colour.</summary>
        Friend Function Aci(index As Short) As Color
            Return Color.FromColorIndex(ColorMethod.ByAci, index)
        End Function

    End Module

End Namespace
