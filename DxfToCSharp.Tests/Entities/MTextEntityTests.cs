using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;

namespace DxfToCSharp.Tests.Entities;

public class MTextEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void MText_BasicRoundTrip_ShouldPreserveMTextProperties()
    {
        // Arrange
        var originalMText = new MText(
            "Multi-line\\PText Content",
            new Vector3(25, 25, 0),
            8.0);

        // Act & Assert
        PerformRoundTripTest(originalMText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
        });
    }

    [Fact]
    public void MText_WithRectangleWidth_ShouldPreserveRectangleWidth()
    {
        // Arrange
        var originalMText = new MText(
            "This is a long text that should wrap within the specified rectangle width.",
            new Vector3(0, 0, 0),
            5.0)
        {
            RectangleWidth = 100.0
        };

        // Act & Assert
        PerformRoundTripTest(originalMText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
            AssertDoubleEqual(original.RectangleWidth, recreated.RectangleWidth);
        });
    }

    [Fact]
    public void MText_WithFormattingCodes_ShouldPreserveFormattingCodes()
    {
        // Arrange
        var originalMText = new MText(
            "\\fArial|b1|i1;Bold Italic Text\\fArial|b0|i0;\\PNormal Text",
            new Vector3(10, 10, 0),
            10.0);

        // Act & Assert
        PerformRoundTripTest(originalMText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
        });
    }

    [Fact]
    public void MText_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("MTextLayer")
        {
            Color = new AciColor(4), // Cyan
            Lineweight = Lineweight.Default
        };

        var originalMText = new MText(
            "Multi-line text\\Pwith custom layer",
            new Vector3(30, 40, 0),
            7.0)
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalMText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void MText_WithMultipleLines_ShouldPreserveLineBreaks()
    {
        // Arrange
        var originalMText = new MText(
            "Line 1\\PLine 2\\PLine 3\\PLine 4",
            new Vector3(0, 0, 0),
            6.0);

        // Act & Assert
        PerformRoundTripTest(originalMText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
        });
    }
}
