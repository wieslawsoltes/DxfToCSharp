using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;

namespace DxfToCSharp.Tests.Entities;

public class TraceEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Trace_BasicRoundTrip_ShouldPreserveTraceVertices()
    {
        // Arrange
        var originalTrace = new Trace(
            new Vector2(0, 0),
            new Vector2(10, 0),
            new Vector2(10, 5),
            new Vector2(0, 5));

        // Act & Assert
        PerformRoundTripTest(originalTrace, (original, recreated) =>
        {
            AssertVector2Equal(original.FirstVertex, recreated.FirstVertex);
            AssertVector2Equal(original.SecondVertex, recreated.SecondVertex);
            AssertVector2Equal(original.ThirdVertex, recreated.ThirdVertex);
            AssertVector2Equal(original.FourthVertex, recreated.FourthVertex);
        });
    }

    [Fact]
    public void Trace_WithDifferentVertices_ShouldPreserveDifferentVertices()
    {
        // Arrange
        var originalTrace = new Trace(
            new Vector2(-5, -10),
            new Vector2(15, -10),
            new Vector2(20, 8),
            new Vector2(-10, 8));

        // Act & Assert
        PerformRoundTripTest(originalTrace, (original, recreated) =>
        {
            AssertVector2Equal(original.FirstVertex, recreated.FirstVertex);
            AssertVector2Equal(original.SecondVertex, recreated.SecondVertex);
            AssertVector2Equal(original.ThirdVertex, recreated.ThirdVertex);
            AssertVector2Equal(original.FourthVertex, recreated.FourthVertex);
        });
    }

    [Fact]
    public void Trace_TriangularShape_ShouldPreserveTriangularShape()
    {
        // Arrange - Create a triangular trace by repeating one vertex
        var originalTrace = new Trace(
            new Vector2(0, 0),
            new Vector2(10, 0),
            new Vector2(5, 10),
            new Vector2(5, 10)); // Fourth vertex same as third for triangle

        // Act & Assert
        PerformRoundTripTest(originalTrace, (original, recreated) =>
        {
            AssertVector2Equal(original.FirstVertex, recreated.FirstVertex);
            AssertVector2Equal(original.SecondVertex, recreated.SecondVertex);
            AssertVector2Equal(original.ThirdVertex, recreated.ThirdVertex);
            AssertVector2Equal(original.FourthVertex, recreated.FourthVertex);
        });
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
