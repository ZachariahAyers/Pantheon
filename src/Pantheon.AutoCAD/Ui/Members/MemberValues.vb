Imports System.Globalization
Imports Autodesk.AutoCAD.Runtime
Imports Pantheon.Core.Workframes

Namespace Ui.Members

    ''' <summary>The text a drafter entered on a member dialog, read as typed values.</summary>
    Friend NotInheritable Class MemberValues

        Private ReadOnly _fields As Dictionary(Of String, MemberField)
        Private ReadOnly _texts As Dictionary(Of String, String)

        Friend Sub New(fields As MemberField(), texts As Dictionary(Of String, String))
            _fields = fields.ToDictionary(Function(field) field.Key, StringComparer.Ordinal)
            _texts = texts
        End Sub

        ''' <summary>A distance in the drawing's current units, so 40' and 480 both work in an architectural drawing.</summary>
        Friend Function Distance(key As String) As Double
            Try
                Return Converter.StringToDistance(_texts(key))
            Catch ex As System.Exception When IsUnitError(ex)
                Throw New FormatException(CaptionOf(key) & ": enter a distance, for example 40' or 480.", ex)
            End Try
        End Function

        ''' <summary>A bay list; each size is read in architectural units, as engineers write them.</summary>
        Friend Function Bays(key As String) As IReadOnlyList(Of Double)
            Dim caption = CaptionOf(key)
            Return BayList.Parse(_texts(key), Function(size) BayDistance(caption, size))
        End Function

        Friend Function Number(key As String) As Double
            Dim value As Double
            If Double.TryParse(_texts(key), NumberStyles.Float, CultureInfo.CurrentCulture, value) Then
                Return value
            End If
            Throw New FormatException(CaptionOf(key) & ": enter a number.")
        End Function

        Friend Function Choice(key As String) As String
            Return _texts(key)
        End Function

        Friend Function Flag(key As String) As Boolean
            Return String.Equals(_texts(key), Boolean.TrueString, StringComparison.Ordinal)
        End Function

        Private Function CaptionOf(key As String) As String
            Return _fields(key).Caption
        End Function

        Private Shared Function BayDistance(caption As String, size As String) As Double
            Try
                Return Converter.StringToDistance(size, DistanceUnitFormat.Architectural)
            Catch ex As System.Exception When IsUnitError(ex)
                Throw New FormatException(caption & ": """ & size & """ is not a distance.", ex)
            End Try
        End Function

        Private Shared Function IsUnitError(ex As System.Exception) As Boolean
            Return TypeOf ex Is Autodesk.AutoCAD.Runtime.Exception OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is FormatException
        End Function

    End Class

End Namespace
