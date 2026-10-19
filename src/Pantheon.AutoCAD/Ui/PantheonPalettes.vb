Imports System.Drawing
Imports Autodesk.AutoCAD.Windows

Namespace Ui

    ''' <summary>The Pantheon palette set: shapes and pitch.</summary>
    Friend Module PantheonPalettes

        Private _palettes As PaletteSet

        Friend Sub Show()
            If _palettes Is Nothing Then
                _palettes = New PaletteSet("Pantheon")
                _palettes.Add("Shapes", New ShapesPalette())
                _palettes.Add("Pitch", New PitchPalette())
                _palettes.Size = New Size(240, 420)
            End If
            _palettes.Visible = True
        End Sub

    End Module

End Namespace
