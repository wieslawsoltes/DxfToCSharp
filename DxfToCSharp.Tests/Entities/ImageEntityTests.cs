using netDxf;
using netDxf.Entities;
using netDxf.Objects;
using netDxf.Units;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class ImageEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Image_BasicRoundTrip_ShouldPreserveImageProperties()
    {
        // Arrange
        var imageDefinition = new ImageDefinition("test.jpg", 800, 72.0, 600, 72.0, ImageResolutionUnits.Inches);
        var originalImage = new Image(imageDefinition, 
            new Vector3(10, 20, 0), 
            800, 600);
        originalImage.Rotation = 45.0;
        originalImage.Brightness = 75;
        originalImage.Contrast = 80;
        originalImage.Fade = 25;

        // Act & Assert
        PerformRoundTripTest(originalImage, (original, recreated) =>
        {
            Assert.Equal(original.Definition.Name, recreated.Definition.Name);
            Assert.Equal(original.Definition.Width, recreated.Definition.Width);
            Assert.Equal(original.Definition.Height, recreated.Definition.Height);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Width, recreated.Width);
            AssertDoubleEqual(original.Height, recreated.Height);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
            Assert.Equal(original.Brightness, recreated.Brightness);
            Assert.Equal(original.Contrast, recreated.Contrast);
            Assert.Equal(original.Fade, recreated.Fade);
        });
    }

    [Fact]
    public void Image_WithClippingBoundary_ShouldPreserveClipping()
    {
        // Arrange
        var imageDefinition = new ImageDefinition("test.png", 1024, 72.0, 768, 72.0, ImageResolutionUnits.Inches);
        var originalImage = new Image(imageDefinition, 
            new Vector3(0, 0, 0), 
            1024, 768);
        
        var clippingVertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(512, 0),
            new Vector2(512, 384),
            new Vector2(0, 384)
        };
        originalImage.ClippingBoundary = new ClippingBoundary(clippingVertices);

        // Act & Assert
        PerformRoundTripTest(originalImage, (original, recreated) =>
        {
            Assert.Equal(original.Definition.Name, recreated.Definition.Name);
            AssertVector3Equal(original.Position, recreated.Position);
            
            if (original.ClippingBoundary != null && recreated.ClippingBoundary != null)
            {
                Assert.Equal(original.ClippingBoundary.Vertexes.Count, recreated.ClippingBoundary.Vertexes.Count);
                for (var i = 0; i < original.ClippingBoundary.Vertexes.Count; i++)
                {
                    AssertVector2Equal(original.ClippingBoundary.Vertexes[i], recreated.ClippingBoundary.Vertexes[i]);
                }
            }
        });
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}