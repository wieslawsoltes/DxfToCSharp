using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Tables;
using PointEntity = netDxf.Entities.Point;

namespace DxfToCSharp.Tests.Entities;

public class PointEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Point_BasicRoundTrip_ShouldPreservePosition()
    {
        // Arrange
        var originalPoint = new PointEntity(
            new Vector3(25.5, 30.7, 0));

        // Act & Assert
        PerformRoundTripTest(originalPoint, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
        });
    }

    [Fact]
    public void Point_With3DPosition_ShouldPreserveZValue()
    {
        // Arrange
        var originalPoint = new PointEntity(
            new Vector3(10, 20, 35.5));

        // Act & Assert
        PerformRoundTripTest(originalPoint, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
        });
    }

    [Fact]
    public void Point_AtOrigin_ShouldPreserveOriginPosition()
    {
        // Arrange
        var originalPoint = new PointEntity(
            new Vector3(0, 0, 0));

        // Act & Assert
        PerformRoundTripTest(originalPoint, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
        });
    }

    [Fact]
    public void Point_WithNegativeCoordinates_ShouldPreserveNegativeValues()
    {
        // Arrange
        var originalPoint = new PointEntity(
            new Vector3(-25.5, -30.7, -10.2));

        // Act & Assert
        PerformRoundTripTest(originalPoint, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
        });
    }

    [Fact]
    public void Point_WithVerySmallCoordinates_ShouldPreservePrecision()
    {
        // Arrange
        var originalPoint = new PointEntity(
            new Vector3(0.000001, 0.000002, 0.000003));

        // Act & Assert
        PerformRoundTripTest(originalPoint, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position, 1e-15);
        });
    }

    [Fact]
    public void Point_WithVeryLargeCoordinates_ShouldPreserveLargeValues()
    {
        // Arrange
        var originalPoint = new PointEntity(
            new Vector3(1000000.123456, 2000000.654321, 3000000.987654));

        // Act & Assert
        PerformRoundTripTest(originalPoint, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
        });
    }

    [Fact]
    public void Point_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("PointLayer")
        {
            Color = new AciColor(7), // White
            Lineweight = Lineweight.Default
        };

        var originalPoint = new PointEntity(
            new Vector3(50, 50, 0))
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalPoint, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void Point_WithCustomColor_ShouldPreserveColor()
    {
        // Arrange
        var originalPoint = new PointEntity(
            new Vector3(0, 0, 0))
        {
            Color = new AciColor(2) // Yellow
        };

        // Act & Assert
        PerformRoundTripTest(originalPoint, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
        });
    }

    [Fact]
    public void Point_WithCustomLineweight_ShouldPreserveLineweight()
    {
        // Arrange
        var originalPoint = new PointEntity(
            new Vector3(100, 200, 0))
        {
            Lineweight = Lineweight.Default
        };

        // Act & Assert
        PerformRoundTripTest(originalPoint, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
            Assert.Equal(original.Lineweight, recreated.Lineweight);
        });
    }

    [Fact]
    public void Point_WithLinetypeScale_ShouldPreserveLinetypeScale()
    {
        // Arrange
        var originalPoint = new PointEntity(
            new Vector3(0, 0, 0))
        {
            LinetypeScale = 2.0
        };

        // Act & Assert
        PerformRoundTripTest(originalPoint, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.LinetypeScale, recreated.LinetypeScale);
        });
    }

    [Fact]
    public void Point_WithCustomNormal_ShouldPreserveNormal()
    {
        // Arrange
        var originalPoint = new PointEntity(
            new Vector3(0, 0, 0))
        {
            Normal = new Vector3(0.707, 0.707, 0) // Custom normal vector
        };

        // Act & Assert
        PerformRoundTripTest(originalPoint, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
            AssertVector3Equal(original.Normal, recreated.Normal);
        });
    }

    [Fact]
    public void Point_WithPreciseCoordinates_ShouldPreserveCoordinatePrecision()
    {
        // Arrange
        var originalPoint = new PointEntity(
            new Vector3(12.345678901234567, 98.765432109876543, 45.123456789012345));

        // Act & Assert
        PerformRoundTripTest(originalPoint, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position, 1e-12);
        });
    }

    [Fact]
    public void Point_WithByBlockColor_ShouldPreserveByBlockColor()
    {
        // Arrange
        var originalPoint = new PointEntity(
            new Vector3(10, 20, 30))
        {
            Color = AciColor.ByBlock
        };

        // Act & Assert
        PerformRoundTripTest(originalPoint, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
        });
    }

    [Fact]
    public void Point_WithByLayerColor_ShouldPreserveByLayerColor()
    {
        // Arrange
        var originalPoint = new PointEntity(
            new Vector3(10, 20, 30))
        {
            Color = AciColor.ByLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalPoint, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
        });
    }
}
