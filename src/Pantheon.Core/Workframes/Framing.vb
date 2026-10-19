Imports Pantheon.Core.Geometry

Namespace Workframes

    ''' <summary>How the wall girts meet the columns.</summary>
    Public Enum GirtAttachment
        ''' <summary>Girts sit flush with the outside of the columns.</summary>
        Flush
        ''' <summary>Girts run past the outside of the columns, so the purlin line stops short of the wall.</summary>
        Bypass
    End Enum

    ''' <summary>The secondary framing that sets the offset lines inside a frame.</summary>
    Public NotInheritable Class Framing

        Public Sub New(purlinDepth As Double, girtDepth As Double, attachment As GirtAttachment)
            Guard.Positive(purlinDepth, NameOf(purlinDepth))
            Guard.Positive(girtDepth, NameOf(girtDepth))
            Me.PurlinDepth = purlinDepth
            Me.GirtDepth = girtDepth
            Me.Attachment = attachment
        End Sub

        Public ReadOnly Property PurlinDepth As Double

        Public ReadOnly Property GirtDepth As Double

        Public ReadOnly Property Attachment As GirtAttachment

    End Class

End Namespace
