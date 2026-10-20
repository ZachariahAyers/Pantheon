Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Windows.Media.Imaging

Namespace Ui

    ''' <summary>The icons embedded in the add-in, for palettes and the ribbon.</summary>
    Friend Module Icons

        ''' <summary>The grey the ribbon bitmaps use for their background.</summary>
        Private ReadOnly RibbonBackground As Color = Color.FromArgb(192, 192, 192)

        Friend Function LoadBitmap(fileName As String) As Bitmap
            Using stream = OpenIcon(fileName), original = New Bitmap(stream)
                ' Copy so the bitmap does not depend on the stream staying open.
                Return New Bitmap(original)
            End Using
        End Function

        ''' <summary>
        ''' A ribbon image with the grey background made transparent. A nonzero <paramref name="size"/>
        ''' decodes it at that many pixels square.
        ''' </summary>
        Friend Function LoadRibbonImage(fileName As String, Optional size As Integer = 0) As BitmapImage
            Using source = LoadBitmap(fileName), encoded = New MemoryStream()
                source.MakeTransparent(RibbonBackground)
                source.Save(encoded, ImageFormat.Png)
                encoded.Position = 0
                Dim image = New BitmapImage()
                image.BeginInit()
                image.CacheOption = BitmapCacheOption.OnLoad
                If size > 0 Then
                    image.DecodePixelWidth = size
                    image.DecodePixelHeight = size
                End If
                image.StreamSource = encoded
                image.EndInit()
                image.Freeze()
                Return image
            End Using
        End Function

        Private Function OpenIcon(fileName As String) As Stream
            Dim stream = GetType(Icons).Assembly.GetManifestResourceStream("Pantheon.Icons." & fileName)
            If stream Is Nothing Then
                Throw New InvalidOperationException("The add-in is missing its icon " & fileName & ".")
            End If
            Return stream
        End Function

    End Module

End Namespace
