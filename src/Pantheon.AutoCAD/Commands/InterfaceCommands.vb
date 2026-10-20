Imports Autodesk.AutoCAD.Runtime
Imports Pantheon.AutoCAD.Drawing
Imports Pantheon.AutoCAD.Ui

<Assembly: CommandClass(GetType(Pantheon.AutoCAD.Commands.InterfaceCommands))>

Namespace Commands

    ''' <summary>Commands that show Pantheon's ribbon tab and palettes.</summary>
    Public NotInheritable Class InterfaceCommands

        <CommandMethod("PANTHEON")>
        Public Shared Sub ShowPantheon()
            If Not PantheonRibbon.Show(activate:=True) Then
                DrawingTarget.Active()?.Message("The ribbon is off. Turn it on with the RIBBON command, then run PANTHEON again.")
            End If
            PantheonPalettes.Show()
        End Sub

        <CommandMethod("PANTHEONPALETTE")>
        Public Shared Sub ShowPalettes()
            PantheonPalettes.Show()
        End Sub

        <CommandMethod("PANTHEONABOUT")>
        Public Shared Sub ShowAbout()
            AboutBox.Show()
        End Sub

    End Class

End Namespace
