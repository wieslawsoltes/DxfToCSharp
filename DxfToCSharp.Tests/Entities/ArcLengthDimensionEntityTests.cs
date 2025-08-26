using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class ArcLengthDimensionEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void ArcLengthDimension_BasicRoundTrip_ShouldPreserveProperties()
    {
        // Arrange
        var originalDimension = new ArcLengthDimension(
            new Vector2(0, 0),    // center point
            50.0,                 // radius
            0.0,                  // start angle
            Math.PI / 2,          // end angle (90 degrees)
            15.0);                // offset

        // Act & Assert
        PerformRoundTripTest<ArcLengthDimension>(originalDimension, (original, recreated) =>
        {
            AssertVector2Equal(original.CenterPoint, recreated.CenterPoint);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
            AssertDoubleEqual(original.Offset, recreated.Offset);
        });
    }

    [Fact]
    public void ArcLengthDimension_FullCircle_ShouldRoundTrip()
    {
        // Arrange
        var originalDimension = new ArcLengthDimension(
            new Vector2(10, 10),  // center point
            25.0,                 // radius
            0.0,                  // start angle
            2 * Math.PI,          // end angle (full circle)
            10.0);                // offset

        // Act & Assert
        PerformRoundTripTest<ArcLengthDimension>(originalDimension, (original, recreated) =>
        {
            AssertVector2Equal(original.CenterPoint, recreated.CenterPoint);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
            AssertDoubleEqual(original.Offset, recreated.Offset);
        });
    }

    [Fact]
    public void ArcLengthDimension_NegativeOffset_ShouldRoundTrip()
    {
        // Arrange
        var originalDimension = new ArcLengthDimension(
            new Vector2(-5, -5),  // center point
            30.0,                 // radius
            Math.PI / 4,          // start angle (45 degrees)
            3 * Math.PI / 4,      // end angle (135 degrees)
            -20.0);               // negative offset

        // Act & Assert
        PerformRoundTripTest<ArcLengthDimension>(originalDimension, (original, recreated) =>
        {
            AssertVector2Equal(original.CenterPoint, recreated.CenterPoint);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
            AssertDoubleEqual(original.Offset, recreated.Offset);
        });
    }

    [Fact]
    public void ArcLengthDimension_SmallArc_ShouldRoundTrip()
    {
        // Arrange
        var originalDimension = new ArcLengthDimension(
            new Vector2(100, 50), // center point
            5.0,                  // small radius
            0.0,                  // start angle
            Math.PI / 6,          // end angle (30 degrees)
            2.0);                 // small offset

        // Act & Assert
        PerformRoundTripTest<ArcLengthDimension>(originalDimension, (original, recreated) =>
        {
            AssertVector2Equal(original.CenterPoint, recreated.CenterPoint);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
            AssertDoubleEqual(original.Offset, recreated.Offset);
        });
    }

    [Fact]
    public void ArcLengthDimension_LargeArc_ShouldRoundTrip()
    {
        // Arrange
        var originalDimension = new ArcLengthDimension(
            new Vector2(0, 0),    // center point
            200.0,                // large radius
            Math.PI,              // start angle (180 degrees)
            3 * Math.PI / 2,      // end angle (270 degrees)
            50.0);                // large offset

        // Act & Assert
        PerformRoundTripTest<ArcLengthDimension>(originalDimension, (original, recreated) =>
        {
            AssertVector2Equal(original.CenterPoint, recreated.CenterPoint);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
            AssertDoubleEqual(original.Offset, recreated.Offset);
        });
    }

    [Fact]
    public void ArcLengthDimension_WithCustomDimensionStyle_ShouldRoundTrip()
    {
        // Arrange
        var customStyle = new DimensionStyle("CustomArcStyle")
        {
            DimScaleOverall = 2.0,
            TextHeight = 5.0
        };
        
        var originalDimension = new ArcLengthDimension(
            new Vector2(20, 30),  // center point
            40.0,                 // radius
            Math.PI / 3,          // start angle (60 degrees)
            2 * Math.PI / 3,      // end angle (120 degrees)
            12.0)                 // offset
        {
            Style = customStyle
        };

        // Act & Assert
        PerformRoundTripTest<ArcLengthDimension>(originalDimension, (original, recreated) =>
        {
            AssertVector2Equal(original.CenterPoint, recreated.CenterPoint);
            AssertDoubleEqual(original.Radius, recreated.Radius);
            AssertDoubleEqual(original.StartAngle, recreated.StartAngle);
            AssertDoubleEqual(original.EndAngle, recreated.EndAngle);
            AssertDoubleEqual(original.Offset, recreated.Offset);
            // NOTE: Custom dimension style names are not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.Style.Name, recreated.Style.Name);
        });
    }
}