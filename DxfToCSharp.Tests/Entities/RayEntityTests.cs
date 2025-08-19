using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;
using Xunit;

namespace DxfToCSharp.Tests.Entities;

public class RayEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Ray_BasicRoundTrip_ShouldPreserveGeometry()
    {
        // Arrange
        var originalRay = new Ray(
            new Vector3(0, 0, 0),
            new Vector3(1, 1, 0));

        // Act & Assert
        PerformRoundTripTest(originalRay, (original, recreated) =>
        {
            AssertVector3Equal(original.Origin, recreated.Origin);
            AssertVector3Equal(original.Direction, recreated.Direction);
        });
    }

    [Fact]
    public void Ray_With3DDirection_ShouldPreserveZValues()
    {
        // Arrange
        var originalRay = new Ray(
            new Vector3(10, 20, 30),
            new Vector3(0.5, 0.5, 0.707));

        // Act & Assert
        PerformRoundTripTest(originalRay, (original, recreated) =>
        {
            AssertVector3Equal(original.Origin, recreated.Origin);
            AssertVector3Equal(original.Direction, recreated.Direction);
        });
    }

    [Fact]
    public void Ray_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("RayLayer")
        {
            Color = new AciColor(4), // Cyan
            Lineweight = Lineweight.W25
        };
        
        var originalRay = new Ray(
            new Vector3(5, 5, 0),
            new Vector3(1, 0, 0))
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalRay, (original, recreated) =>
        {
            AssertVector3Equal(original.Origin, recreated.Origin);
            AssertVector3Equal(original.Direction, recreated.Direction);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void Ray_WithCustomColor_ShouldPreserveColor()
    {
        // Arrange
        var originalRay = new Ray(
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0))
        {
            Color = new AciColor(2) // Yellow
        };

        // Act & Assert
        PerformRoundTripTest(originalRay, (original, recreated) =>
        {
            AssertVector3Equal(original.Origin, recreated.Origin);
            AssertVector3Equal(original.Direction, recreated.Direction);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
        });
    }

    [Fact]
    public void Ray_NegativeDirection_ShouldPreserveDirection()
    {
        // Arrange
        var originalRay = new Ray(
            new Vector3(50, 50, 0),
            new Vector3(-1, -1, 0));

        // Act & Assert
        PerformRoundTripTest(originalRay, (original, recreated) =>
        {
            AssertVector3Equal(original.Origin, recreated.Origin);
            AssertVector3Equal(original.Direction, recreated.Direction);
        });
    }
}