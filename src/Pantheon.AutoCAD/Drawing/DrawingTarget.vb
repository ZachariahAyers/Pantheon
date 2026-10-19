Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports Pantheon.Core.Geometry
Imports AcadApplication = Autodesk.AutoCAD.ApplicationServices.Application

Namespace Drawing

    ''' <summary>The drawing in front, and the database work every Pantheon command shares.</summary>
    Friend NotInheritable Class DrawingTarget

        Private Sub New(document As Document)
            Me.Document = document
        End Sub

        Friend ReadOnly Property Document As Document

        Friend ReadOnly Property Editor As Editor
            Get
                Return Document.Editor
            End Get
        End Property

        Friend ReadOnly Property Database As Database
            Get
                Return Document.Database
            End Get
        End Property

        ''' <summary>The active drawing, or Nothing when none is open.</summary>
        Friend Shared Function Active() As DrawingTarget
            Dim current = AcadApplication.DocumentManager.MdiActiveDocument
            Return If(current Is Nothing, Nothing, New DrawingTarget(current))
        End Function

        Friend Sub Message(text As String)
            Editor.WriteMessage(vbLf & text)
        End Sub

        Friend Function HasBlock(name As String) As Boolean
            Using transaction = Database.TransactionManager.StartTransaction()
                Dim blocks = DirectCast(transaction.GetObject(Database.BlockTableId, OpenMode.ForRead), BlockTable)
                Dim found = blocks.Has(name)
                transaction.Commit()
                Return found
            End Using
        End Function

        ''' <summary>Adds an outline given in current UCS coordinates to the current space, on the current layer.</summary>
        Friend Sub AddOutline(outline As Outline2)
            Using documentLock = Document.LockDocument()
                Using transaction = Database.TransactionManager.StartTransaction()
                    Dim space = DirectCast(transaction.GetObject(Database.CurrentSpaceId, OpenMode.ForWrite), BlockTableRecord)
                    Dim shape = ToPolyline(outline)
                    shape.TransformBy(Editor.CurrentUserCoordinateSystem)
                    space.AppendEntity(shape)
                    transaction.AddNewlyCreatedDBObject(shape, True)
                    transaction.Commit()
                End Using
            End Using
        End Sub

        ''' <summary>
        ''' Defines a block named <paramref name="name"/> from entities built about the origin and
        ''' inserts it at <paramref name="insertion"/>, a point in the current UCS. The layer, and the
        ''' linetype when one is named, exist before <paramref name="build"/> runs.
        ''' </summary>
        Friend Sub InsertBlock(name As String, layer As MemberLayer, insertion As Point3d,
                               build As Func(Of Transaction, List(Of Entity)), Optional linetype As String = Nothing)
            Using documentLock = Document.LockDocument()
                Using transaction = Database.TransactionManager.StartTransaction()
                    EnsureLayer(transaction, layer)
                    If linetype IsNot Nothing Then
                        EnsureLinetype(transaction, linetype)
                    End If

                    Dim blocks = DirectCast(transaction.GetObject(Database.BlockTableId, OpenMode.ForWrite), BlockTable)
                    If blocks.Has(name) Then
                        Throw New InvalidOperationException("""" & name & """ is already in this drawing.")
                    End If

                    Dim definition = New BlockTableRecord With {.Name = name, .Origin = Point3d.Origin}
                    Dim definitionId = blocks.Add(definition)
                    transaction.AddNewlyCreatedDBObject(definition, True)
                    For Each entity In build(transaction)
                        definition.AppendEntity(entity)
                        transaction.AddNewlyCreatedDBObject(entity, True)
                        entity.Layer = layer.Name
                    Next

                    Dim space = DirectCast(transaction.GetObject(Database.CurrentSpaceId, OpenMode.ForWrite), BlockTableRecord)
                    Dim reference = New BlockReference(insertion, definitionId)
                    reference.TransformBy(Editor.CurrentUserCoordinateSystem)
                    space.AppendEntity(reference)
                    transaction.AddNewlyCreatedDBObject(reference, True)
                    reference.Layer = layer.Name
                    transaction.Commit()
                End Using
            End Using
        End Sub

        ''' <summary>Turns the current model viewport to a south-west isometric view and zooms to the drawing.</summary>
        Friend Sub ShowSouthWestIsometric()
            Using documentLock = Document.LockDocument()
                Using transaction = Database.TransactionManager.StartTransaction()
                    Dim viewport = TryCast(transaction.GetObject(Editor.ActiveViewportId, OpenMode.ForWrite), ViewportTableRecord)
                    If viewport IsNot Nothing Then
                        viewport.ViewDirection = New Vector3d(-1, -1, 1)
                        Editor.UpdateTiledViewportsFromDatabase()
                    End If
                    transaction.Commit()
                End Using
            End Using
            Document.SendStringToExecute("_zoom _extents ", True, False, False)
        End Sub

        Friend Shared Function ToPolyline(outline As Outline2) As Polyline
            Dim shape = New Polyline()
            For i = 0 To outline.Points.Count - 1
                shape.AddVertexAt(i, ToAcad(outline.Points(i)), 0, 0, 0)
            Next
            shape.Closed = outline.IsClosed
            Return shape
        End Function

        Private Sub EnsureLayer(transaction As Transaction, layer As MemberLayer)
            Dim layers = DirectCast(transaction.GetObject(Database.LayerTableId, OpenMode.ForRead), LayerTable)
            If layers.Has(layer.Name) Then
                Return
            End If
            layers.UpgradeOpen()
            Dim record = New LayerTableRecord With {.Name = layer.Name, .Color = Aci(layer.ColorIndex)}
            layers.Add(record)
            transaction.AddNewlyCreatedDBObject(record, True)
            ' A layer description is stored with the database, so it is set once the record is added.
            record.Description = layer.Description
        End Sub

        Private Sub EnsureLinetype(transaction As Transaction, name As String)
            Dim linetypes = DirectCast(transaction.GetObject(Database.LinetypeTableId, OpenMode.ForRead), LinetypeTable)
            If Not linetypes.Has(name) Then
                Database.LoadLineTypeFile(name, "acad.lin")
            End If
        End Sub

    End Class

End Namespace
