Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Runtime
Imports Pantheon.AutoCAD.Drawing
Imports Pantheon.Core.Geometry
Imports Pantheon.Core.Profiles
Imports Pantheon.Core.Roofs

<Assembly: CommandClass(GetType(Pantheon.AutoCAD.Commands.ShapeCommands))>

Namespace Commands

    ''' <summary>
    ''' Commands that draw a cold-formed section at picked points. Sizes changed at a prompt are kept
    ''' for the rest of the session.
    ''' </summary>
    Public NotInheritable Class ShapeCommands

        Private Shared _zeeWeb As Double = ZeeProfile.DefaultWeb
        Private Shared _zeeFlange As Double = ZeeProfile.DefaultFlange
        Private Shared _ceeWeb As Double = CeeProfile.DefaultWeb
        Private Shared _eaveTopFlange As Double = EaveStrutProfile.DefaultTopFlange
        Private Shared _eaveBottomFlange As Double = EaveStrutProfile.DefaultBottomFlange
        ' Starts at DoubleSlopeUp, the first style.
        Private Shared _eaveStyle As EaveStrutStyle

        <CommandMethod("ZEE")>
        Public Shared Sub DrawZee()
            Dim target = DrawingTarget.Active()
            If target Is Nothing Then
                Return
            End If
            Dim input = target.Editor

            Dim member = AskKeyword(input, "Enter zee type [Girt/Purlin]", "Girt Purlin")
            If member Is Nothing Then
                Return
            End If
            Dim side = AskKeyword(input, "Enter orientation [Left/Right]", "Left Right")
            If side Is Nothing Then
                Return
            End If
            Dim zeeKind = If(member = "Purlin", ZeeMember.Purlin, ZeeMember.Girt)
            Dim facing = If(side = "Right", Hand.Right, Hand.Left)

            Do
                Dim entry = input.GetPoint(New PromptPointOptions(vbLf & "Select zee entry point or [Flange/Web]: ", "Flange Web"))
                Select Case entry.Status
                    Case PromptStatus.Keyword
                        Dim answered = If(entry.StringResult = "Flange",
                                          AskLength(input, "Enter flange length: ", _zeeFlange),
                                          AskLength(input, "Enter web depth: ", _zeeWeb))
                        If Not answered Then
                            Return
                        End If
                    Case PromptStatus.OK
                        Dim slope = 0.0
                        If zeeKind = ZeeMember.Purlin Then
                            Dim rise = input.GetDouble(New PromptDoubleOptions(vbLf & "Enter pitch (rise per 12): "))
                            If rise.Status <> PromptStatus.OK Then
                                Return
                            End If
                            slope = Pitch.FromRisePerTwelve(rise.Value)
                        End If
                        Dim origin = ToPlane(entry.Value)
                        AddShape(target, Function() ZeeProfile.Outline(zeeKind, facing, _zeeWeb, _zeeFlange, slope).MovedTo(origin))
                        Return
                    Case Else
                        Return
                End Select
            Loop
        End Sub

        <CommandMethod("CEE")>
        Public Shared Sub DrawCee()
            Dim target = DrawingTarget.Active()
            If target Is Nothing Then
                Return
            End If
            Dim input = target.Editor

            Dim member = AskKeyword(input, "Enter cee type [Jamb/Post]", "Jamb Post")
            If member Is Nothing Then
                Return
            End If
            Dim layout = AskKeyword(input, "Enter orientation [Horizontal/Vertical]", "Horizontal Vertical")
            If layout Is Nothing Then
                Return
            End If
            Dim flange = If(member = "Jamb", CeeProfile.JambFlange, CeeProfile.PostFlange)

            Do
                Dim middle = input.GetPoint(New PromptPointOptions(vbLf & "Select cee mid point or [Flange/Web]: ", "Flange Web"))
                Select Case middle.Status
                    Case PromptStatus.Keyword
                        Dim answered = If(middle.StringResult = "Flange",
                                          AskLength(input, "Enter flange length: ", flange),
                                          AskLength(input, "Enter web depth: ", _ceeWeb))
                        If Not answered Then
                            Return
                        End If
                    Case PromptStatus.OK
                        Dim toward = input.GetPoint(New PromptPointOptions(vbLf & "Select orientation point: "))
                        If toward.Status <> PromptStatus.OK Then
                            Return
                        End If
                        Dim direction As FlangeDirection
                        If layout = "Horizontal" Then
                            direction = If(toward.Value.Y > middle.Value.Y, FlangeDirection.Up, FlangeDirection.Down)
                        Else
                            direction = If(toward.Value.X > middle.Value.X, FlangeDirection.Right, FlangeDirection.Left)
                        End If
                        Dim origin = ToPlane(middle.Value)
                        Dim flangeLength = flange
                        AddShape(target, Function() CeeProfile.Outline(_ceeWeb, flangeLength, direction).MovedTo(origin))
                        Return
                    Case Else
                        Return
                End Select
            Loop
        End Sub

        <CommandMethod("EAVECEE")>
        Public Shared Sub DrawEaveStrut()
            Dim target = DrawingTarget.Active()
            If target Is Nothing Then
                Return
            End If
            Dim input = target.Editor

            Do
                Dim bottom = input.GetPoint(New PromptPointOptions(
                    vbLf & "Select eave web bottom point or [Topflange/Bottomflange/Orientation]: ",
                    "Topflange Bottomflange Orientation"))
                Select Case bottom.Status
                    Case PromptStatus.Keyword
                        If Not ChangeEaveSetting(input, bottom.StringResult) Then
                            Return
                        End If
                    Case PromptStatus.OK
                        Dim top = input.GetPoint(New PromptPointOptions(vbLf & "Select eave web top point: "))
                        If top.Status <> PromptStatus.OK Then
                            Return
                        End If
                        Dim roof = input.GetPoint(New PromptPointOptions(vbLf & "Select pitch point: "))
                        If roof.Status <> PromptStatus.OK Then
                            Return
                        End If
                        Dim webBottom = ToPlane(bottom.Value)
                        Dim webTop = ToPlane(top.Value)
                        Dim pitchPoint = ToPlane(roof.Value)
                        AddShape(target, Function() EaveStrutProfile.Outline(webBottom, webTop, pitchPoint,
                                                                             _eaveStyle, _eaveTopFlange, _eaveBottomFlange))
                        Return
                    Case Else
                        Return
                End Select
            Loop
        End Sub

        Private Shared Function ChangeEaveSetting(input As Editor, setting As String) As Boolean
            Select Case setting
                Case "Topflange"
                    Return AskLength(input, "Enter top flange length: ", _eaveTopFlange)
                Case "Bottomflange"
                    Return AskLength(input, "Enter bottom flange length: ", _eaveBottomFlange)
                Case Else
                    Dim style = AskKeyword(input, "Enter orientation type [DSU/DSD/SSU/SSD]", "DSU DSD SSU SSD")
                    Select Case style
                        Case "DSU"
                            _eaveStyle = EaveStrutStyle.DoubleSlopeUp
                        Case "DSD"
                            _eaveStyle = EaveStrutStyle.DoubleSlopeDown
                        Case "SSU"
                            _eaveStyle = EaveStrutStyle.SingleSlopeUp
                        Case "SSD"
                            _eaveStyle = EaveStrutStyle.SingleSlopeDown
                        Case Else
                            Return False
                    End Select
                    Return True
            End Select
        End Function

        ''' <summary>Draws a shape, reporting a bad size, a bad pick or an AutoCAD refusal on the command line.</summary>
        Private Shared Sub AddShape(target As DrawingTarget, shape As Func(Of Outline2))
            target.Report(Sub() target.AddOutline(shape()))
        End Sub

        Private Shared Function AskKeyword(input As Editor, message As String, keywords As String) As String
            Dim result = input.GetKeywords(New PromptKeywordOptions(vbLf & message, keywords))
            Return If(result.Status = PromptStatus.OK, result.StringResult, Nothing)
        End Function

        Private Shared Function AskLength(input As Editor, message As String, ByRef length As Double) As Boolean
            Dim result = input.GetDistance(New PromptDistanceOptions(vbLf & message))
            If result.Status <> PromptStatus.OK Then
                Return False
            End If
            length = result.Value
            Return True
        End Function

    End Class

End Namespace
