Imports System.Windows.Input
Imports Autodesk.Windows

Namespace Ui

    ''' <summary>Sends a ribbon button's command string to the command line.</summary>
    Friend NotInheritable Class RibbonCommand
        Implements ICommand

        Friend Shared ReadOnly Property Instance As New RibbonCommand()

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim button = TryCast(parameter, RibbonButton)
            If button IsNot Nothing Then
                CommandLine.Send(TryCast(button.CommandParameter, String))
            End If
        End Sub

    End Class

End Namespace
