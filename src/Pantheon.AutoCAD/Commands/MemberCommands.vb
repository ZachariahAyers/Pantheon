Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Runtime
Imports Pantheon.AutoCAD.Drawing
Imports Pantheon.AutoCAD.Ui.Members

<Assembly: CommandClass(GetType(Pantheon.AutoCAD.Commands.MemberCommands))>

Namespace Commands

    ''' <summary>Commands that draw a member from a dialog of the engineer's numbers.</summary>
    Public NotInheritable Class MemberCommands

        <CommandMethod("WORKFRAME")>
        Public Shared Sub DrawWorkframe()
            Dim target = DrawingTarget.Active()
            If target Is Nothing Then
                Return
            End If

            Dim options = New PromptKeywordOptions(vbLf & "Enter workframe type [SYMmetrical/SINgleslope/Offcenter]",
                                                   "Symmetrical Singleslope Offcenter")
            Dim result = target.Editor.GetKeywords(options)
            If result.Status <> PromptStatus.OK Then
                Return
            End If

            Select Case result.StringResult
                Case "Symmetrical"
                    target.Report(Sub() MemberCatalog.SymmetricalFrame.Run(target))
                Case "Singleslope"
                    target.Report(Sub() MemberCatalog.SingleSlopeFrame.Run(target))
                Case "Offcenter"
                    target.Report(Sub() MemberCatalog.OffsetRidgeFrame.Run(target))
            End Select
        End Sub

        <CommandMethod("SLAB")>
        Public Shared Sub DrawSlab()
            Dim target = DrawingTarget.Active()
            If target IsNot Nothing Then
                target.Report(Sub() MemberCatalog.StandardSlab.Run(target))
            End If
        End Sub

        <CommandMethod("COLUMN")>
        Public Shared Sub DrawColumn()
            Dim target = DrawingTarget.Active()
            If target IsNot Nothing Then
                target.Report(Sub() MemberCatalog.StraightColumn.Run(target))
            End If
        End Sub


    End Class

End Namespace
