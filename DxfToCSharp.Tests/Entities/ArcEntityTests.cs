using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class ArcEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Arc_BasicRoundTrip_ShouldPreserveGeometry()
    {
        // Arrange
        var originalArc = new Arc(
            new Vector3(50, 50, 0),
            25.5,
            30.0,
            120.0);

        // Act & Assert
        PerformRoundTripTest(originalArc, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
        });
    }

    [Fact]
    public void Arc_FullCircle_ShouldPreserveAngles()
    {
        // Arrange
        var originalArc = new Arc(
            new Vector3(0, 0, 0),
            10.0,
            0.0,
            360.0);

        // Act & Assert
        PerformRoundTripTest(originalArc, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
        });
    }

    [Fact]
    public void Arc_With3DCenter_ShouldPreserveZValue()
    {
        // Arrange
        var originalArc = new Arc(
            new Vector3(10, 20, 15.5),
            30.0,
            45.0,
            135.0);

        // Act & Assert
        PerformRoundTripTest(originalArc, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
        });
    }

    [Fact]
    public void Arc_WithSmallRadius_ShouldPreservePrecision()
    {
        // Arrange
        var originalArc = new Arc(
            new Vector3(0, 0, 0),
            0.001,
            0.0,
            90.0);

        // Act & Assert
        PerformRoundTripTest(originalArc, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius, 1e-15);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
        });
    }

    [Fact]
    public void Arc_WithLargeRadius_ShouldPreserveLargeValues()
    {
        // Arrange
        var originalArc = new Arc(
            new Vector3(1000, 2000, 0),
            50000.123456,
            15.0,
            285.0);

        // Act & Assert
        PerformRoundTripTest(originalArc, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
        });
    }

    [Fact]
    public void Arc_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("ArcLayer")
        {
            Color = new AciColor(2), // Yellow
            Lineweight = Lineweight.Default
        };
        
        var originalArc = new Arc(
            new Vector3(25, 25, 0),
            15.0,
            0.0,
            180.0)
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalArc, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void Arc_WithCustomColor_ShouldPreserveColor()
    {
        // Arrange
        var originalArc = new Arc(
            new Vector3(0, 0, 0),
            20.0,
            90.0,
            270.0)
        {
            Color = new AciColor(4) // Cyan
        };

        // Act & Assert
        PerformRoundTripTest(originalArc, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
        });
    }

    [Fact]
    public void Arc_WithThickness_ShouldPreserveThickness()
    {
        // Arrange
        var originalArc = new Arc(
            new Vector3(0, 0, 0),
            10.0,
            0.0,
            90.0)
        {
            Thickness = 3.5
        };

        // Act & Assert
        PerformRoundTripTest(originalArc, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
            AssertDoubleEqual(original.Thickness, recreated.Thickness);
        });
    }

    [Fact]
    public void Arc_WithNegativeAngles_ShouldPreserveNegativeValues()
    {
        // Arrange
        var originalArc = new Arc(
            new Vector3(0, 0, 0),
            15.0,
            -45.0,
            -15.0);

        // Act & Assert
        PerformRoundTripTest(originalArc, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
        });
    }

    [Fact]
    public void Arc_WithAnglesGreaterThan360_ShouldPreserveLargeAngles()
    {
        // Arrange
        var originalArc = new Arc(
            new Vector3(0, 0, 0),
            12.0,
            390.0,
            450.0);

        // Act & Assert
        PerformRoundTripTest(originalArc, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
        });
    }

    [Fact]
    public void Arc_WithCustomNormal_ShouldPreserveNormal()
    {
        // Arrange
        var originalArc = new Arc(
            new Vector3(0, 0, 0),
            25.0,
            0.0,
            180.0)
        {
            Normal = new Vector3(0.707, 0.707, 0) // Custom normal vector
        };

        // Act & Assert
        PerformRoundTripTest(originalArc, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
            AssertVector3Equal(original.Normal, recreated.Normal);
        });
    }

    [Fact]
    public void Arc_WithPreciseAngles_ShouldPreserveAnglePrecision()
    {
        // Arrange
        var originalArc = new Arc(
            new Vector3(0, 0, 0),
            10.0,
            12.345678901234,
            87.654321098765);

        // Act & Assert
        PerformRoundTripTest(originalArc, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle, 1e-12);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle, 1e-12);
        });
    }
}