using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;
using netDxf.Objects;

namespace DxfToCSharp.Tests.Entities;

public class UnderlayEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Underlay_BasicRoundTrip_ShouldPreserveUnderlayProperties()
    {
        // Arrange
        var underlayDefinition = new UnderlayDgnDefinition("test.dgn");
        var originalUnderlay = new Underlay(underlayDefinition,
            new Vector3(10, 20, 0),
            1.5);
        originalUnderlay.Rotation = 45.0;
        originalUnderlay.Contrast = 75;
        originalUnderlay.Fade = 25;
        originalUnderlay.DisplayOptions = UnderlayDisplayFlags.ShowUnderlay;

        // Act & Assert
        PerformRoundTripTest(originalUnderlay, (original, recreated) =>
        {
            Assert.Equal(original.Definition.Name, recreated.Definition.Name);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertVector2Equal(original.Scale, recreated.Scale);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
            Assert.Equal(original.Contrast, recreated.Contrast);
            Assert.Equal(original.Fade, recreated.Fade);
            Assert.Equal(original.DisplayOptions, recreated.DisplayOptions);
        });
    }

    [Fact]
    public void Underlay_PdfType_ShouldPreservePdfUnderlay()
    {
        // Arrange
        var underlayDefinition = new UnderlayPdfDefinition("document.pdf");
        var originalUnderlay = new Underlay(underlayDefinition,
            new Vector3(0, 0, 0),
            2.0);
        originalUnderlay.Rotation = 90.0;
        originalUnderlay.Contrast = 50;
        originalUnderlay.Fade = 10;

        // Act & Assert
        PerformRoundTripTest(originalUnderlay, (original, recreated) =>
        {
            Assert.Equal(original.Definition.Name, recreated.Definition.Name);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertVector2Equal(original.Scale, recreated.Scale);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
            Assert.Equal(original.Contrast, recreated.Contrast);
            Assert.Equal(original.Fade, recreated.Fade);
        });
    }

    [Fact]
    public void Underlay_DwfType_ShouldPreserveDwfUnderlay()
    {
        // Arrange
        var underlayDefinition = new UnderlayDwfDefinition("drawing.dwf");
        var originalUnderlay = new Underlay(underlayDefinition,
            new Vector3(-5, -10, 2),
            0.5);
        originalUnderlay.Rotation = 180.0;
        originalUnderlay.Contrast = 100;
        originalUnderlay.Fade = 0;

        // Act & Assert
        PerformRoundTripTest(originalUnderlay, (original, recreated) =>
        {
            Assert.Equal(original.Definition.Name, recreated.Definition.Name);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertVector2Equal(original.Scale, recreated.Scale);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
            Assert.Equal(original.Contrast, recreated.Contrast);
            Assert.Equal(original.Fade, recreated.Fade);
        });
    }

    public override void Dispose()
    {
        // Cleanup if needed
        base.Dispose();
    }
}
