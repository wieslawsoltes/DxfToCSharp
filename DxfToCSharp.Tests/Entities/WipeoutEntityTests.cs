using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class WipeoutEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Wipeout_BasicRoundTrip_ShouldPreserveBoundary()
    {
        // Arrange
        var boundaryVertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(100, 0),
            new Vector2(100, 100),
            new Vector2(0, 100)
        };
        var originalWipeout = new Wipeout(boundaryVertices);

        // Act & Assert
        PerformRoundTripTest(originalWipeout, (original, recreated) =>
        {
            Assert.Equal(original.ClippingBoundary.Vertexes.Count, recreated.ClippingBoundary.Vertexes.Count);
            for (var i = 0; i < original.ClippingBoundary.Vertexes.Count; i++)
            {
                var originalVertex = original.ClippingBoundary.Vertexes[i];
                var recreatedVertex = recreated.ClippingBoundary.Vertexes[i];
                AssertDoubleEqual(originalVertex.X, recreatedVertex.X);
                AssertDoubleEqual(originalVertex.Y, recreatedVertex.Y);
            }
        });
    }

    [Fact]
    public void Wipeout_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("WipeoutLayer")
        {
            Color = new AciColor(7), // White
            Lineweight = Lineweight.Default
        };
        
        var boundaryVertices = new List<Vector2>
        {
            new Vector2(10, 10),
            new Vector2(50, 10),
            new Vector2(50, 50),
            new Vector2(10, 50)
        };
        
        var originalWipeout = new Wipeout(boundaryVertices)
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalWipeout, (original, recreated) =>
        {
            Assert.Equal(original.ClippingBoundary.Vertexes.Count, recreated.ClippingBoundary.Vertexes.Count);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void Wipeout_TriangularBoundary_ShouldPreserveShape()
    {
        // Arrange
        var boundaryVertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(50, 0),
            new Vector2(25, 43.3) // Approximate equilateral triangle
        };
        var originalWipeout = new Wipeout(boundaryVertices);

        // Act & Assert
        PerformRoundTripTest(originalWipeout, (original, recreated) =>
        {
            Assert.Equal(original.ClippingBoundary.Vertexes.Count, recreated.ClippingBoundary.Vertexes.Count);
            for (var i = 0; i < original.ClippingBoundary.Vertexes.Count; i++)
            {
                var originalVertex = original.ClippingBoundary.Vertexes[i];
                var recreatedVertex = recreated.ClippingBoundary.Vertexes[i];
                AssertDoubleEqual(originalVertex.X, recreatedVertex.X);
                AssertDoubleEqual(originalVertex.Y, recreatedVertex.Y);
            }
        });
    }

    [Fact]
    public void Wipeout_ComplexPolygon_ShouldPreserveAllVertices()
    {
        // Arrange
        var boundaryVertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(20, 5),
            new Vector2(40, 0),
            new Vector2(45, 20),
            new Vector2(40, 40),
            new Vector2(20, 35),
            new Vector2(0, 40),
            new Vector2(-5, 20)
        };
        var originalWipeout = new Wipeout(boundaryVertices);

        // Act & Assert
        PerformRoundTripTest(originalWipeout, (original, recreated) =>
        {
            Assert.Equal(original.ClippingBoundary.Vertexes.Count, recreated.ClippingBoundary.Vertexes.Count);
            for (var i = 0; i < original.ClippingBoundary.Vertexes.Count; i++)
            {
            var originalVertex = original.ClippingBoundary.Vertexes[i];
                var recreatedVertex = recreated.ClippingBoundary.Vertexes[i];
                AssertDoubleEqual(originalVertex.X, recreatedVertex.X);
                AssertDoubleEqual(originalVertex.Y, recreatedVertex.Y);
            }
        });
    }
}