Imports Autodesk.AutoCAD.Runtime
Imports Autodesk.Windows
Imports CoreApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application

<Assembly: ExtensionApplication(GetType(Pantheon.AutoCAD.Ui.PantheonApplication))>

Namespace Ui

    ''' <summary>Adds the Pantheon ribbon tab once AutoCAD's ribbon exists, so the tools are there at startup.</summary>
    Public NotInheritable Class PantheonApplication
        Implements IExtensionApplication

        Public Sub Initialize() Implements IExtensionApplication.Initialize
            AddHandler CoreApplication.Idle, AddressOf ShowRibbonWhenReady
        End Sub

        Public Sub Terminate() Implements IExtensionApplication.Terminate
            RemoveHandler CoreApplication.Idle, AddressOf ShowRibbonWhenReady
        End Sub

        ''' <summary>Waits for an idle moment with the ribbon showing, then adds the tab once without bringing it to the front.</summary>
        Private Shared Sub ShowRibbonWhenReady(sender As Object, e As EventArgs)
            If ComponentManager.Ribbon Is Nothing Then
                Return
            End If
            RemoveHandler CoreApplication.Idle, AddressOf ShowRibbonWhenReady
            PantheonRibbon.Show(activate:=False)
        End Sub

    End Class

End Namespace
