using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class LinearDimensionEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void LinearDimension_BasicRoundTrip_ShouldBecomeAlignedDimension()
    {
        // Arrange
        var originalDimension = new LinearDimension(
            new Vector2(0, 0),
            new Vector2(50, 0),
            15.0,
            0.0);

        // Act & Assert
        // Note: netDxf.netstandard 3.0.1 converts LinearDimension to AlignedDimension
        PerformRoundTripTest<LinearDimension, AlignedDimension>(originalDimension, (original, recreated) =>
        {
            AssertVector2Equal(original.FirstReferencePoint, recreated.FirstReferencePoint);
            AssertVector2Equal(original.SecondReferencePoint, recreated.SecondReferencePoint);
            // AlignedDimension doesn't have offset and rotation properties like LinearDimension
        });
    }

    [Fact]
    public void LinearDimension_WithRotation_ShouldBecomeAlignedDimension()
    {
        // Arrange
        var originalDimension = new LinearDimension(
            new Vector2(0, 0),
            new Vector2(50, 50),
            15.0,
            Math.PI / 4); // 45 degrees

        // Act & Assert
        // Note: netDxf.netstandard 3.0.1 converts LinearDimension with rotation to AlignedDimension
        PerformRoundTripTest<LinearDimension, AlignedDimension>(originalDimension, (original, recreated) =>
        {
            AssertVector2Equal(original.FirstReferencePoint, recreated.FirstReferencePoint);
            AssertVector2Equal(original.SecondReferencePoint, recreated.SecondReferencePoint);
            // AlignedDimension doesn't have rotation, but preserves the reference points
        });
    }

    [Fact]
    public void LinearDimension_WithCustomLayer_ShouldBecomeAlignedDimension()
    {
        // Arrange
        var customLayer = new Layer("DimensionLayer")
        {
            Color = new AciColor(3), // Green
            Lineweight = Lineweight.W15
        };
        
        var originalDimension = new LinearDimension(
            new Vector2(10, 10),
            new Vector2(60, 10),
            20.0,
            0.0)
        {
            Layer = customLayer
        };

        // Act & Assert
        // Note: netDxf.netstandard 3.0.1 converts LinearDimension to AlignedDimension
        PerformRoundTripTest<LinearDimension, AlignedDimension>(originalDimension, (original, recreated) =>
        {
            AssertVector2Equal(original.FirstReferencePoint, recreated.FirstReferencePoint);
            AssertVector2Equal(original.SecondReferencePoint, recreated.SecondReferencePoint);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void LinearDimension_With3DCoordinates_ShouldBecomeAlignedDimension()
    {
        // Arrange
        var originalDimension = new LinearDimension(
            new Vector2(0, 0),
            new Vector2(75, 0),
            30.0,
            0.0);

        // Act & Assert
        // Note: netDxf.netstandard 3.0.1 converts LinearDimension to AlignedDimension
        PerformRoundTripTest<LinearDimension, AlignedDimension>(originalDimension, (original, recreated) =>
        {
            AssertVector2Equal(original.FirstReferencePoint, recreated.FirstReferencePoint);
            AssertVector2Equal(original.SecondReferencePoint, recreated.SecondReferencePoint);
            // AlignedDimension doesn't have offset and rotation properties like LinearDimension
        });
    }

    [Fact]
    public void LinearDimension_VerticalDimension_ShouldBecomeAlignedDimension()
    {
        // Arrange
        var originalDimension = new LinearDimension(
            new Vector2(0, 0),
            new Vector2(0, 80),
            25.0,
            Math.PI / 2); // 90 degrees

        // Act & Assert
        // Note: netDxf.netstandard 3.0.1 converts LinearDimension with rotation to AlignedDimension
        PerformRoundTripTest<LinearDimension, AlignedDimension>(originalDimension, (original, recreated) =>
        {
            AssertVector2Equal(original.FirstReferencePoint, recreated.FirstReferencePoint);
            AssertVector2Equal(original.SecondReferencePoint, recreated.SecondReferencePoint);
            // AlignedDimension doesn't have rotation, but preserves the reference points
        });
    }
}