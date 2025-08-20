using netDxf;
using netDxf.Entities;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class PolygonMeshEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void PolygonMesh_BasicRoundTrip_ShouldPreservePolygonMeshProperties()
    {
        // Arrange
        short u = 4;
        short v = 3;
        var vertices = new List<Vector3>
        {
            // Row 0
            new Vector3(0, 0, 0),
            new Vector3(10, 0, 0),
            new Vector3(20, 0, 0),
            new Vector3(30, 0, 0),
            // Row 1
            new Vector3(0, 10, 2),
            new Vector3(10, 10, 3),
            new Vector3(20, 10, 3),
            new Vector3(30, 10, 2),
            // Row 2
            new Vector3(0, 20, 0),
            new Vector3(10, 20, 0),
            new Vector3(20, 20, 0),
            new Vector3(30, 20, 0)
        };
        
        var originalPolygonMesh = new PolygonMesh(u, v, vertices);
        originalPolygonMesh.DensityU = 6;
        originalPolygonMesh.DensityV = 6;
        originalPolygonMesh.SmoothType = PolylineSmoothType.Quadratic;
        originalPolygonMesh.IsClosedInU = true;
        originalPolygonMesh.IsClosedInV = false;

        // Act & Assert
        PerformRoundTripTest(originalPolygonMesh, (original, recreated) =>
        {
            Assert.Equal(original.U, recreated.U);
            Assert.Equal(original.V, recreated.V);
            Assert.Equal(original.Vertexes.Length, recreated.Vertexes.Length);
            
            for (int i = 0; i < original.Vertexes.Length; i++)
            {
                AssertVector3Equal(original.Vertexes[i], recreated.Vertexes[i]);
            }
            
            Assert.Equal(original.DensityU, recreated.DensityU);
            Assert.Equal(original.DensityV, recreated.DensityV);
            Assert.Equal(original.SmoothType, recreated.SmoothType);
            Assert.Equal(original.IsClosedInU, recreated.IsClosedInU);
            Assert.Equal(original.IsClosedInV, recreated.IsClosedInV);
        });
    }

    [Fact]
    public void PolygonMesh_SimpleGrid_ShouldPreserveSimpleGrid()
    {
        // Arrange
        short u = 3;
        short v = 2;
        var vertices = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(5, 0, 0),
            new Vector3(10, 0, 0),
            new Vector3(0, 5, 0),
            new Vector3(5, 5, 1),
            new Vector3(10, 5, 0)
        };
        
        var originalPolygonMesh = new PolygonMesh(u, v, vertices);

        // Act & Assert
        PerformRoundTripTest(originalPolygonMesh, (original, recreated) =>
        {
            Assert.Equal(original.U, recreated.U);
            Assert.Equal(original.V, recreated.V);
            Assert.Equal(original.Vertexes.Length, recreated.Vertexes.Length);
            
            for (int i = 0; i < original.Vertexes.Length; i++)
            {
                AssertVector3Equal(original.Vertexes[i], recreated.Vertexes[i]);
            }
        });
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}