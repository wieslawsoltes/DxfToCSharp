using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;

namespace DxfToCSharp.Tests.Entities;

public class MeshEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Mesh_BasicRoundTrip_ShouldPreserveMeshProperties()
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

        var faces = new List<int[]>
        {
            new int[] { 0, 1, 4 },
            new int[] { 1, 2, 4 },
            new int[] { 2, 3, 4 },
            new int[] { 3, 0, 4 },
            new int[] { 0, 1, 2, 3 }
        };

        var originalMesh = new Mesh(vertices, faces);
        originalMesh.SubdivisionLevel = 2;

        // Act & Assert
        PerformRoundTripTest(originalMesh, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            for (var i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(original.Vertexes[i], recreated.Vertexes[i]);
            }

            Assert.Equal(original.Faces.Count, recreated.Faces.Count);
            for (var i = 0; i < original.Faces.Count; i++)
            {
                Assert.Equal(original.Faces[i].Length, recreated.Faces[i].Length);
                for (var j = 0; j < original.Faces[i].Length; j++)
                {
                    Assert.Equal(original.Faces[i][j], recreated.Faces[i][j]);
                }
            }

            Assert.Equal(original.SubdivisionLevel, recreated.SubdivisionLevel);
        });
    }

    [Fact]
    public void Mesh_WithEdges_ShouldPreserveMeshWithEdges()
    {
        // Arrange
        var vertices = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(10, 0, 0),
            new Vector3(10, 10, 0),
            new Vector3(0, 10, 0)
        };

        var faces = new List<int[]>
        {
            new int[] { 0, 1, 2, 3 }
        };

        var edges = new List<MeshEdge>
        {
            new MeshEdge(0, 1),
            new MeshEdge(1, 2),
            new MeshEdge(2, 3),
            new MeshEdge(3, 0)
        };

        var originalMesh = new Mesh(vertices, faces, edges);

        // Act & Assert
        PerformRoundTripTest(originalMesh, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.Faces.Count, recreated.Faces.Count);
            Assert.Equal(original.Edges.Count, recreated.Edges.Count);

            for (var i = 0; i < original.Edges.Count; i++)
            {
                Assert.Equal(original.Edges[i].StartVertexIndex, recreated.Edges[i].StartVertexIndex);
                Assert.Equal(original.Edges[i].EndVertexIndex, recreated.Edges[i].EndVertexIndex);
            }
        });
    }

    public override void Dispose()
    {
        // Cleanup if needed
        base.Dispose();
    }
}
