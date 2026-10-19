Imports System.Drawing
Imports System.Windows.Forms

Namespace Ui

    ''' <summary>One-click buttons for the zee, cee and eave strut shapes.</summary>
    Friend NotInheritable Class ShapesPalette
        Inherits UserControl

        Private Shared ReadOnly Tools As (Icon As String, Tip As String, Command As String)() = {
            ("PurlinLeft", "Purlin, left hand", "_zee _purlin _left "),
            ("PurlinRight", "Purlin, right hand", "_zee _purlin _right "),
            ("GirtLeft", "Girt, left hand", "_zee _girt _left "),
            ("GirtRight", "Girt, right hand", "_zee _girt _right "),
            ("PostVert", "Post, vertical", "_cee _post _vertical "),
            ("PostHor", "Post, horizontal", "_cee _post _horizontal "),
            ("JambVert", "Jamb, vertical", "_cee _jamb _vertical "),
            ("JambHor", "Jamb, horizontal", "_cee _jamb _horizontal "),
            ("EaveDSU", "Eave strut, double slope up", "_eavecee _orientation _dsu "),
            ("EaveSSU", "Eave strut, single slope up", "_eavecee _orientation _ssu "),
            ("EaveDSD", "Eave strut, double slope down", "_eavecee _orientation _dsd "),
            ("EaveSSD", "Eave strut, single slope down", "_eavecee _orientation _ssd ")}

        Private ReadOnly _toolTips As New ToolTip()

        Friend Sub New()
            Dim strip = New FlowLayoutPanel With {.Dock = DockStyle.Fill, .AutoScroll = True, .Padding = New Padding(4)}
            For Each tool In Tools
                Dim toolButton = New Button With {
                    .Image = LoadBitmap(tool.Icon & ".png"),
                    .Size = New Size(52, 52),
                    .FlatStyle = FlatStyle.Flat,
                    .Tag = tool.Command,
                    .AccessibleName = tool.Tip}
                _toolTips.SetToolTip(toolButton, tool.Tip)
                AddHandler toolButton.Click, AddressOf OnToolClick
                strip.Controls.Add(toolButton)
            Next
            Controls.Add(strip)
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                _toolTips.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        Private Shared Sub OnToolClick(sender As Object, e As EventArgs)
            CommandLine.Send(TryCast(DirectCast(sender, Control).Tag, String))
        End Sub

    End Class

End Namespace
