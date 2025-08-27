using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;

namespace DxfToCSharp.Tests.Entities;

public class TextEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Text_BasicRoundTrip_ShouldPreserveTextProperties()
    {
        // Arrange
        var originalText = new Text(
            "Hello World",
            new Vector3(10, 20, 0),
            5.0);

        // Act & Assert
        PerformRoundTripTest(originalText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
        });
    }

    [Fact]
    public void Text_WithRotation_ShouldPreserveRotation()
    {
        // Arrange
        var originalText = new Text(
            "Rotated Text",
            new Vector3(0, 0, 0),
            10.0)
        {
            Rotation = 45.0 * Math.PI / 180.0 // 45 degrees in radians
        };

        // Act & Assert
        PerformRoundTripTest(originalText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
        });
    }

    [Fact]
    public void Text_With3DPosition_ShouldPreserveZValue()
    {
        // Arrange
        var originalText = new Text(
            "3D Text",
            new Vector3(10, 20, 15.5),
            8.0);

        // Act & Assert
        PerformRoundTripTest(originalText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
        });
    }

    [Fact]
    public void Text_WithSpecialCharacters_ShouldPreserveSpecialCharacters()
    {
        // Arrange
        var originalText = new Text(
            "Special: äöü ñ © ® ™ § ¶ † ‡ • … ‰ ′ ″",
            new Vector3(0, 0, 0),
            12.0);

        // Act & Assert
        PerformRoundTripTest(originalText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
        });
    }

    [Fact]
    public void Text_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("TextLayer")
        {
            Color = new AciColor(3), // Green
            Lineweight = Lineweight.Default
        };

        var originalText = new Text(
            "Layer Text",
            new Vector3(50, 50, 0),
            6.0)
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void Text_WithEmptyString_ShouldPreserveEmptyString()
    {
        // Arrange
        var originalText = new Text(
            "",
            new Vector3(0, 0, 0),
            5.0);

        // Act & Assert
        PerformRoundTripTest(originalText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
        });
    }



    [Fact]
    public void Text_WithVerySmallHeight_ShouldPreserveSmallHeight()
    {
        // Arrange
        var originalText = new Text(
            "Tiny Text",
            new Vector3(0, 0, 0),
            0.001);

        // Act & Assert
        PerformRoundTripTest(originalText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height, 1e-15);
        });
    }

    [Fact]
    public void Text_WithVeryLargeHeight_ShouldPreserveLargeHeight()
    {
        // Arrange
        var originalText = new Text(
            "Large Text",
            new Vector3(0, 0, 0),
            1000.0);

        // Act & Assert
        PerformRoundTripTest(originalText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
        });
    }

    [Fact]
    public void Text_WithNegativePosition_ShouldPreserveNegativePosition()
    {
        // Arrange
        var originalText = new Text(
            "Negative Position",
            new Vector3(-50.5, -30.7, -10.2),
            8.0);

        // Act & Assert
        PerformRoundTripTest(originalText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
        });
    }

    [Fact]
    public void Text_WithCustomColor_ShouldPreserveColor()
    {
        // Arrange
        var originalText = new Text(
            "Colored Text",
            new Vector3(0, 0, 0),
            12.0)
        {
            Color = new AciColor(6) // Magenta
        };

        // Act & Assert
        PerformRoundTripTest(originalText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
        });
    }



    [Fact]
    public void Text_WithPreciseRotation_ShouldPreserveRotationPrecision()
    {
        // Arrange
        var originalText = new Text(
            "Precise Rotation",
            new Vector3(0, 0, 0),
            10.0)
        {
            Rotation = 1.23456789012345 // Precise rotation in radians
        };

        // Act & Assert
        PerformRoundTripTest(originalText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
            AssertDoubleEqual(original.Rotation, recreated.Rotation, 1e-12);
        });
    }

    [Fact]
    public void Text_WithTransparency_ShouldPreserveTransparency()
    {
        // Arrange
        var originalText = new Text(
            "Transparent Text",
            new Vector3(10, 20, 0),
            5.0)
        {
            Transparency = new Transparency(63) // 70% transparency (0-90 scale)
        };

        // Act & Assert
        PerformRoundTripTest(originalText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
            AssertDoubleEqual(original.Transparency.Value, recreated.Transparency.Value);
        });
    }

    [Fact]
    public void Text_WithIsVisibleFalse_ShouldPreserveVisibility()
    {
        // Arrange
        var originalText = new Text(
            "Hidden Text",
            new Vector3(10, 20, 0),
            5.0)
        {
            IsVisible = false
        };

        // Act & Assert
        PerformRoundTripTest(originalText, (original, recreated) =>
        {
            Assert.Equal(original.Value, recreated.Value);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.Height, recreated.Height);
            Assert.Equal(original.IsVisible, recreated.IsVisible);
        });
    }
}
