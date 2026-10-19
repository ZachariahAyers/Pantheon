Imports Pantheon.AutoCAD.Drawing
Imports Pantheon.Core.Members
Imports Pantheon.Core.Roofs
Imports Pantheon.Core.Workframes

Namespace Ui.Members

    ''' <summary>Every member Pantheon draws from a dialog.</summary>
    Friend Module MemberCatalog

        Private ReadOnly GirtChoices As String() = {"Flush", "Bypass"}

        Private Const BayHint As String = "Bay sizes, for example 3*25',20'. Leave blank for none."

        Friend ReadOnly Property SymmetricalFrame As New MemberDefinition(Of IWorkframe)(
            "Symmetrical workframe",
            Concat({
                MemberField.Distance("width", "Width", "Building"),
                MemberField.Distance("eave", "Eave height", "Building"),
                MemberField.Distance("length", "Length", "Building"),
                MemberField.Number("pitch", "Pitch", "Building", "Rise per 12 of run, for example 1 for 1:12.")},
                FramingFields("", "Framing"), BayFields()),
            Function(values) New SymmetricalWorkframe(
                values.Distance("width"), values.Distance("length"), values.Distance("eave"),
                Pitch.FromRisePerTwelve(values.Number("pitch")), FramingOf(values, ""), BaysOf(values)),
            AddressOf MemberDrawing.DrawWorkframe)

        Friend ReadOnly Property SingleSlopeFrame As New MemberDefinition(Of IWorkframe)(
            "Single slope workframe",
            Concat({
                MemberField.Distance("width", "Width", "Building"),
                MemberField.Distance("lowEave", "Low eave height", "Building"),
                MemberField.Distance("highEave", "High eave height", "Building"),
                MemberField.Distance("length", "Length", "Building")},
                FramingFields("", "Framing"), BayFields()),
            Function(values) New SingleSlopeWorkframe(
                values.Distance("width"), values.Distance("length"), values.Distance("lowEave"), values.Distance("highEave"),
                FramingOf(values, ""), BaysOf(values)),
            AddressOf MemberDrawing.DrawWorkframe)

        Friend ReadOnly Property OffsetRidgeFrame As New MemberDefinition(Of IWorkframe)(
            "Off-center ridge workframe",
            Concat(SideFields("left.", "Left side"), SideFields("right.", "Right side"), BayFields()),
            Function(values) New OffsetRidgeWorkframe(SideOf(values, "left."), SideOf(values, "right."), BaysOf(values)),
            AddressOf MemberDrawing.DrawWorkframe)

        Friend ReadOnly Property StandardSlab As New MemberDefinition(Of Slab)(
            "Standard slab",
            {MemberField.Distance("width", "Width", "Slab"),
             MemberField.Distance("length", "Length", "Slab"),
             MemberField.Flag("notch", "Notch for a brick ledge", "Slab", isSet:=True),
             MemberField.Distance("notchDepth", "Notch depth", "Slab", "1-1/2""", enabledBy:="notch")},
            Function(values) New Slab(values.Distance("width"), values.Distance("length"),
                                      If(values.Flag("notch"), values.Distance("notchDepth"), 0)),
            AddressOf MemberDrawing.DrawSlab)

        Friend ReadOnly Property StraightColumn As New MemberDefinition(Of BasePlate)(
            "Straight column base plate",
            {MemberField.Distance("webDepth", "Web depth", "Column"),
             MemberField.Distance("outerFlange", "Outer flange thickness", "Column"),
             MemberField.Distance("innerFlange", "Inner flange thickness", "Column"),
             MemberField.Distance("plateWidth", "Plate width", "Base plate", "8"""),
             MemberField.Distance("plateThickness", "Plate thickness", "Base plate", "1/2"""),
             MemberField.Choice("anchor", "Anchor rod", "Anchor rods", AnchorRods.Labels.ToArray()),
             MemberField.Distance("anchorFromFlange", "From outer flange", "Anchor rods", "4"""),
             MemberField.Distance("anchorSpacing", "Spacing", "Anchor rods", "4""")},
            Function(values) New BasePlate(
                values.Distance("plateWidth"), values.Distance("plateThickness"), values.Distance("webDepth"),
                values.Distance("outerFlange"), values.Distance("innerFlange"),
                AnchorRods.DiameterOf(values.Choice("anchor")),
                values.Distance("anchorFromFlange"), values.Distance("anchorSpacing")),
            AddressOf MemberDrawing.DrawBasePlate)

        Private Function FramingFields(prefix As String, group As String) As MemberField()
            Return {
                MemberField.Distance(prefix & "purlin", "Purlin depth", group, "8"""),
                MemberField.Distance(prefix & "girt", "Girt depth", group, "8"""),
                MemberField.Choice(prefix & "girts", "Girts", group, GirtChoices)}
        End Function

        Private Function SideFields(prefix As String, group As String) As MemberField()
            Return Concat({
                MemberField.Distance(prefix & "width", "Width", group),
                MemberField.Distance(prefix & "eave", "Eave height", group),
                MemberField.Distance(prefix & "length", "Length", group),
                MemberField.Number(prefix & "pitch", "Pitch", group, "Rise per 12 of run, rising toward the ridge.")},
                FramingFields(prefix, group))
        End Function

        Private Function BayFields() As MemberField()
            Return {
                MemberField.Bays("sideBays", "Sidewall bays", "Bays", BayHint),
                MemberField.Bays("endBays", "Endwall bays", "Bays", BayHint)}
        End Function

        Private Function FramingOf(values As MemberValues, prefix As String) As Framing
            Dim attachment = If(values.Choice(prefix & "girts") = "Bypass", GirtAttachment.Bypass, GirtAttachment.Flush)
            Return New Framing(values.Distance(prefix & "purlin"), values.Distance(prefix & "girt"), attachment)
        End Function

        Private Function SideOf(values As MemberValues, prefix As String) As RoofSide
            Return New RoofSide(values.Distance(prefix & "width"), values.Distance(prefix & "length"),
                                values.Distance(prefix & "eave"), Pitch.FromRisePerTwelve(values.Number(prefix & "pitch")),
                                FramingOf(values, prefix))
        End Function

        Private Function BaysOf(values As MemberValues) As BayLayout
            Return New BayLayout(values.Bays("sideBays"), values.Bays("endBays"))
        End Function

        Private Function Concat(ParamArray parts As MemberField()()) As MemberField()
            Return parts.SelectMany(Function(part) part).ToArray()
        End Function

    End Module

End Namespace
