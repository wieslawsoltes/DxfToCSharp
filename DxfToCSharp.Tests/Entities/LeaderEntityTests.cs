using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class LeaderEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Leader_BasicRoundTrip_ShouldPreserveVertices()
    {
        // Arrange
        var vertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(50, 25),
            new Vector2(100, 50)
        };
        var originalLeader = new Leader(vertices);

        // Act & Assert
        PerformRoundTripTest(originalLeader, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            for (var i = 0; i < original.Vertexes.Count; i++)
            {
                AssertDoubleEqual(original.Vertexes[i].X, recreated.Vertexes[i].X);
                AssertDoubleEqual(original.Vertexes[i].Y, recreated.Vertexes[i].Y);
            }
        });
    }

    [Fact]
    public void Leader_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("LeaderLayer")
        {
            Color = new AciColor(2), // Yellow
            Lineweight = Lineweight.W30
        };
        
        var vertices = new List<Vector2>
        {
            new Vector2(10, 10),
            new Vector2(30, 20),
            new Vector2(50, 30)
        };
        
        var originalLeader = new Leader(vertices)
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalLeader, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void Leader_WithCustomColor_ShouldPreserveColor()
    {
        // Arrange
        var vertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(25, 12.5),
            new Vector2(50, 25)
        };
        
        var originalLeader = new Leader(vertices)
        {
            Color = new AciColor(4) // Cyan
        };

        // Act & Assert
        PerformRoundTripTest(originalLeader, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
        });
    }

    [Fact]
    public void Leader_SingleSegment_ShouldPreserveGeometry()
    {
        // Arrange
        var vertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(100, 0)
        };
        var originalLeader = new Leader(vertices);

        // Act & Assert
        PerformRoundTripTest(originalLeader, (original, recreated) =>
        {
            Assert.Equal(2, recreated.Vertexes.Count);
            AssertDoubleEqual(original.Vertexes[0].X, recreated.Vertexes[0].X);
            AssertDoubleEqual(original.Vertexes[0].Y, recreated.Vertexes[0].Y);
            AssertDoubleEqual(original.Vertexes[1].X, recreated.Vertexes[1].X);
            AssertDoubleEqual(original.Vertexes[1].Y, recreated.Vertexes[1].Y);
        });
    }

    [Fact]
    public void Leader_MultipleSegments_ShouldPreserveAllVertices()
    {
        // Arrange
        var vertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(25, 25),
            new Vector2(50, 25),
            new Vector2(75, 50),
            new Vector2(100, 50)
        };
        var originalLeader = new Leader(vertices);

        // Act & Assert
        PerformRoundTripTest(originalLeader, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            for (var i = 0; i < original.Vertexes.Count; i++)
            {
                AssertDoubleEqual(original.Vertexes[i].X, recreated.Vertexes[i].X);
                AssertDoubleEqual(original.Vertexes[i].Y, recreated.Vertexes[i].Y);
            }
        });
    }
}