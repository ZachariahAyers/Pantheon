Imports System.Windows.Forms
Imports Autodesk.AutoCAD.DatabaseServices
Imports Pantheon.AutoCAD.Drawing
Imports AcadApplication = Autodesk.AutoCAD.ApplicationServices.Application

Namespace Ui.Members

    ''' <summary>
    ''' A member type: its dialog fields, how the entries become a Pantheon.Core model, and how the
    ''' model is drawn. Each definition keeps the members made with it this session.
    ''' </summary>
    Friend NotInheritable Class MemberDefinition(Of TModel)

        Private ReadOnly _library As New MemberLibrary()
        Private ReadOnly _title As String
        Private ReadOnly _fields As MemberField()
        Private ReadOnly _build As Func(Of MemberValues, TModel)
        Private ReadOnly _draw As Action(Of DrawingTarget, String, TModel)

        Friend Sub New(title As String, fields As MemberField(), build As Func(Of MemberValues, TModel),
                       draw As Action(Of DrawingTarget, String, TModel))
            _title = title
            _fields = fields
            _build = build
            _draw = draw
        End Sub

        ''' <summary>Shows the dialog, then draws the member it describes.</summary>
        Friend Sub Run(target As DrawingTarget)
            Dim model As TModel = Nothing
            Dim mark As String = Nothing
            Using dialog = New MemberDialog(_title, _fields, _library,
                                            Sub(candidate, values)
                                                CheckMark(target, candidate)
                                                model = _build(values)
                                            End Sub)
                If AcadApplication.ShowModalDialog(dialog) <> DialogResult.OK Then
                    Return
                End If
                mark = dialog.Mark
            End Using
            _draw(target, mark, model)
        End Sub

        Private Shared Sub CheckMark(target As DrawingTarget, mark As String)
            Try
                SymbolUtilityServices.ValidateSymbolName(mark, False)
            Catch ex As Autodesk.AutoCAD.Runtime.Exception
                Throw New InvalidOperationException("""" & mark & """ cannot be a block name. Leave out characters such as < > / \ "" : ; ? * | , = and `.", ex)
            End Try
            If target.HasBlock(mark) Then
                Throw New InvalidOperationException("""" & mark & """ is already in this drawing. Choose another mark.")
            End If
        End Sub

    End Class

End Namespace
