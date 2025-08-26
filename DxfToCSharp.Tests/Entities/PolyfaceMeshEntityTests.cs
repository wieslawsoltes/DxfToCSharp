using netDxf;
using netDxf.Entities;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class PolyfaceMeshEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void PolyfaceMesh_BasicRoundTrip_ShouldPreservePolyfaceMeshProperties()
    {
        // Arrange
        var vertices = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(10, 0, 0),
            new Vector3(10, 10, 0),
            new Vector3(0, 10, 0),
            new Vector3(5, 5, 5)
        };
        
        var faces = new List<short[]>
        {
            new short[] { 1, 2, 5 },
            new short[] { 2, 3, 5 },
            new short[] { 3, 4, 5 },
            new short[] { 4, 1, 5 },
            new short[] { 1, 2, 3, 4 }
        };
        
        var originalPolyfaceMesh = new PolyfaceMesh(vertices, faces);

        // Act & Assert
        PerformRoundTripTest(originalPolyfaceMesh, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Length, recreated.Vertexes.Length);
            for (var i = 0; i < original.Vertexes.Length; i++)
            {
                AssertVector3Equal(original.Vertexes[i], recreated.Vertexes[i]);
            }
            
            Assert.Equal(original.Faces.Count, recreated.Faces.Count);
            for (var i = 0; i < original.Faces.Count; i++)
            {
                Assert.Equal(original.Faces[i].VertexIndexes.Length, recreated.Faces[i].VertexIndexes.Length);
                for (var j = 0; j < original.Faces[i].VertexIndexes.Length; j++)
                {
                    Assert.Equal(original.Faces[i].VertexIndexes[j], recreated.Faces[i].VertexIndexes[j]);
                }
            }
        });
    }

    [Fact]
    public void PolyfaceMesh_WithPolyfaceMeshFaces_ShouldPreservePolyfaceMeshWithFaces()
    {
        // Arrange
        var vertices = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(10, 0, 0),
            new Vector3(10, 10, 0),
            new Vector3(0, 10, 0)
        };
        
        var faces = new List<PolyfaceMeshFace>
        {
            new PolyfaceMeshFace(new short[] { 1, 2, 3 }),
            new PolyfaceMeshFace(new short[] { 1, 3, 4 })
        };
        
        var originalPolyfaceMesh = new PolyfaceMesh(vertices, faces);

        // Act & Assert
        PerformRoundTripTest(originalPolyfaceMesh, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Length, recreated.Vertexes.Length);
            for (var i = 0; i < original.Vertexes.Length; i++)
            {
                AssertVector3Equal(original.Vertexes[i], recreated.Vertexes[i]);
            }
            
            Assert.Equal(original.Faces.Count, recreated.Faces.Count);
            for (var i = 0; i < original.Faces.Count; i++)
            {
                Assert.Equal(original.Faces[i].VertexIndexes.Length, recreated.Faces[i].VertexIndexes.Length);
                for (var j = 0; j < original.Faces[i].VertexIndexes.Length; j++)
                {
                    Assert.Equal(original.Faces[i].VertexIndexes[j], recreated.Faces[i].VertexIndexes[j]);
                }
            }
        });
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}