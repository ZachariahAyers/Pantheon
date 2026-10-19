Namespace Ui.Members

    ''' <summary>
    ''' The members created this session, by mark, so a dialog can recall or copy one.
    ''' Marks are block names, which AutoCAD compares without case.
    ''' </summary>
    Friend NotInheritable Class MemberLibrary

        Private ReadOnly _members As New Dictionary(Of String, Dictionary(Of String, String))(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly _marks As New List(Of String)

        Friend Function Marks() As String()
            Return _marks.ToArray()
        End Function

        Friend Function TryGetTexts(mark As String, ByRef texts As Dictionary(Of String, String)) As Boolean
            texts = Nothing
            Return mark IsNot Nothing AndAlso _members.TryGetValue(mark, texts)
        End Function

        Friend Sub Save(mark As String, texts As Dictionary(Of String, String))
            If _members.TryAdd(mark, texts) Then
                _marks.Add(mark)
            Else
                _members(mark) = texts
            End If
        End Sub

    End Class

End Namespace
