Namespace Workframes

    ''' <summary>A building shape that can lay out its workframe.</summary>
    Public Interface IWorkframe

        ''' <summary>Builds the lines and grid bubbles, sizing the bubbles from the drawing's text height.</summary>
        Function Build(textHeight As Double) As WorkframeGeometry

    End Interface

End Namespace
