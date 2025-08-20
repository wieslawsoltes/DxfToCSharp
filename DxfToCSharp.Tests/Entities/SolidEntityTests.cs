using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class SolidEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Solid_BasicRoundTrip_ShouldPreserveVertices()
    {
        // Arrange
        var originalSolid = new Solid(
            new Vector2(0, 0),
            new Vector2(100, 0),
            new Vector2(100, 100),
            new Vector2(0, 100));

        // Act & Assert
        PerformRoundTripTest(originalSolid, (original, recreated) =>
        {
            AssertVector2Equal(original.FirstVertex, recreated.FirstVertex);
            AssertVector2Equal(original.SecondVertex, recreated.SecondVertex);
            AssertVector2Equal(original.ThirdVertex, recreated.ThirdVertex);
            AssertVector2Equal(original.FourthVertex, recreated.FourthVertex);
        });
    }

    [Fact]
    public void Solid_With3DCoordinates_ShouldPreserveZValues()
    {
        // Arrange
        var originalSolid = new Solid(
            new Vector2(0, 0),
            new Vector2(50, 0),
            new Vector2(50, 50),
            new Vector2(0, 50));

        // Act & Assert
        PerformRoundTripTest(originalSolid, (original, recreated) =>
        {
            AssertVector2Equal(original.FirstVertex, recreated.FirstVertex);
            AssertVector2Equal(original.SecondVertex, recreated.SecondVertex);
            AssertVector2Equal(original.ThirdVertex, recreated.ThirdVertex);
            AssertVector2Equal(original.FourthVertex, recreated.FourthVertex);
        });
    }

    [Fact]
    public void Solid_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("SolidLayer")
        {
            Color = new AciColor(1), // Red
            Lineweight = Lineweight.W40
        };
        
        var originalSolid = new Solid(
            new Vector2(0, 0),
            new Vector2(25, 0),
            new Vector2(25, 25),
            new Vector2(0, 25))
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalSolid, (original, recreated) =>
        {
            AssertVector2Equal(original.FirstVertex, recreated.FirstVertex);
            AssertVector2Equal(original.SecondVertex, recreated.SecondVertex);
            AssertVector2Equal(original.ThirdVertex, recreated.ThirdVertex);
            AssertVector2Equal(original.FourthVertex, recreated.FourthVertex);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void Solid_WithCustomColor_ShouldPreserveColor()
    {
        // Arrange
        var originalSolid = new Solid(
            new Vector2(0, 0),
            new Vector2(30, 0),
            new Vector2(30, 30),
            new Vector2(0, 30))
        {
            Color = new AciColor(3) // Green
        };

        // Act & Assert
        PerformRoundTripTest(originalSolid, (original, recreated) =>
        {
            AssertVector2Equal(original.FirstVertex, recreated.FirstVertex);
            AssertVector2Equal(original.SecondVertex, recreated.SecondVertex);
            AssertVector2Equal(original.ThirdVertex, recreated.ThirdVertex);
            AssertVector2Equal(original.FourthVertex, recreated.FourthVertex);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
        });
    }

    [Fact]
    public void Solid_Triangle_ShouldPreserveGeometry()
    {
        // Arrange - Triangle (fourth vertex same as third)
        var originalSolid = new Solid(
            new Vector2(0, 0),
            new Vector2(60, 0),
            new Vector2(30, 52),
            new Vector2(30, 52));

        // Act & Assert
        PerformRoundTripTest(originalSolid, (original, recreated) =>
        {
            AssertVector2Equal(original.FirstVertex, recreated.FirstVertex);
            AssertVector2Equal(original.SecondVertex, recreated.SecondVertex);
            AssertVector2Equal(original.ThirdVertex, recreated.ThirdVertex);
            AssertVector2Equal(original.FourthVertex, recreated.FourthVertex);
        });
    }

    [Fact]
    public void Solid_WithThickness_ShouldPreserveThickness()
    {
        // Arrange
        var originalSolid = new Solid(
            new Vector2(0, 0),
            new Vector2(40, 0),
            new Vector2(40, 40),
            new Vector2(0, 40))
        {
            Thickness = 15.5
        };

        // Act & Assert
        PerformRoundTripTest(originalSolid, (original, recreated) =>
        {
            AssertVector2Equal(original.FirstVertex, recreated.FirstVertex);
            AssertVector2Equal(original.SecondVertex, recreated.SecondVertex);
            AssertVector2Equal(original.ThirdVertex, recreated.ThirdVertex);
            AssertVector2Equal(original.FourthVertex, recreated.FourthVertex);
            AssertDoubleEqual(original.Thickness, recreated.Thickness);
        });
    }
}