using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class LineEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Line_BasicRoundTrip_ShouldPreserveGeometry()
    {
        // Arrange
        var originalLine = new Line(
            new Vector3(10.5, 20.3, 0),
            new Vector3(50.7, 80.9, 0));

        // Act & Assert
        PerformRoundTripTest(originalLine, (original, recreated) =>
        {
            AssertVector3Equal(original.StartPoint, recreated.StartPoint);
            AssertVector3Equal(original.EndPoint, recreated.EndPoint);
        });
    }

    [Fact]
    public void Line_With3DCoordinates_ShouldPreserveZValues()
    {
        // Arrange
        var originalLine = new Line(
            new Vector3(0, 0, 10.5),
            new Vector3(100, 200, 25.7));

        // Act & Assert
        PerformRoundTripTest(originalLine, (original, recreated) =>
        {
            AssertVector3Equal(original.StartPoint, recreated.StartPoint);
            AssertVector3Equal(original.EndPoint, recreated.EndPoint);
        });
    }

    [Fact]
    public void Line_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("TestLayer")
        {
            Color = new AciColor(5), // Blue
            Lineweight = Lineweight.Default
        };
        
        var originalLine = new Line(
            new Vector3(0, 0, 0),
            new Vector3(100, 100, 0))
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalLine, (original, recreated) =>
        {
            AssertVector3Equal(original.StartPoint, recreated.StartPoint);
            AssertVector3Equal(original.EndPoint, recreated.EndPoint);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void Line_WithCustomColor_ShouldPreserveColor()
    {
        // Arrange
        var originalLine = new Line(
            new Vector3(0, 0, 0),
            new Vector3(50, 50, 0))
        {
            Color = new AciColor(3) // Green
        };

        // Act & Assert
        PerformRoundTripTest(originalLine, (original, recreated) =>
        {
            AssertVector3Equal(original.StartPoint, recreated.StartPoint);
            AssertVector3Equal(original.EndPoint, recreated.EndPoint);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
        });
    }

    [Fact]
    public void Line_WithThickness_ShouldPreserveThickness()
    {
        // Arrange
        var originalLine = new Line(
            new Vector3(0, 0, 0),
            new Vector3(100, 0, 0))
        {
            Thickness = 5.5
        };

        // Act & Assert
        PerformRoundTripTest(originalLine, (original, recreated) =>
        {
            AssertVector3Equal(original.StartPoint, recreated.StartPoint);
            AssertVector3Equal(original.EndPoint, recreated.EndPoint);
            AssertDoubleEqual(original.Thickness, recreated.Thickness);
        });
    }

    [Fact]
    public void Line_WithCustomLineweight_ShouldPreserveLineweight()
    {
        // Arrange
        var originalLine = new Line(
            new Vector3(0, 0, 0),
            new Vector3(100, 100, 0))
        {
            Lineweight = Lineweight.Default
        };

        // Act & Assert
        PerformRoundTripTest(originalLine, (original, recreated) =>
        {
            AssertVector3Equal(original.StartPoint, recreated.StartPoint);
            AssertVector3Equal(original.EndPoint, recreated.EndPoint);
            Assert.Equal(original.Lineweight, recreated.Lineweight);
        });
    }

    [Fact]
    public void Line_WithLinetypeScale_ShouldPreserveLinetypeScale()
    {
        // Arrange
        var originalLine = new Line(
            new Vector3(0, 0, 0),
            new Vector3(200, 0, 0))
        {
            LinetypeScale = 2.5
        };

        // Act & Assert
        PerformRoundTripTest(originalLine, (original, recreated) =>
        {
            AssertVector3Equal(original.StartPoint, recreated.StartPoint);
            AssertVector3Equal(original.EndPoint, recreated.EndPoint);
            AssertDoubleEqual(original.LinetypeScale, recreated.LinetypeScale);
        });
    }

    [Fact]
    public void Line_WithCustomNormal_ShouldPreserveNormal()
    {
        // Arrange
        var originalLine = new Line(
            new Vector3(0, 0, 0),
            new Vector3(100, 100, 0))
        {
            Normal = new Vector3(0.5, 0.5, 0.707) // Custom normal vector
        };

        // Act & Assert
        PerformRoundTripTest(originalLine, (original, recreated) =>
        {
            AssertVector3Equal(original.StartPoint, recreated.StartPoint);
            AssertVector3Equal(original.EndPoint, recreated.EndPoint);
            AssertVector3Equal(original.Normal, recreated.Normal);
        });
    }

    [Fact]
    public void Line_WithNegativeCoordinates_ShouldPreserveNegativeValues()
    {
        // Arrange
        var originalLine = new Line(
            new Vector3(-50.5, -30.2, -10.1),
            new Vector3(-100.7, -200.9, -25.3));

        // Act & Assert
        PerformRoundTripTest(originalLine, (original, recreated) =>
        {
            AssertVector3Equal(original.StartPoint, recreated.StartPoint);
            AssertVector3Equal(original.EndPoint, recreated.EndPoint);
        });
    }

    [Fact]
    public void Line_WithVerySmallCoordinates_ShouldPreservePrecision()
    {
        // Arrange
        var originalLine = new Line(
            new Vector3(0.000001, 0.000002, 0.000003),
            new Vector3(0.000004, 0.000005, 0.000006));

        // Act & Assert
        PerformRoundTripTest(originalLine, (original, recreated) =>
        {
            AssertVector3Equal(original.StartPoint, recreated.StartPoint, 1e-15);
            AssertVector3Equal(original.EndPoint, recreated.EndPoint, 1e-15);
        });
    }

    [Fact]
    public void Line_WithVeryLargeCoordinates_ShouldPreserveLargeValues()
    {
        // Arrange
        var originalLine = new Line(
            new Vector3(1000000.123456, 2000000.654321, 3000000.987654),
            new Vector3(4000000.111111, 5000000.222222, 6000000.333333));

        // Act & Assert
        PerformRoundTripTest(originalLine, (original, recreated) =>
        {
            AssertVector3Equal(original.StartPoint, recreated.StartPoint);
            AssertVector3Equal(original.EndPoint, recreated.EndPoint);
        });
    }
}