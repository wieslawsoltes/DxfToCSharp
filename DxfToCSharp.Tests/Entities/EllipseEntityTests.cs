using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class EllipseEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Ellipse_BasicRoundTrip_ShouldPreserveEllipseProperties()
    {
        // Arrange
        var originalEllipse = new Ellipse(
            new Vector3(10, 20, 0),
            15.0, // Major axis
            8.0   // Minor axis
        );

        // Act & Assert
        PerformRoundTripTest(originalEllipse, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.MajorAxis, recreated.MajorAxis);
            AssertDoubleEqual(original.MinorAxis, recreated.MinorAxis);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
        });
    }

    [Fact]
    public void Ellipse_WithAngles_ShouldPreserveAngles()
    {
        // Arrange
        var originalEllipse = new Ellipse(
            new Vector3(0, 0, 0),
            20.0,
            10.0)
        {
            StartAngle = Math.PI / 6,  // 30 degrees
            EndAngle = Math.PI * 5 / 6 // 150 degrees
        };

        // Act & Assert
        PerformRoundTripTest(originalEllipse, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.MajorAxis, recreated.MajorAxis);
            AssertDoubleEqual(original.MinorAxis, recreated.MinorAxis);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
        });
    }

    [Fact]
    public void Ellipse_3D_ShouldPreserve3DCenter()
    {
        // Arrange
        var originalEllipse = new Ellipse(
            new Vector3(5, 10, 25),
            12.0,
            6.0);

        // Act & Assert
        PerformRoundTripTest(originalEllipse, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.MajorAxis, recreated.MajorAxis);
            AssertDoubleEqual(original.MinorAxis, recreated.MinorAxis);
        });
    }

    [Fact]
    public void Ellipse_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("EllipseLayer")
        {
            Color = new AciColor(2), // Yellow
            Lineweight = Lineweight.Default
        };
        
        var originalEllipse = new Ellipse(
            new Vector3(0, 0, 0),
            8.0,
            4.0)
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalEllipse, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.MajorAxis, recreated.MajorAxis);
            AssertDoubleEqual(original.MinorAxis, recreated.MinorAxis);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void Ellipse_VerySmallAxes_ShouldPreserveSmallAxes()
    {
        // Arrange
        var originalEllipse = new Ellipse(
            new Vector3(0, 0, 0),
            0.001, // Very small major axis
            0.0005 // Very small minor axis
        );

        // Act & Assert
        PerformRoundTripTest(originalEllipse, (original, recreated) =>
        {
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.MajorAxis, recreated.MajorAxis, 1e-15);
            AssertDoubleEqual(original.MinorAxis, recreated.MinorAxis, 1e-15);
        });
    }
}