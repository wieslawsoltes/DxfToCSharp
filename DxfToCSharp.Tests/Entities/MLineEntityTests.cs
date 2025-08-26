using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class MLineEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void MLine_BasicRoundTrip_ShouldPreserveVertices()
    {
        // Arrange
        var vertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(50, 25),
            new Vector2(100, 50)
        };
        var originalMLine = new MLine(vertices);

        // Act & Assert
        PerformRoundTripTest(originalMLine, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            for (var i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector2Equal(original.Vertexes[i].Position, recreated.Vertexes[i].Position);
            }
        });
    }

    [Fact]
    public void MLine_With3DCoordinates_ShouldPreserveZValues()
    {
        // Arrange
        var vertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(25, 25),
            new Vector2(50, 50),
            new Vector2(75, 25)
        };
        var originalMLine = new MLine(vertices);

        // Act & Assert
        PerformRoundTripTest(originalMLine, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            for (var i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector2Equal(original.Vertexes[i].Position, recreated.Vertexes[i].Position);
            }
        });
    }

    [Fact]
    public void MLine_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("MLineLayer")
        {
            Color = new AciColor(4), // Cyan
            Lineweight = Lineweight.W50
        };
        
        var vertices = new List<Vector2>
        {
            new Vector2(10, 10),
            new Vector2(30, 20),
            new Vector2(50, 30)
        };
        
        var originalMLine = new MLine(vertices)
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalMLine, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void MLine_WithCustomColor_ShouldPreserveColor()
    {
        // Arrange
        var vertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(25, 12.5),
            new Vector2(50, 25)
        };
        
        var originalMLine = new MLine(vertices)
        {
            Color = new AciColor(5) // Blue
        };

        // Act & Assert
        PerformRoundTripTest(originalMLine, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
        });
    }

    [Fact]
    public void MLine_SingleSegment_ShouldPreserveGeometry()
    {
        // Arrange
        var vertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(100, 0)
        };
        var originalMLine = new MLine(vertices);

        // Act & Assert
        PerformRoundTripTest(originalMLine, (original, recreated) =>
        {
            Assert.Equal(2, recreated.Vertexes.Count);
            AssertVector2Equal(original.Vertexes[0].Position, recreated.Vertexes[0].Position);
            AssertVector2Equal(original.Vertexes[1].Position, recreated.Vertexes[1].Position);
        });
    }

    [Fact]
    public void MLine_MultipleSegments_ShouldPreserveAllVertices()
    {
        // Arrange
        var vertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(20, 20),
            new Vector2(40, 20),
            new Vector2(60, 40),
            new Vector2(80, 40),
            new Vector2(100, 60)
        };
        var originalMLine = new MLine(vertices);

        // Act & Assert
        PerformRoundTripTest(originalMLine, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            for (var i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector2Equal(original.Vertexes[i].Position, recreated.Vertexes[i].Position);
            }
        });
    }
}