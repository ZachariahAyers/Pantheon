Imports System.Globalization

Namespace Workframes

    ''' <summary>Reads the bay notation engineers write on frame drawings, such as <c>3*25',20'</c>.</summary>
    Public Module BayList

        ''' <summary>
        ''' Expands a comma separated bay list into one size per bay. A <c>count*size</c> entry repeats
        ''' a size. Spaces are ignored. <paramref name="toDistance"/> converts each size, so the
        ''' caller decides which unit notation is accepted.
        ''' </summary>
        Public Function Parse(text As String, toDistance As Func(Of String, Double)) As IReadOnlyList(Of Double)
            ArgumentNullException.ThrowIfNull(toDistance, NameOf(toDistance))
            Dim bays = New List(Of Double)
            If String.IsNullOrWhiteSpace(text) Then
                Return bays
            End If

            For Each entry In text.Replace(" ", "", StringComparison.Ordinal).Split(","c)
                If entry.Length = 0 Then
                    Continue For
                End If

                Dim parts = entry.Split("*"c)
                If parts.Length > 2 Then
                    Throw New FormatException("""" & entry & """ has more than one *. Write a repeated bay as count*size, for example 3*25'.")
                End If

                Dim count = 1
                Dim size = parts(parts.Length - 1)
                If parts.Length = 2 AndAlso
                   (Not Integer.TryParse(parts(0), NumberStyles.None, CultureInfo.InvariantCulture, count) OrElse count < 1) Then
                    Throw New FormatException("""" & entry & """ needs a whole number before the *, for example 3*25'.")
                End If

                Dim bay = toDistance(size)
                If Not (bay > 0) Then
                    Throw New FormatException("""" & entry & """ must be a size greater than zero.")
                End If

                For i = 1 To count
                    bays.Add(bay)
                Next
            Next

            Return bays
        End Function

    End Module

End Namespace
