Imports Autodesk.AutoCAD.Internal
Imports AcadApplication = Autodesk.AutoCAD.ApplicationServices.Application

Namespace Ui

    ''' <summary>Runs commands from palettes and the ribbon as if the drafter typed them.</summary>
    Friend Module CommandLine

        Friend Sub Send(command As String)
            Dim current = AcadApplication.DocumentManager.MdiActiveDocument
            If current Is Nothing OrElse String.IsNullOrEmpty(command) Then
                Return
            End If
            Utils.SetFocusToDwgView()
            current.SendStringToExecute(command, True, False, False)
        End Sub

    End Module

End Namespace
