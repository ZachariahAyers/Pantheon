Imports Pantheon.Core.Members
Imports Xunit

Public NotInheritable Class MemberTests

    <Fact>
    Public Sub SlabOutlineStartsAtTheInsertionPoint()
        SamePoints({P(0, 0), P(0, 480), P(900, 480), P(900, 0)}, New Slab(480, 900, 0).Outline().Points)
    End Sub

    <Fact>
    Public Sub SlabNotchRunsOutsideEveryEdge()
        SamePoints({P(-1.5, -1.5), P(-1.5, 481.5), P(901.5, 481.5), P(901.5, -1.5)}, New Slab(480, 900, 1.5).NotchOutline().Points)
    End Sub

    <Fact>
    Public Sub SlabWithoutANotchHasNoNotchOutline()
        Dim slab = New Slab(480, 900, 0)
        Assert.False(slab.HasNotch)
        Assert.Throws(Of InvalidOperationException)(Function() slab.NotchOutline())
    End Sub

    <Fact>
    Public Sub BasePlateSpansTheFullColumnDepth()
        Dim plate = New BasePlate(8, 0.5, 10, 0.25, 0.25, 0.625, 4, 4)
        Assert.Equal(10.5, plate.Length, 9)
        SamePoints({P(-2, 1.25), P(2, 1.25), P(-2, -2.75), P(2, -2.75)}, plate.Holes)
        Assert.Equal(0.625, plate.HoleDiameter, 9)
    End Sub

    <Fact>
    Public Sub BasePlateRejectsHolesOffThePlate()
        Assert.Throws(Of ArgumentException)(Function() New BasePlate(8, 0.5, 10, 0.25, 0.25, 0.625, 4, 8))
    End Sub

    <Theory>
    <InlineData("1/2"" x 8""", 0.5)>
    <InlineData("3/4"" x 15""", 0.75)>
    <InlineData("5/8"" x 6"" Titen HD", 0.625)>
    Public Sub AnchorRodDiameterComesFromItsLabel(label As String, diameter As Double)
        Assert.Equal(diameter, AnchorRods.DiameterOf(label), 9)
    End Sub

End Class
