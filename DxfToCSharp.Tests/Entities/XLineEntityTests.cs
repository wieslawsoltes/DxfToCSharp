using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class XLineEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void XLine_BasicRoundTrip_ShouldPreserveGeometry()
    {
        // Arrange
        var originalXLine = new XLine(
            new Vector3(0, 0, 0),
            new Vector3(1, 1, 0));

        // Act & Assert
        PerformRoundTripTest(originalXLine, (original, recreated) =>
        {
            AssertVector3Equal(original.Origin, recreated.Origin);
            AssertVector3Equal(original.Direction, recreated.Direction);
        });
    }

    [Fact]
    public void XLine_With3DDirection_ShouldPreserveZValues()
    {
        // Arrange
        var originalXLine = new XLine(
            new Vector3(15, 25, 35),
            new Vector3(0.577, 0.577, 0.577)); // Normalized direction

        // Act & Assert
        PerformRoundTripTest(originalXLine, (original, recreated) =>
        {
            AssertVector3Equal(original.Origin, recreated.Origin);
            AssertVector3Equal(original.Direction, recreated.Direction);
        });
    }

    [Fact]
    public void XLine_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("XLineLayer")
        {
            Color = new AciColor(5), // Blue
            Lineweight = Lineweight.W35
        };
        
        var originalXLine = new XLine(
            new Vector3(10, 10, 0),
            new Vector3(1, 0, 0))
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalXLine, (original, recreated) =>
        {
            AssertVector3Equal(original.Origin, recreated.Origin);
            AssertVector3Equal(original.Direction, recreated.Direction);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void XLine_WithCustomColor_ShouldPreserveColor()
    {
        // Arrange
        var originalXLine = new XLine(
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0))
        {
            Color = new AciColor(6) // Magenta
        };

        // Act & Assert
        PerformRoundTripTest(originalXLine, (original, recreated) =>
        {
            AssertVector3Equal(original.Origin, recreated.Origin);
            AssertVector3Equal(original.Direction, recreated.Direction);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
        });
    }

    [Fact]
    public void XLine_VerticalLine_ShouldPreserveDirection()
    {
        // Arrange
        var originalXLine = new XLine(
            new Vector3(25, 25, 0),
            new Vector3(0, 1, 0)); // Vertical direction

        // Act & Assert
        PerformRoundTripTest(originalXLine, (original, recreated) =>
        {
            AssertVector3Equal(original.Origin, recreated.Origin);
            AssertVector3Equal(original.Direction, recreated.Direction);
        });
    }

    [Fact]
    public void XLine_HorizontalLine_ShouldPreserveDirection()
    {
        // Arrange
        var originalXLine = new XLine(
            new Vector3(30, 40, 0),
            new Vector3(1, 0, 0)); // Horizontal direction

        // Act & Assert
        PerformRoundTripTest(originalXLine, (original, recreated) =>
        {
            AssertVector3Equal(original.Origin, recreated.Origin);
            AssertVector3Equal(original.Direction, recreated.Direction);
        });
    }
}