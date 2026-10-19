Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports Pantheon.Core.Members
Imports Pantheon.Core.Workframes

Namespace Drawing

    ''' <summary>Turns Pantheon.Core members into blocks in the drawing.</summary>
    Friend Module MemberDrawing

        Private Const HiddenLinetype As String = "HIDDEN"
        Private Const SlabColor As Short = 7
        Private Const AnnotationColor As Short = 3

        Friend Sub DrawWorkframe(target As DrawingTarget, mark As String, workframe As IWorkframe)
            Dim insertion = AskPoint(target, "Select bottom left entry point: ")
            If insertion Is Nothing Then
                Return
            End If
            Dim geometry = workframe.Build(target.Database.Dimtxt)
            target.InsertBlock(mark, MemberLayer.Workframes, insertion.Value,
                               Function(transaction) WorkframeEntities(target.Database, geometry))
            target.ShowSouthWestIsometric()
        End Sub

        Friend Sub DrawSlab(target As DrawingTarget, mark As String, slab As Slab)
            Dim insertion = AskPoint(target, "Select bottom left entry point: ")
            If insertion Is Nothing Then
                Return
            End If
            target.InsertBlock(mark, MemberLayer.Foundations, insertion.Value,
                               Function(transaction) SlabEntities(target.Database, transaction, slab),
                               If(slab.HasNotch, HiddenLinetype, Nothing))
        End Sub

        Friend Sub DrawBasePlate(target As DrawingTarget, mark As String, plate As BasePlate)
            Dim insertion = AskPoint(target, "Select base point: ")
            If insertion Is Nothing Then
                Return
            End If
            target.InsertBlock(mark, MemberLayer.Plates, insertion.Value,
                               Function(transaction) New List(Of Entity) From {PlateSolid(plate)})
        End Sub

        Private Function AskPoint(target As DrawingTarget, prompt As String) As Point3d?
            Dim result = target.Editor.GetPoint(New PromptPointOptions(vbLf & prompt))
            Return If(result.Status = PromptStatus.OK, result.Value, CType(Nothing, Point3d?))
        End Function

        Private Function WorkframeEntities(database As Database, geometry As WorkframeGeometry) As List(Of Entity)
            Dim entities = New List(Of Entity)
            For Each segment In geometry.Lines
                entities.Add(New Line(ToAcad(segment.StartPoint), ToAcad(segment.EndPoint)) With {.Color = Aci(ColorOf(segment.Kind))})
            Next
            For Each bubble In geometry.Bubbles
                entities.Add(New Circle(ToAcad(bubble.Center), Vector3d.ZAxis, bubble.Radius) With {.Color = Aci(AnnotationColor)})
                Dim text = New MText()
                text.SetDatabaseDefaults()
                text.Contents = bubble.Label
                text.TextHeight = database.Dimtxt
                text.TextStyleId = database.Textstyle
                text.Attachment = AttachmentPoint.MiddleCenter
                text.Location = ToAcad(bubble.Center)
                text.Width = 0
                text.Color = Aci(AnnotationColor)
                entities.Add(text)
            Next
            Return entities
        End Function

        Private Function SlabEntities(database As Database, transaction As Transaction, slab As Slab) As List(Of Entity)
            Dim edge = DrawingTarget.ToPolyline(slab.Outline())
            edge.Color = Aci(SlabColor)
            Dim entities = New List(Of Entity) From {edge}
            If slab.HasNotch Then
                Dim linetypes = DirectCast(transaction.GetObject(database.LinetypeTableId, OpenMode.ForRead), LinetypeTable)
                Dim notch = DrawingTarget.ToPolyline(slab.NotchOutline())
                notch.LinetypeId = linetypes(HiddenLinetype)
                entities.Add(notch)
            End If
            Return entities
        End Function

        ''' <summary>The plate as a solid with its anchor holes cut through.</summary>
        Private Function PlateSolid(plate As BasePlate) As Solid3d
            Dim solid = New Solid3d()
            solid.CreateBox(plate.Width, plate.Length, plate.Thickness)
            Dim radius = plate.HoleDiameter / 2
            For Each hole In plate.Holes
                ' Twice the plate thickness so the cutter passes clean through both faces.
                Using cutter = New Solid3d()
                    cutter.CreateFrustum(plate.Thickness * 2, radius, radius, radius)
                    cutter.TransformBy(Matrix3d.Displacement(New Vector3d(hole.X, hole.Y, 0)))
                    solid.BooleanOperation(BooleanOperationType.BoolSubtract, cutter)
                End Using
            Next
            Return solid
        End Function

        Private Function ColorOf(kind As LineKind) As Short
            Select Case kind
                Case LineKind.Base
                    Return 20
                Case LineKind.Offset
                    Return 2
                Case Else
                    Return 3
            End Select
        End Function

    End Module

End Namespace
