using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;
using Xunit;
using System;
using System.IO;
using System.Linq;

namespace DxfToCSharp.Tests.Entities;

public class ShapeEntityTests : RoundTripTestBase, IDisposable
{
    [Fact(Skip = "Shape entities require external SHX files that are not available in test environment")]
    public void Shape_BasicRoundTrip_ShouldPreserveShapeProperties()
    {
        // Arrange
        var shapeStyle = new ShapeStyle("TestStyle", "test.shx");
        var originalShape = new Shape("TestShape", shapeStyle,
            new Vector3(10, 20, 0),
            5.0, 45.0);
        // Rotation is set in constructor
        originalShape.WidthFactor = 1.5;
        originalShape.ObliqueAngle = 15.0;



        // Act & Assert
        PerformRoundTripTest(originalShape, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Size, recreated.Size);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
            AssertDoubleEqual(original.WidthFactor, recreated.WidthFactor);
            AssertDoubleEqual(original.ObliqueAngle, recreated.ObliqueAngle);
        });
    }

    [Fact(Skip = "Shape entities require external SHX files that are not available in test environment")]
    public void Shape_WithDifferentProperties_ShouldPreserveDifferentProperties()
    {
        // Arrange
        var shapeStyle = new ShapeStyle("AnotherStyle", "test.shx");
        var originalShape = new Shape("AnotherShape", shapeStyle,
            new Vector3(-5, -10, 2),
            12.5, 90.0);
        // Rotation is set in constructor
        originalShape.WidthFactor = 0.8;
        originalShape.ObliqueAngle = -10.0;

        // Act & Assert
        PerformRoundTripTest(originalShape, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Size, recreated.Size);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
            AssertDoubleEqual(original.WidthFactor, recreated.WidthFactor);
            AssertDoubleEqual(original.ObliqueAngle, recreated.ObliqueAngle);
        });
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}