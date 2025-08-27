using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;

namespace DxfToCSharp.Tests.Entities;

public class Face3dEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Face3d_BasicRoundTrip_ShouldPreserveVertices()
    {
        // Arrange
        var originalFace3d = new Face3D(
            new Vector3(0, 0, 0),
            new Vector3(100, 0, 0),
            new Vector3(100, 100, 0),
            new Vector3(0, 100, 0));

        // Act & Assert
        PerformRoundTripTest(originalFace3d, (original, recreated) =>
        {
            AssertVector3Equal(original.FirstVertex, recreated.FirstVertex);
            AssertVector3Equal(original.SecondVertex, recreated.SecondVertex);
            AssertVector3Equal(original.ThirdVertex, recreated.ThirdVertex);
            AssertVector3Equal(original.FourthVertex, recreated.FourthVertex);
        });
    }

    [Fact]
    public void Face3d_With3DCoordinates_ShouldPreserveZValues()
    {
        // Arrange
        var originalFace3d = new Face3D(
            new Vector3(0, 0, 10),
            new Vector3(50, 0, 15),
            new Vector3(50, 50, 20),
            new Vector3(0, 50, 25));

        // Act & Assert
        PerformRoundTripTest(originalFace3d, (original, recreated) =>
        {
            AssertVector3Equal(original.FirstVertex, recreated.FirstVertex);
            AssertVector3Equal(original.SecondVertex, recreated.SecondVertex);
            AssertVector3Equal(original.ThirdVertex, recreated.ThirdVertex);
            AssertVector3Equal(original.FourthVertex, recreated.FourthVertex);
        });
    }

    [Fact]
    public void Face3d_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("Face3dLayer")
        {
            Color = new AciColor(6), // Magenta
            Lineweight = Lineweight.W20
        };

        var originalFace3d = new Face3D(
            new Vector3(0, 0, 0),
            new Vector3(25, 0, 0),
            new Vector3(25, 25, 0),
            new Vector3(0, 25, 0))
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalFace3d, (original, recreated) =>
        {
            AssertVector3Equal(original.FirstVertex, recreated.FirstVertex);
            AssertVector3Equal(original.SecondVertex, recreated.SecondVertex);
            AssertVector3Equal(original.ThirdVertex, recreated.ThirdVertex);
            AssertVector3Equal(original.FourthVertex, recreated.FourthVertex);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void Face3d_WithCustomColor_ShouldPreserveColor()
    {
        // Arrange
        var originalFace3d = new Face3D(
            new Vector3(0, 0, 0),
            new Vector3(30, 0, 0),
            new Vector3(30, 30, 0),
            new Vector3(0, 30, 0))
        {
            Color = new AciColor(1) // Red
        };

        // Act & Assert
        PerformRoundTripTest(originalFace3d, (original, recreated) =>
        {
            AssertVector3Equal(original.FirstVertex, recreated.FirstVertex);
            AssertVector3Equal(original.SecondVertex, recreated.SecondVertex);
            AssertVector3Equal(original.ThirdVertex, recreated.ThirdVertex);
            AssertVector3Equal(original.FourthVertex, recreated.FourthVertex);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
        });
    }

    [Fact]
    public void Face3d_Triangle_ShouldPreserveGeometry()
    {
        // Arrange - Triangle (fourth vertex same as third)
        var originalFace3d = new Face3D(
            new Vector3(0, 0, 0),
            new Vector3(50, 0, 0),
            new Vector3(25, 43.3, 0),
            new Vector3(25, 43.3, 0));

        // Act & Assert
        PerformRoundTripTest(originalFace3d, (original, recreated) =>
        {
            AssertVector3Equal(original.FirstVertex, recreated.FirstVertex);
            AssertVector3Equal(original.SecondVertex, recreated.SecondVertex);
            AssertVector3Equal(original.ThirdVertex, recreated.ThirdVertex);
            AssertVector3Equal(original.FourthVertex, recreated.FourthVertex);
        });
    }

    [Fact]
    public void Face3d_WithEdgeFlags_ShouldPreserveEdgeFlags()
    {
        // Arrange
        var originalFace3d = new Face3D(
            new Vector3(0, 0, 0),
            new Vector3(40, 0, 0),
            new Vector3(40, 40, 0),
            new Vector3(0, 40, 0))
        {
            EdgeFlags = Face3DEdgeFlags.First | Face3DEdgeFlags.Third
        };

        // Act & Assert
        PerformRoundTripTest(originalFace3d, (original, recreated) =>
        {
            AssertVector3Equal(original.FirstVertex, recreated.FirstVertex);
            AssertVector3Equal(original.SecondVertex, recreated.SecondVertex);
            AssertVector3Equal(original.ThirdVertex, recreated.ThirdVertex);
            AssertVector3Equal(original.FourthVertex, recreated.FourthVertex);
            Assert.Equal(original.EdgeFlags, recreated.EdgeFlags);
        });
    }
}
