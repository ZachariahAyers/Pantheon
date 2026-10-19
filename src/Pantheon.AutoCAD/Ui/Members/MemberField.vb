Namespace Ui.Members

    Friend Enum FieldKind
        ''' <summary>A distance in drawing units, read with the drawing's unit settings.</summary>
        Distance
        ''' <summary>A plain number.</summary>
        Number
        ''' <summary>A bay list such as 3*25',20'.</summary>
        Bays
        ''' <summary>One of a fixed list.</summary>
        Choice
        ''' <summary>A yes or no option.</summary>
        Flag
    End Enum

    ''' <summary>One input on a member dialog.</summary>
    Friend NotInheritable Class MemberField

        Private Sub New(key As String, caption As String, kind As FieldKind, defaultText As String,
                        group As String, hint As String, choices As String(), enabledBy As String)
            Me.Key = key
            Me.Caption = caption
            Me.Kind = kind
            Me.DefaultText = defaultText
            Me.Group = group
            Me.Hint = hint
            Me.Choices = choices
            Me.EnabledBy = enabledBy
        End Sub

        Friend ReadOnly Property Key As String

        Friend ReadOnly Property Caption As String

        Friend ReadOnly Property Kind As FieldKind

        Friend ReadOnly Property DefaultText As String

        ''' <summary>Heading the field sits under; consecutive fields share one heading.</summary>
        Friend ReadOnly Property Group As String

        Friend ReadOnly Property Hint As String

        Friend ReadOnly Property Choices As String()

        ''' <summary>Key of a flag field that must be ticked for this field to be editable, or empty.</summary>
        Friend ReadOnly Property EnabledBy As String

        Friend Shared Function Distance(key As String, caption As String, group As String,
                                        Optional defaultText As String = "", Optional enabledBy As String = "") As MemberField
            Return New MemberField(key, caption, FieldKind.Distance, defaultText, group, "", Array.Empty(Of String)(), enabledBy)
        End Function

        Friend Shared Function Number(key As String, caption As String, group As String, hint As String) As MemberField
            Return New MemberField(key, caption, FieldKind.Number, "", group, hint, Array.Empty(Of String)(), "")
        End Function

        Friend Shared Function Bays(key As String, caption As String, group As String, hint As String) As MemberField
            Return New MemberField(key, caption, FieldKind.Bays, "", group, hint, Array.Empty(Of String)(), "")
        End Function

        Friend Shared Function Choice(key As String, caption As String, group As String, choices As String()) As MemberField
            Return New MemberField(key, caption, FieldKind.Choice, choices(0), group, "", choices, "")
        End Function

        Friend Shared Function Flag(key As String, caption As String, group As String, isSet As Boolean) As MemberField
            Return New MemberField(key, caption, FieldKind.Flag, If(isSet, Boolean.TrueString, Boolean.FalseString),
                                   group, "", Array.Empty(Of String)(), "")
        End Function

    End Class

End Namespace
