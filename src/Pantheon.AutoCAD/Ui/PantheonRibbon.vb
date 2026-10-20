Imports Autodesk.Windows

Namespace Ui

    ''' <summary>The Pantheon ribbon tab.</summary>
    Friend Module PantheonRibbon

        Private Const TabId As String = "Pantheon"
        Private Const SmallIconSize As Integer = 16
        Private Const LargeIconSize As Integer = 32

        ''' <summary>
        ''' Adds the tab, replacing an earlier copy, and brings it to the front when <paramref name="activate"/>
        ''' is set. Returns False when the ribbon is not showing.
        ''' </summary>
        Friend Function Show(activate As Boolean) As Boolean
            Dim ribbon = ComponentManager.Ribbon
            If ribbon Is Nothing Then
                Return False
            End If

            Dim earlier = ribbon.FindTab(TabId)
            If earlier IsNot Nothing Then
                ribbon.Tabs.Remove(earlier)
            End If

            Dim pantheonTab = New RibbonTab With {.Title = "Pantheon", .Id = TabId}
            pantheonTab.Panels.Add(MakePanel("Work Frames",
                MakeButton("Symmetrical", "Draws a gable workframe.", "SymWork", "_WORKFRAME _Symmetrical "),
                MakeButton("Single Slope", "Draws a single slope workframe.", "SingleWork", "_WORKFRAME _Singleslope "),
                MakeButton("Off-Center Ridge", "Draws a workframe with its ridge off the centre line.", "OffCenWork", "_WORKFRAME _Offcenter ")))
            pantheonTab.Panels.Add(MakePanel("Foundation",
                MakeButton("Slab", "Draws a foundation slab.", "Slab", "_SLAB ")))
            pantheonTab.Panels.Add(MakePanel("Columns",
                MakeButton("Straight Column", "Draws a straight column base plate.", "StraightCol", "_COLUMN ")))
            pantheonTab.Panels.Add(MakePanel("Sections",
                MakeButton("Zee", "Draws a zee girt or purlin section.", "GirtLeft", "_ZEE ", png:=True),
                MakeButton("Cee", "Draws a cee jamb or post section.", "JambVert", "_CEE ", png:=True),
                MakeButton("Eave Strut", "Draws an eave strut section.", "EaveDSU", "_EAVECEE ", png:=True)))
            pantheonTab.Panels.Add(MakePanel("Pantheon",
                MakeTextButton("Palettes", "Shows the Shapes and Pitch palettes.", "_PANTHEONPALETTE "),
                MakeTextButton("About", "Version, quick start, privacy policy and support.", "_PANTHEONABOUT ")))
            ribbon.Tabs.Add(pantheonTab)
            If activate Then
                pantheonTab.IsActive = True
            End If
            Return True
        End Function

        Private Function MakePanel(title As String, ParamArray buttons As RibbonButton()) As RibbonPanel
            Dim source = New RibbonPanelSource With {.Title = title}
            For Each item In buttons
                source.Items.Add(item)
            Next
            Return New RibbonPanel With {.Source = source}
        End Function

        ''' <summary>
        ''' A large button. Ribbon art comes as a small and a large bitmap; a palette's single PNG
        ''' (<paramref name="png"/>) is decoded at both sizes.
        ''' </summary>
        Private Function MakeButton(caption As String, description As String, icon As String, command As String,
                                    Optional png As Boolean = False) As RibbonButton
            Return New RibbonButton With {
                .Id = "Pantheon." & icon,
                .Text = caption,
                .Description = description,
                .ShowText = True,
                .ShowImage = True,
                .Size = RibbonItemSize.Large,
                .Orientation = System.Windows.Controls.Orientation.Vertical,
                .Image = If(png, LoadRibbonImage(icon & ".png", SmallIconSize), LoadRibbonImage(icon & "_S.bmp")),
                .LargeImage = If(png, LoadRibbonImage(icon & ".png", LargeIconSize), LoadRibbonImage(icon & "_L.bmp")),
                .CommandParameter = command,
                .CommandHandler = RibbonCommand.Instance}
        End Function

        ''' <summary>A button with no artwork yet: it shows its caption.</summary>
        Private Function MakeTextButton(caption As String, description As String, command As String) As RibbonButton
            Return New RibbonButton With {
                .Id = "Pantheon." & caption,
                .Text = caption,
                .Description = description,
                .ShowText = True,
                .ShowImage = False,
                .Size = RibbonItemSize.Standard,
                .CommandParameter = command,
                .CommandHandler = RibbonCommand.Instance}
        End Function

    End Module

End Namespace
