using DxfToCSharp.Core;
using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;
using netDxf.Objects;

namespace DxfToCSharp.Tests.Tables;

public class MLineStyleTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void MLineStyle_BasicProperties_ShouldRoundTrip()
    {
        // Arrange
        var originalMLineStyle = new MLineStyle("TestMLineStyle")
        {
            Description = "Test multiline style",
            FillColor = AciColor.Red,
            StartAngle = 45.0,
            EndAngle = 135.0
        };

        // Create a document with an MLine that uses this style
        var originalDoc = new DxfDocument();
        originalDoc.MlineStyles.Add(originalMLineStyle);

        var vertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(100, 0),
            new Vector2(100, 100)
        };

        var mline = new MLine(vertices)
        {
            Style = originalMLineStyle
        };
        originalDoc.Entities.Add(mline);

        // Act & Assert
        PerformMLineStyleRoundTripTest(originalDoc, originalMLineStyle, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            Assert.Equal(original.Description, recreated.Description);
            Assert.Equal(original.FillColor.Index, recreated.FillColor.Index);
            Assert.Equal(original.StartAngle, recreated.StartAngle, 6);
            Assert.Equal(original.EndAngle, recreated.EndAngle, 6);
        });
    }

    [Fact]
    public void MLineStyle_WithElements_ShouldRoundTrip()
    {
        // Arrange
        var elements = new List<MLineStyleElement>
        {
            new MLineStyleElement(0.5) { Color = AciColor.Red },
            new MLineStyleElement(-0.5) { Color = AciColor.Blue }
        };

        var originalMLineStyle = new MLineStyle("TestMLineStyleWithElements", elements)
        {
            Description = "Test multiline style with elements"
        };

        // Create a document with an MLine that uses this style
        var originalDoc = new DxfDocument();
        originalDoc.MlineStyles.Add(originalMLineStyle);

        var vertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(100, 0)
        };

        var mline = new MLine(vertices)
        {
            Style = originalMLineStyle
        };
        originalDoc.Entities.Add(mline);

        // Act & Assert
        PerformMLineStyleRoundTripTest(originalDoc, originalMLineStyle, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            Assert.Equal(original.Description, recreated.Description);
            Assert.Equal(original.Elements.Count, recreated.Elements.Count);

            for (var i = 0; i < original.Elements.Count; i++)
            {
                Assert.Equal(original.Elements[i].Offset, recreated.Elements[i].Offset, 6);
                Assert.Equal(original.Elements[i].Color.Index, recreated.Elements[i].Color.Index);
            }
        });
    }

    [Fact]
    public void MLineStyle_SpecialCharacters_ShouldRoundTrip()
    {
        // Arrange
        var originalMLineStyle = new MLineStyle("MLine_Style-123")
        {
            Description = "Test style with special chars: !@#$%^&*()_+-={}[]|\\:;\"'<>?,./ and unicode: αβγδε"
        };

        // Create a document with an MLine that uses this style
        var originalDoc = new DxfDocument();
        originalDoc.MlineStyles.Add(originalMLineStyle);

        var vertices = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(50, 50)
        };

        var mline = new MLine(vertices)
        {
            Style = originalMLineStyle
        };
        originalDoc.Entities.Add(mline);

        // Act & Assert
        PerformMLineStyleRoundTripTest(originalDoc, originalMLineStyle, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            Assert.Equal(original.Description, recreated.Description);
        });
    }

    [Fact]
    public void MLineStyle_MultipleTypes_ShouldRoundTrip()
    {
        // Arrange
        var mlineStyle1 = new MLineStyle("Style1") { Description = "First style" };
        var mlineStyle2 = new MLineStyle("Style2") { Description = "Second style" };
        var mlineStyle3 = new MLineStyle("Style3") { Description = "Third style" };

        var originalMLineStyles = new[] { mlineStyle1, mlineStyle2, mlineStyle3 };

        // Act & Assert
        PerformMultipleObjectsRoundTripTest(originalMLineStyles, (originalObjects, recreatedDoc) =>
        {
            foreach (var originalStyle in originalObjects)
            {
                var recreatedStyle = recreatedDoc.MlineStyles.FirstOrDefault(s => s.Name == originalStyle.Name);
                Assert.NotNull(recreatedStyle);
                Assert.Equal(originalStyle.Name, recreatedStyle.Name);
                Assert.Equal(originalStyle.Description, recreatedStyle.Description);
            }
        });
    }

    private void PerformMLineStyleRoundTripTest(DxfDocument originalDoc, MLineStyle originalMLineStyle, Action<MLineStyle, MLineStyle> validator)
    {
        // Generate code
        var options = new DxfCodeGenerationOptions
        {
            GenerateMLineStyles = true,
            GenerateMLineEntities = true
        };
        var generator = new DxfCodeGenerator();
        var code = generator.Generate(originalDoc, null, null, options);

        // Compile and execute the generated code
        var recreatedDoc = CompileAndExecuteCode(code);

        // Find the recreated MLineStyle
        var recreatedMLineStyle = recreatedDoc.MlineStyles.FirstOrDefault(s => s.Name == originalMLineStyle.Name);
        Assert.NotNull(recreatedMLineStyle);

        // Perform validation
        validator(originalMLineStyle, recreatedMLineStyle);
    }

    private void PerformMultipleObjectsRoundTripTest(MLineStyle[] originalObjects, Action<MLineStyle[], DxfDocument> validator)
    {
        // Create a document with multiple MLineStyles
        var originalDoc = new DxfDocument();
        foreach (var style in originalObjects)
        {
            originalDoc.MlineStyles.Add(style);

            // Create an MLine entity that uses this style
            var vertices = new List<Vector2>
            {
                new Vector2(0, 0),
                new Vector2(100, 0)
            };
            var mline = new MLine(vertices) { Style = style };
            originalDoc.Entities.Add(mline);
        }

        // Generate code
        var options = new DxfCodeGenerationOptions
        {
            GenerateMLineStyles = true,
            GenerateMLineEntities = true
        };
        var generator = new DxfCodeGenerator();
        var code = generator.Generate(originalDoc, null, null, options);

        // Compile and execute the generated code
        var recreatedDoc = CompileAndExecuteCode(code);

        // Perform validation
        validator(originalObjects, recreatedDoc);
    }

    public override void Dispose()
    {
        // Cleanup if needed
        base.Dispose();
    }
}
