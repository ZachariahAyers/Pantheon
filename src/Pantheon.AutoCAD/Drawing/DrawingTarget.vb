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

        ''' <summary>
        ''' Runs one drawing step. Bad input and Pantheon's own refusals are shown unchanged on the
        ''' command line; a refusal from AutoCAD is named as one. Nothing reaches AutoCAD's error dialog.
        ''' </summary>
        Friend Sub Report(action As Action)
            Try
                action()
            Catch ex As System.Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is InvalidOperationException
                Message(ex.Message)
            Catch ex As Autodesk.AutoCAD.Runtime.Exception
                Message("AutoCAD could not add the drawing: " & ex.Message)
            End Try
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
        ''' linetype when one is named, exist before <paramref name="build"/> runs. A locked layer
        ''' stops the command before anything is added; an off or frozen one is drawn on and reported.
        ''' </summary>
        Friend Sub InsertBlock(name As String, layer As MemberLayer, insertion As Point3d,
                               build As Func(Of Transaction, List(Of Entity)), Optional linetype As String = Nothing)
            Using documentLock = Document.LockDocument()
                Using transaction = Database.TransactionManager.StartTransaction()
                    Dim record = EnsureLayer(transaction, layer)
                    If record.IsLocked Then
                        Throw New InvalidOperationException("The " & layer.Name & " layer is locked. Unlock it, then draw the member again.")
                    End If
                    Dim hidden = record.IsOff OrElse record.IsFrozen
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
                        entity.Layer = layer.Name
                        definition.AppendEntity(entity)
                        transaction.AddNewlyCreatedDBObject(entity, True)
                    Next

                    Dim space = DirectCast(transaction.GetObject(Database.CurrentSpaceId, OpenMode.ForWrite), BlockTableRecord)
                    Dim reference = New BlockReference(insertion, definitionId) With {.Layer = layer.Name}
                    reference.TransformBy(Editor.CurrentUserCoordinateSystem)
                    space.AppendEntity(reference)
                    transaction.AddNewlyCreatedDBObject(reference, True)
                    transaction.Commit()
                    If hidden Then
                        Message("""" & name & """ is on the " & layer.Name & " layer, which is off or frozen. Turn it on and thaw it to see the member.")
                    End If
                End Using
            End Using
        End Sub

        ''' <summary>
        ''' Turns the current model viewport to a south-west isometric view and zooms to the drawing,
        ''' inside the running command so one undo takes back the member and the view together.
        ''' </summary>
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
            Editor.Command("_.ZOOM", "_Extents")
        End Sub

        Friend Shared Function ToPolyline(outline As Outline2) As Polyline
            Dim shape = New Polyline()
            For i = 0 To outline.Points.Count - 1
                shape.AddVertexAt(i, ToAcad(outline.Points(i)), 0, 0, 0)
            Next
            shape.Closed = outline.IsClosed
            Return shape
        End Function

        Private Function EnsureLayer(transaction As Transaction, layer As MemberLayer) As LayerTableRecord
            Dim layers = DirectCast(transaction.GetObject(Database.LayerTableId, OpenMode.ForRead), LayerTable)
            If layers.Has(layer.Name) Then
                Return DirectCast(transaction.GetObject(layers(layer.Name), OpenMode.ForRead), LayerTableRecord)
            End If
            layers.UpgradeOpen()
            Dim record = New LayerTableRecord With {.Name = layer.Name, .Color = Aci(layer.ColorIndex)}
            layers.Add(record)
            transaction.AddNewlyCreatedDBObject(record, True)
            ' A layer description is stored with the database, so it is set once the record is added.
            record.Description = layer.Description
            Return record
        End Function

        Private Sub EnsureLinetype(transaction As Transaction, name As String)
            Dim linetypes = DirectCast(transaction.GetObject(Database.LinetypeTableId, OpenMode.ForRead), LinetypeTable)
            If Not linetypes.Has(name) Then
                Database.LoadLineTypeFile(name, "acad.lin")
            End If
        End Sub

    End Class

End Namespace
