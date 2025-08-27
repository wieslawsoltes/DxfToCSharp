using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;

namespace DxfToCSharp.Tests.Entities;

public class Polyline3DEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Polyline3D_BasicOpenPolyline_ShouldPreserveVertices()
    {
        // Arrange
        var vertices = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(10, 0, 5),
            new Vector3(10, 10, 10),
            new Vector3(0, 10, 15)
        };
        var originalPolyline = new Polyline3D(vertices, false);

        // Act & Assert
        PerformRoundTripTest(originalPolyline, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.IsClosed, recreated.IsClosed);

            for (var i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(original.Vertexes[i], recreated.Vertexes[i]);
            }
        });
    }

    [Fact]
    public void Polyline3D_ClosedPolyline_ShouldPreserveClosedState()
    {
        // Arrange
        var vertices = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(10, 0, 0),
            new Vector3(10, 10, 0),
            new Vector3(0, 10, 0)
        };
        var originalPolyline = new Polyline3D(vertices, true);

        // Act & Assert
        PerformRoundTripTest(originalPolyline, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.IsClosed, recreated.IsClosed);
            Assert.True(recreated.IsClosed);

            for (var i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(original.Vertexes[i], recreated.Vertexes[i]);
            }
        });
    }

    [Fact]
    public void Polyline3D_With3DVertices_ShouldPreserveZValues()
    {
        // Arrange
        var vertices = new List<Vector3>
        {
            new Vector3(0, 0, 5),
            new Vector3(10, 5, 10),
            new Vector3(20, 10, 15),
            new Vector3(30, 15, 20)
        };
        var originalPolyline = new Polyline3D(vertices, false);

        // Act & Assert
        PerformRoundTripTest(originalPolyline, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.IsClosed, recreated.IsClosed);

            for (var i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(original.Vertexes[i], recreated.Vertexes[i]);
            }
        });
    }

    [Fact]
    public void Polyline3D_SingleVertex_ShouldHandleGracefully()
    {
        // Arrange
        var vertices = new List<Vector3>
        {
            new Vector3(5, 10, 15)
        };
        var originalPolyline = new Polyline3D(vertices, false);

        // Act
        var originalDoc = new DxfDocument();
        originalDoc.Entities.Add(originalPolyline);

        var tempPath = Path.Join(Path.GetTempPath(), "test_single_vertex.dxf");
        originalDoc.Save(tempPath);

        var loadedDoc = DxfDocument.Load(tempPath);

        // Assert
        // Note: DXF format limitation - single vertex polylines are not written to DXF file
        Assert.Empty(loadedDoc.Entities.All);

        // Clean up
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    [Fact]
    public void Polyline3D_EmptyVertexList_ShouldHandleGracefully()
    {
        // Arrange
        var vertices = new List<Vector3>();
        var originalPolyline = new Polyline3D(vertices, false);

        // Act
        var originalDoc = new DxfDocument();
        originalDoc.Entities.Add(originalPolyline);

        var tempPath = Path.Join(Path.GetTempPath(), "test_empty_vertex.dxf");
        originalDoc.Save(tempPath);

        var loadedDoc = DxfDocument.Load(tempPath);

        // Assert
        // Note: DXF format limitation - empty polylines are not written to DXF file
        Assert.Empty(loadedDoc.Entities.All);

        // Clean up
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    [Fact]
    public void Polyline3D_ComplexPath_ShouldPreserveAllVertices()
    {
        // Arrange
        var vertices = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(10, 0, 2),
            new Vector3(15, 5, 4),
            new Vector3(20, 10, 6),
            new Vector3(15, 15, 8),
            new Vector3(10, 20, 10),
            new Vector3(5, 15, 12),
            new Vector3(0, 10, 14),
            new Vector3(-5, 5, 16)
        };
        var originalPolyline = new Polyline3D(vertices, false);

        // Act & Assert
        PerformRoundTripTest(originalPolyline, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.IsClosed, recreated.IsClosed);

            for (var i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(original.Vertexes[i], recreated.Vertexes[i]);
            }
        });
    }

    [Fact]
    public void Polyline3D_NegativeCoordinates_ShouldPreserveValues()
    {
        // Arrange
        var vertices = new List<Vector3>
        {
            new Vector3(-10, -5, -2),
            new Vector3(-5, -10, -4),
            new Vector3(0, -15, -6),
            new Vector3(5, -10, -8)
        };
        var originalPolyline = new Polyline3D(vertices, true);

        // Act & Assert
        PerformRoundTripTest(originalPolyline, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.IsClosed, recreated.IsClosed);

            for (var i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(original.Vertexes[i], recreated.Vertexes[i]);
            }
        });
    }

    public override void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
        catch (IOException)
        {
            // Ignore cleanup errors - directory may be in use
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore cleanup errors - insufficient permissions
        }

        base.Dispose();
    }
}
