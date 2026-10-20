Imports System.Diagnostics
Imports System.Windows.Forms

Namespace Ui

    ''' <summary>The version and licence, with links to the quick start, the privacy policy, support and the source.</summary>
    Friend Module AboutBox

        Friend Sub Show()
            Dim version = GetType(AboutBox).Assembly.GetName().Version
            Dim quickStart = New TaskDialogCommandLinkButton("Quick start", "The commands and how to begin")
            Dim privacy = New TaskDialogCommandLinkButton("Privacy policy", "Pantheon collects nothing and sends nothing")
            Dim support = New TaskDialogCommandLinkButton("Email support", ProductInfo.SupportAddress)
            Dim source = New TaskDialogCommandLinkButton("Source code", "Free software under the GNU GPL version 3")
            Dim page = New TaskDialogPage With {
                .Caption = "About Pantheon",
                .Heading = "Pantheon " & If(version?.ToString(3), ""),
                .Text = "Draws metal building framing in AutoCAD from the engineer's numbers. Free. Made in the USA by Ayers Electronics.",
                .Footnote = New TaskDialogFootnote With {.Text = "Copyright Ayers Electronics Inc."}}
            page.Buttons.Add(quickStart)
            page.Buttons.Add(privacy)
            page.Buttons.Add(support)
            page.Buttons.Add(source)
            page.Buttons.Add(TaskDialogButton.Close)

            Dim choice = TaskDialog.ShowDialog(page)
            Dim target As String = Nothing
            If choice Is quickStart Then
                target = ProductInfo.QuickStart()
            ElseIf choice Is privacy Then
                target = ProductInfo.PrivacyPolicy
            ElseIf choice Is support Then
                target = "mailto:" & ProductInfo.SupportAddress
            ElseIf choice Is source Then
                target = ProductInfo.SourceCode
            End If
            If target IsNot Nothing Then
                ' The browser or mail program opens the link; Pantheon itself makes no connection.
                Using Process.Start(New ProcessStartInfo(target) With {.UseShellExecute = True})
                End Using
            End If
        End Sub

    End Module

End Namespace
