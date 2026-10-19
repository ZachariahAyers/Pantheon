Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports Pantheon.Core.Roofs

Namespace Ui

    ''' <summary>
    ''' Rotates the UCS to a roof pitch so shapes can be drawn along the roof line, and back to World.
    ''' </summary>
    Friend NotInheritable Class PitchPalette
        Inherits UserControl

        Private ReadOnly _customRise As New TextBox With {.Width = 64, .TextAlign = HorizontalAlignment.Center}

        Friend Sub New()
            Dim grid = New TableLayoutPanel With {.ColumnCount = 2, .AutoSize = True, .Dock = DockStyle.Top, .Padding = New Padding(4)}
            AddChoice(grid, "World", "_ucs _world ", 0, 0)
            For rise = 1 To 6
                Dim fraction = rise.ToString(CultureInfo.InvariantCulture) & "/12"
                AddChoice(grid, fraction, Pitch.UcsRotationCommand(rise), 0, rise)
                AddChoice(grid, "-" & fraction, Pitch.UcsRotationCommand(-rise), 1, rise)
            Next

            grid.Controls.Add(_customRise, 0, 7)
            Dim custom = AddChoice(grid, "Custom", "", 1, 7)
            AddHandler custom.Click, AddressOf OnCustomClick

            AddChoice(grid, "Object", "_ucs _object ", 0, 8)
            AddChoice(grid, "Previous", "_ucs _previous ", 1, 8)
            Controls.Add(grid)
        End Sub

        Private Shared Function AddChoice(grid As TableLayoutPanel, caption As String, command As String,
                                          column As Integer, row As Integer) As RadioButton
            Dim choice = New RadioButton With {
                .Text = caption,
                .Appearance = Appearance.Button,
                .TextAlign = ContentAlignment.MiddleCenter,
                .Size = New Size(64, 24),
                .Tag = command}
            If command.Length > 0 Then
                AddHandler choice.Click, AddressOf OnPresetClick
            End If
            grid.Controls.Add(choice, column, row)
            Return choice
        End Function

        Private Shared Sub OnPresetClick(sender As Object, e As EventArgs)
            CommandLine.Send(TryCast(DirectCast(sender, Control).Tag, String))
        End Sub

        Private Sub OnCustomClick(sender As Object, e As EventArgs)
            Dim rise As Double
            If Double.TryParse(_customRise.Text, NumberStyles.Float, CultureInfo.CurrentCulture, rise) Then
                CommandLine.Send(Pitch.UcsRotationCommand(rise))
            Else
                MessageBox.Show(Me, "Enter the pitch as rise per 12, for example 4.5.", "Pitch", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        End Sub

    End Class

End Namespace
