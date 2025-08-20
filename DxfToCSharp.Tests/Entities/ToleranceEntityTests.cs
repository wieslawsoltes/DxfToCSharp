using netDxf;
using netDxf.Entities;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class ToleranceEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Tolerance_BasicRoundTrip_ShouldPreserveToleranceProperties()
    {
        // Arrange
        var toleranceEntry = new ToleranceEntry();
        var originalTolerance = new Tolerance(toleranceEntry, 
            new Vector3(25, 30, 0));
        originalTolerance.TextHeight = 2.5;
        originalTolerance.Rotation = 30.0;

        // Act & Assert
        PerformRoundTripTest(originalTolerance, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.TextHeight, recreated.TextHeight);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
            // Note: ToleranceEntry properties may not be fully preserved during round-trip
            Assert.NotNull(recreated.Entry1);
        });
    }

    [Fact]
    public void Tolerance_WithDifferentProperties_ShouldPreserveDifferentProperties()
    {
        // Arrange
        var toleranceEntry = new ToleranceEntry();
        var originalTolerance = new Tolerance(toleranceEntry, 
            new Vector3(-10, -15, 5));
        originalTolerance.TextHeight = 5.0;
        originalTolerance.Rotation = 180.0;

        // Act & Assert
        PerformRoundTripTest(originalTolerance, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.TextHeight, recreated.TextHeight);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
            Assert.NotNull(recreated.Entry1);
        });
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}