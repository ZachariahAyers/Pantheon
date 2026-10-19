Imports Autodesk.Windows

Namespace Ui

    ''' <summary>The Pantheon ribbon tab.</summary>
    Friend Module PantheonRibbon

        Private Const TabId As String = "Pantheon"

        ''' <summary>Adds the tab, replacing an earlier copy. Returns False when the ribbon is not showing.</summary>
        Friend Function Show() As Boolean
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
            ribbon.Tabs.Add(pantheonTab)
            pantheonTab.IsActive = True
            Return True
        End Function

        Private Function MakePanel(title As String, ParamArray buttons As RibbonButton()) As RibbonPanel
            Dim source = New RibbonPanelSource With {.Title = title}
            For Each item In buttons
                source.Items.Add(item)
            Next
            Return New RibbonPanel With {.Source = source}
        End Function

        Private Function MakeButton(caption As String, description As String, icon As String, command As String) As RibbonButton
            Return New RibbonButton With {
                .Id = "Pantheon." & icon,
                .Text = caption,
                .Description = description,
                .ShowText = True,
                .ShowImage = True,
                .Size = RibbonItemSize.Large,
                .Orientation = System.Windows.Controls.Orientation.Vertical,
                .Image = LoadRibbonImage(icon & "_S.bmp"),
                .LargeImage = LoadRibbonImage(icon & "_L.bmp"),
                .CommandParameter = command,
                .CommandHandler = RibbonCommand.Instance}
        End Function

    End Module

End Namespace
