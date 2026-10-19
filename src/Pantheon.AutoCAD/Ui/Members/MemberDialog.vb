Imports System.Drawing
Imports System.Windows.Forms
Imports WinForms = System.Windows.Forms

Namespace Ui.Members

    ''' <summary>
    ''' The dialog for one member type: a mark, then the member's fields. Picking a mark the session
    ''' already knows recalls its values; typing a new mark keeps the values on screen, which copies a member.
    ''' </summary>
    Friend NotInheritable Class MemberDialog
        Inherits Form

        Private Const InputWidth As Integer = 180
        Private Const BaysWidth As Integer = 260

        Private ReadOnly _fields As MemberField()
        Private ReadOnly _library As MemberLibrary
        Private ReadOnly _accept As Action(Of String, MemberValues)
        Private ReadOnly _markBox As New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDown, .Width = InputWidth}
        Private ReadOnly _inputs As New Dictionary(Of String, Control)(StringComparer.Ordinal)
        Private ReadOnly _toolTips As New ToolTip()
        Private ReadOnly _headingFont As Font
        Private _mark As String = ""

        ''' <param name="accept">Checks the entry and keeps what it builds; throws to keep the dialog open with a message.</param>
        Friend Sub New(title As String, fields As MemberField(), library As MemberLibrary, accept As Action(Of String, MemberValues))
            _fields = fields
            _library = library
            _accept = accept
            _headingFont = New Font(Font, FontStyle.Bold)

            Text = title
            FormBorderStyle = WinForms.FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            ShowInTaskbar = False
            StartPosition = FormStartPosition.CenterParent
            AutoScaleMode = WinForms.AutoScaleMode.Dpi
            AutoSize = True
            AutoSizeMode = WinForms.AutoSizeMode.GrowAndShrink

            Dim grid = New TableLayoutPanel With {
                .ColumnCount = 2,
                .AutoSize = True,
                .AutoSizeMode = WinForms.AutoSizeMode.GrowAndShrink,
                .Padding = New Padding(12)}

            AddRow(grid, "Mark", _markBox)
            _markBox.Items.AddRange(library.Marks())
            _toolTips.SetToolTip(_markBox, "The block name. Pick an earlier mark to recall it; type a new one to copy the values shown.")

            Dim heading = ""
            For Each field In fields
                If field.Group <> heading Then
                    heading = field.Group
                    AddHeading(grid, heading)
                End If
                Dim input = CreateInput(field)
                _inputs.Add(field.Key, input)
                AddRow(grid, If(field.Kind = FieldKind.Flag, "", field.Caption), input)
                If field.Hint.Length > 0 Then
                    _toolTips.SetToolTip(input, field.Hint)
                End If
            Next

            For Each field In fields.Where(Function(candidate) candidate.EnabledBy.Length > 0)
                Dim toggle = DirectCast(_inputs(field.EnabledBy), CheckBox)
                Dim dependent = _inputs(field.Key)
                dependent.Enabled = toggle.Checked
                AddHandler toggle.CheckedChanged, Sub(sender, e) dependent.Enabled = toggle.Checked
            Next

            Dim createButton = New Button With {.Text = "Create", .AutoSize = True}
            Dim cancelButton = New Button With {.Text = "Cancel", .AutoSize = True, .DialogResult = WinForms.DialogResult.Cancel}
            AddHandler createButton.Click, AddressOf OnCreate
            Dim buttons = New FlowLayoutPanel With {.FlowDirection = FlowDirection.RightToLeft, .AutoSize = True, .Dock = DockStyle.Fill, .Margin = New Padding(0, 12, 0, 0)}
            buttons.Controls.Add(cancelButton)
            buttons.Controls.Add(createButton)
            AddSpanningRow(grid, buttons)

            AcceptButton = createButton
            Me.CancelButton = cancelButton
            Controls.Add(grid)

            AddHandler _markBox.SelectedIndexChanged, AddressOf OnMarkSelected
            If _markBox.Items.Count > 0 Then
                _markBox.SelectedIndex = _markBox.Items.Count - 1
            End If
        End Sub

        ''' <summary>The mark accepted when the dialog closed with OK.</summary>
        Friend ReadOnly Property Mark As String
            Get
                Return _mark
            End Get
        End Property

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                _toolTips.Dispose()
                _headingFont.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        Private Sub OnCreate(sender As Object, e As EventArgs)
            Dim candidate = _markBox.Text.Trim()
            Dim texts = CurrentTexts()
            Try
                If candidate.Length = 0 Then
                    Throw New InvalidOperationException("Enter a mark for this member.")
                End If
                _accept(candidate, New MemberValues(_fields, texts))
            Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is FormatException OrElse TypeOf ex Is InvalidOperationException
                MessageBox.Show(Me, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End Try

            _library.Save(candidate, texts)
            _mark = candidate
            DialogResult = WinForms.DialogResult.OK
        End Sub

        Private Sub OnMarkSelected(sender As Object, e As EventArgs)
            Dim texts As Dictionary(Of String, String) = Nothing
            If _library.TryGetTexts(TryCast(_markBox.SelectedItem, String), texts) Then
                ShowTexts(texts)
            End If
        End Sub

        Private Function CurrentTexts() As Dictionary(Of String, String)
            Dim texts = New Dictionary(Of String, String)(StringComparer.Ordinal)
            For Each pair In _inputs
                Dim check = TryCast(pair.Value, CheckBox)
                Dim combo = TryCast(pair.Value, ComboBox)
                If check IsNot Nothing Then
                    texts(pair.Key) = If(check.Checked, Boolean.TrueString, Boolean.FalseString)
                ElseIf combo IsNot Nothing Then
                    texts(pair.Key) = If(TryCast(combo.SelectedItem, String), "")
                Else
                    texts(pair.Key) = pair.Value.Text.Trim()
                End If
            Next
            Return texts
        End Function

        Private Sub ShowTexts(texts As Dictionary(Of String, String))
            For Each pair In _inputs
                Dim value As String = Nothing
                If Not texts.TryGetValue(pair.Key, value) Then
                    Continue For
                End If
                Dim check = TryCast(pair.Value, CheckBox)
                Dim combo = TryCast(pair.Value, ComboBox)
                If check IsNot Nothing Then
                    check.Checked = String.Equals(value, Boolean.TrueString, StringComparison.Ordinal)
                ElseIf combo IsNot Nothing Then
                    combo.SelectedItem = value
                Else
                    pair.Value.Text = value
                End If
            Next
        End Sub

        Private Shared Function CreateInput(field As MemberField) As Control
            Select Case field.Kind
                Case FieldKind.Choice
                    Dim combo = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Width = InputWidth}
                    combo.Items.AddRange(field.Choices)
                    combo.SelectedIndex = 0
                    Return combo
                Case FieldKind.Flag
                    Return New CheckBox With {
                        .Text = field.Caption,
                        .AutoSize = True,
                        .Checked = String.Equals(field.DefaultText, Boolean.TrueString, StringComparison.Ordinal)}
                Case FieldKind.Bays
                    Return New TextBox With {.Width = BaysWidth, .Text = field.DefaultText}
                Case Else
                    Return New TextBox With {.Width = InputWidth, .Text = field.DefaultText}
            End Select
        End Function

        Private Shared Sub AddRow(grid As TableLayoutPanel, caption As String, input As Control)
            Dim row = NextRow(grid)
            grid.Controls.Add(New Label With {.Text = caption, .AutoSize = True, .Anchor = AnchorStyles.Left, .Margin = New Padding(0, 6, 12, 0)}, 0, row)
            grid.Controls.Add(input, 1, row)
        End Sub

        Private Sub AddHeading(grid As TableLayoutPanel, heading As String)
            AddSpanningRow(grid, New Label With {.Text = heading, .Font = _headingFont, .AutoSize = True, .Margin = New Padding(0, 12, 0, 4)})
        End Sub

        Private Shared Sub AddSpanningRow(grid As TableLayoutPanel, content As Control)
            Dim row = NextRow(grid)
            grid.Controls.Add(content, 0, row)
            grid.SetColumnSpan(content, 2)
        End Sub

        Private Shared Function NextRow(grid As TableLayoutPanel) As Integer
            Dim row = grid.RowCount
            grid.RowCount = row + 1
            grid.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            Return row
        End Function

    End Class

End Namespace
