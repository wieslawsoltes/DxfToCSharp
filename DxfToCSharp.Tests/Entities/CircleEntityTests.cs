using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;
using Xunit;

namespace DxfToCSharp.Tests.Entities;

public class CircleEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Circle_BasicRoundTrip_ShouldPreserveGeometry()
    {
        // Arrange
        var originalCircle = new Circle(
            new Vector3(25.5, 30.7, 0),
            15.25);

        // Act & Assert
        PerformRoundTripTest(originalCircle, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
        });
    }

    [Fact]
    public void Circle_With3DCenter_ShouldPreserveZValue()
    {
        // Arrange
        var originalCircle = new Circle(
            new Vector3(10, 20, 35.5),
            12.0);

        // Act & Assert
        PerformRoundTripTest(originalCircle, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
        });
    }

    [Fact]
    public void Circle_WithSmallRadius_ShouldPreservePrecision()
    {
        // Arrange
        var originalCircle = new Circle(
            new Vector3(0, 0, 0),
            0.0001);

        // Act & Assert
        PerformRoundTripTest(originalCircle, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius, 1e-15);
        });
    }

    [Fact]
    public void Circle_WithLargeRadius_ShouldPreserveLargeValues()
    {
        // Arrange
        var originalCircle = new Circle(
            new Vector3(1000000, 2000000, 0),
            999999.123456);

        // Act & Assert
        PerformRoundTripTest(originalCircle, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
        });
    }

    [Fact]
    public void Circle_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("CircleLayer")
        {
            Color = new AciColor(6), // Magenta
            Lineweight = Lineweight.Default
        };
        
        var originalCircle = new Circle(
            new Vector3(50, 50, 0),
            25.0)
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalCircle, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void Circle_WithCustomColor_ShouldPreserveColor()
    {
        // Arrange
        var originalCircle = new Circle(
            new Vector3(0, 0, 0),
            10.0)
        {
            Color = new AciColor(1) // Red
        };

        // Act & Assert
        PerformRoundTripTest(originalCircle, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
        });
    }

    [Fact]
    public void Circle_WithThickness_ShouldPreserveThickness()
    {
        // Arrange
        var originalCircle = new Circle(
            new Vector3(0, 0, 0),
            20.0)
        {
            Thickness = 7.5
        };

        // Act & Assert
        PerformRoundTripTest(originalCircle, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.Thickness, recreated.Thickness);
        });
    }

    [Fact]
    public void Circle_WithCustomLineweight_ShouldPreserveLineweight()
    {
        // Arrange
        var originalCircle = new Circle(
            new Vector3(100, 200, 0),
            30.0)
        {
            Lineweight = Lineweight.Default
        };

        // Act & Assert
        PerformRoundTripTest(originalCircle, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            Assert.Equal(original.Lineweight, recreated.Lineweight);
        });
    }

    [Fact]
    public void Circle_WithLinetypeScale_ShouldPreserveLinetypeScale()
    {
        // Arrange
        var originalCircle = new Circle(
            new Vector3(0, 0, 0),
            15.0)
        {
            LinetypeScale = 1.5
        };

        // Act & Assert
        PerformRoundTripTest(originalCircle, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.LinetypeScale, recreated.LinetypeScale);
        });
    }

    [Fact]
    public void Circle_WithCustomNormal_ShouldPreserveNormal()
    {
        // Arrange
        var originalCircle = new Circle(
            new Vector3(0, 0, 0),
            18.0)
        {
            Normal = new Vector3(0.577, 0.577, 0.577) // Custom normal vector
        };

        // Act & Assert
        PerformRoundTripTest(originalCircle, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertVector3Equal(original.Normal, recreated.Normal);
        });
    }

    [Fact]
    public void Circle_WithNegativeCenter_ShouldPreserveNegativeValues()
    {
        // Arrange
        var originalCircle = new Circle(
            new Vector3(-25.5, -30.7, -10.2),
            8.5);

        // Act & Assert
        PerformRoundTripTest(originalCircle, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
        });
    }

    [Fact]
    public void Circle_WithVerySmallCenter_ShouldPreservePrecision()
    {
        // Arrange
        var originalCircle = new Circle(
            new Vector3(0.000001, 0.000002, 0.000003),
            0.000005);

        // Act & Assert
        PerformRoundTripTest(originalCircle, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center, 1e-15);
            AssertDoubleEqual(original.Radius, recreated.Radius, 1e-15);
        });
    }

    [Fact]
    public void Circle_WithPreciseRadius_ShouldPreserveRadiusPrecision()
    {
        // Arrange
        var originalCircle = new Circle(
            new Vector3(0, 0, 0),
            12.345678901234567);

        // Act & Assert
        PerformRoundTripTest(originalCircle, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius, 1e-12);
        });
    }

    [Fact]
    public void Circle_AtOrigin_ShouldPreserveOriginPosition()
    {
        // Arrange
        var originalCircle = new Circle(
            new Vector3(0, 0, 0),
            5.0);

        // Act & Assert
        PerformRoundTripTest(originalCircle, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Radius, recreated.Radius);
        });
    }
}