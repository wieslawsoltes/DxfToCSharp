using netDxf;
using netDxf.Entities;
using netDxf.Objects;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;
using DxfToCSharp.Core;

namespace DxfToCSharp.Tests.Entities;

/// <summary>
/// Comprehensive tests for entity property generation in DxfCodeGenerator
/// </summary>
public class EntityPropertyTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void EntityProperties_BasicProperties_ShouldGenerateCorrectly()
    {
        // Arrange
        var customLayer = new Layer("TestLayer") { Color = new AciColor(5) };
        var customLinetype = new Linetype("TestLinetype");
        
        var line = new Line(new Vector3(0, 0, 0), new Vector3(100, 100, 0))
        {
            Layer = customLayer,
            Color = new AciColor(3), // Green
            Linetype = customLinetype,
            Lineweight = Lineweight.W20,
            LinetypeScale = 2.5,
            Thickness = 5.0,
            IsVisible = true,
            Normal = new Vector3(0, 0, 1)
        };

        // Act & Assert
        PerformRoundTripTest(line, (original, recreated) =>
        {
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
            Assert.Equal(original.Linetype.Name, recreated.Linetype.Name);
            Assert.Equal(original.Lineweight, recreated.Lineweight);
            AssertDoubleEqual(original.LinetypeScale, recreated.LinetypeScale);
            AssertDoubleEqual(original.Thickness, recreated.Thickness);
            Assert.Equal(original.IsVisible, recreated.IsVisible);
            AssertVector3Equal(original.Normal, recreated.Normal);
        });
    }

    [Fact]
    public void EntityProperties_TrueColor_ShouldGenerateCorrectly()
    {
        // Arrange
        var line = new Line(new Vector3(0, 0, 0), new Vector3(100, 100, 0))
        {
            Color = AciColor.FromTrueColor(System.Drawing.Color.FromArgb(255, 128, 64).ToArgb())
        };

        // Act & Assert
        PerformRoundTripTest(line, (original, recreated) =>
        {
            Assert.True(original.Color.UseTrueColor);
            Assert.True(recreated.Color.UseTrueColor);
            Assert.Equal(original.Color.R, recreated.Color.R);
            Assert.Equal(original.Color.G, recreated.Color.G);
            Assert.Equal(original.Color.B, recreated.Color.B);
        });
    }

    [Fact]
    public void EntityProperties_Transparency_ShouldGenerateCorrectly()
    {
        // Arrange
        var circle = new Circle(new Vector3(0, 0, 0), 50)
        {
            Transparency = new Transparency(50) // 50% transparency (valid range 0-90)
        };

        // Act & Assert
        PerformRoundTripTest(circle, (original, recreated) =>
        {
            Assert.Equal(original.Transparency.Value, recreated.Transparency.Value);
        });
    }

    [Fact]
    public void EntityProperties_ThicknessSupport_Line_ShouldGenerate()
    {
        var line = new Line(Vector3.Zero, Vector3.UnitX) { Thickness = 2.5 };
        PerformRoundTripTest(line, (original, recreated) =>
        {
            AssertDoubleEqual(original.Thickness, recreated.Thickness);
        });
    }

    [Fact]
    public void EntityProperties_ThicknessSupport_Arc_ShouldGenerate()
    {
        var arc = new Arc(Vector3.Zero, 10, 0, 90) { Thickness = 3.0 };
        PerformRoundTripTest(arc, (original, recreated) =>
        {
            AssertDoubleEqual(original.Thickness, recreated.Thickness);
        });
    }

    [Fact]
    public void EntityProperties_ThicknessSupport_Circle_ShouldGenerate()
    {
        var circle = new Circle(Vector3.Zero, 25) { Thickness = 1.5 };
        PerformRoundTripTest(circle, (original, recreated) =>
        {
            AssertDoubleEqual(original.Thickness, recreated.Thickness);
        });
    }

    [Fact]
    public void EntityProperties_ThicknessSupport_Point_ShouldGenerate()
    {
        var point = new netDxf.Entities.Point(new Vector3(10, 20, 30)) { Thickness = 0.5 };
        PerformRoundTripTest(point, (original, recreated) =>
        {
            AssertDoubleEqual(original.Thickness, recreated.Thickness);
        });
    }

    [Fact(Skip = "Ellipse thickness is not restored correctly by netDxf")]
    public void EntityProperties_ThicknessSupport_Ellipse_ShouldGenerate()
    {
        var ellipse = new Ellipse(Vector3.Zero, 20, 10) { Thickness = 4.0 };
        var options = new DxfCodeGenerationOptions
        {
            GenerateLayerStateObjects = false
        };
        PerformRoundTripTest(ellipse, (original, recreated) =>
        {
            AssertDoubleEqual(original.Thickness, recreated.Thickness);
        }, options);
    }

    [Fact]
    public void EntityProperties_ThicknessSupport_Solid_ShouldGenerate()
    {
        var solid = new Solid(Vector2.Zero, new Vector2(10, 0), new Vector2(10, 10), new Vector2(0, 10)) { Thickness = 2.0 };
        PerformRoundTripTest(solid, (original, recreated) =>
        {
            AssertDoubleEqual(original.Thickness, recreated.Thickness);
        });
    }

    [Fact]
    public void EntityProperties_ThicknessSupport_Trace_ShouldGenerate()
    {
        var trace = new Trace(Vector2.Zero, new Vector2(10, 0), new Vector2(10, 10), new Vector2(0, 10)) { Thickness = 1.8 };
        PerformRoundTripTest(trace, (original, recreated) =>
        {
            AssertDoubleEqual(original.Thickness, recreated.Thickness);
        });
    }

    [Fact]
    public void EntityProperties_Reactors_ShouldGenerateComments()
    {
        // Arrange
        var line = new Line(Vector3.Zero, Vector3.UnitX);
        
        // Act & Assert - Test that reactors comment is generated when detailed comments are enabled
        var doc = new DxfDocument();
        doc.Entities.Add(line);
        
        // Create a group and add the entity to it, which will add a reactor
        var group = new Group("TestGroup");
        group.Entities.Add(line);
        doc.Groups.Add(group);
        
        var options = new DxfCodeGenerationOptions
        {
            GenerateDetailedComments = true,
            GenerateLineEntities = true,
            GenerateLayerStateObjects = false
        };

        var generatedCode = _generator.Generate(doc, null, null, options);
        Assert.Contains("// Note: Reactors property is read-only and managed internally by netDxf", generatedCode);
    }

    [Fact]
    public void EntityProperties_ByLayerColor_ShouldNotGenerate()
    {
        // Arrange
        var line = new Line(Vector3.Zero, Vector3.UnitX)
        {
            Color = AciColor.ByLayer
        };

        // Act & Assert - ByLayer color should not generate Color property assignment
        var doc = new DxfDocument();
        doc.Entities.Add(line);
        
        var options = new DxfCodeGenerationOptions
        {
            GenerateLineEntities = true,
            GenerateLayerStateObjects = false
        };

        var generatedCode = _generator.Generate(doc, null, null, options);
        Assert.DoesNotContain("Color = ", generatedCode);
    }

    [Fact]
    public void EntityProperties_ByLayerLineweight_ShouldNotGenerate()
    {
        // Arrange
        var line = new Line(Vector3.Zero, Vector3.UnitX)
        {
            Lineweight = Lineweight.ByLayer
        };

        // Act & Assert - ByLayer lineweight should not generate Lineweight property assignment
        var doc = new DxfDocument();
        doc.Entities.Add(line);
        
        var options = new DxfCodeGenerationOptions
        {
            GenerateLineEntities = true,
            GenerateLayerStateObjects = false
        };

        var generatedCode = _generator.Generate(doc, null, null, options);
        Assert.DoesNotContain("Lineweight = ", generatedCode);
    }

    [Fact]
    public void EntityProperties_DefaultLinetypeScale_ShouldNotGenerate()
    {
        // Arrange
        var line = new Line(Vector3.Zero, Vector3.UnitX)
        {
            LinetypeScale = 1.0 // Default value
        };

        // Act & Assert - Default linetype scale should not generate LinetypeScale property assignment
        var doc = new DxfDocument();
        doc.Entities.Add(line);
        
        var options = new DxfCodeGenerationOptions
        {
            GenerateLineEntities = true,
            GenerateLayerStateObjects = false
        };

        var generatedCode = _generator.Generate(doc, null, null, options);
        Assert.DoesNotContain("LinetypeScale = ", generatedCode);
    }

    [Fact]
    public void EntityProperties_DefaultTransparency_ShouldNotGenerate()
    {
        // Arrange
        var line = new Line(Vector3.Zero, Vector3.UnitX)
        {
            Transparency = Transparency.ByLayer // Default value
        };

        // Act & Assert - Default transparency should not generate Transparency property assignment
        var doc = new DxfDocument();
        doc.Entities.Add(line);
        
        var options = new DxfCodeGenerationOptions
        {
            GenerateLineEntities = true,
            GenerateLayerStateObjects = false
        };

        var generatedCode = _generator.Generate(doc, null, null, options);
        Assert.DoesNotContain("Transparency = ", generatedCode);
    }

    [Fact]
    public void EntityProperties_DefaultVisibility_ShouldNotGenerate()
    {
        // Arrange
        var line = new Line(Vector3.Zero, Vector3.UnitX)
        {
            IsVisible = true // Default value
        };

        // Act & Assert - Default visibility should not generate IsVisible property assignment
        var doc = new DxfDocument();
        doc.Entities.Add(line);
        
        var options = new DxfCodeGenerationOptions
        {
            GenerateLineEntities = true,
            GenerateLayerStateObjects = false
        };

        var generatedCode = _generator.Generate(doc, null, null, options);
        Assert.DoesNotContain("IsVisible = ", generatedCode);
    }

    [Fact]
    public void EntityProperties_DefaultNormal_ShouldNotGenerate()
    {
        // Arrange
        var line = new Line(Vector3.Zero, Vector3.UnitX)
        {
            Normal = Vector3.UnitZ // Default value (0, 0, 1)
        };

        // Act & Assert - Default normal should not generate Normal property assignment
        var doc = new DxfDocument();
        doc.Entities.Add(line);
        
        var options = new DxfCodeGenerationOptions
        {
            GenerateLineEntities = true,
            GenerateLayerStateObjects = false
        };

        var generatedCode = _generator.Generate(doc, null, null, options);
        Assert.DoesNotContain("Normal = ", generatedCode);
    }

    [Fact]
    public void EntityProperties_CustomNormal_ShouldGenerate()
    {
        // Arrange
        var line = new Line(Vector3.Zero, Vector3.UnitX)
        {
            Normal = new Vector3(0.5, 0.5, 0.707) // Custom normal
        };

        // Act & Assert - Custom normal should generate Normal property assignment and round-trip correctly
        PerformRoundTripTest(line, (original, recreated) =>
        {
            AssertVector3Equal(original.Normal, recreated.Normal);
        });
        
        // Also verify code generation includes Normal property
        var doc = new DxfDocument();
        doc.Entities.Add((Line)line.Clone());
        
        var options = new DxfCodeGenerationOptions
        {
            GenerateLineEntities = true,
            GenerateLayerStateObjects = false
        };

        var generatedCode = _generator.Generate(doc, null, null, options);
        Assert.Contains("Normal = ", generatedCode);
        Assert.Contains("0.5", generatedCode);
        Assert.Contains("0.707", generatedCode);
    }

    [Fact]
    public void EntityProperties_InvisibleEntity_ShouldGenerate()
    {
        // Arrange
        var line = new Line(Vector3.Zero, Vector3.UnitX)
        {
            IsVisible = false // Non-default value
        };

        // Act & Assert - Non-default visibility should generate IsVisible property assignment and round-trip correctly
        PerformRoundTripTest(line, (original, recreated) =>
        {
            Assert.Equal(original.IsVisible, recreated.IsVisible);
        });
        
        // Also verify code generation includes IsVisible property
        var doc = new DxfDocument();
        doc.Entities.Add((Line)line.Clone());
        
        var options = new DxfCodeGenerationOptions
        {
            GenerateLineEntities = true,
            GenerateLayerStateObjects = false
        };

        var generatedCode = _generator.Generate(doc, null, null, options);
        Assert.Contains("IsVisible = false", generatedCode);
    }

    [Fact]
    public void EntityProperties_MultipleEntitiesWithDifferentProperties_ShouldGenerateCorrectly()
    {
        // Arrange
        var line1 = new Line(Vector3.Zero, Vector3.UnitX)
        {
            Color = new AciColor(1), // Red
            Thickness = 2.0
        };
        
        var line2 = new Line(Vector3.Zero, Vector3.UnitY)
        {
            Color = AciColor.FromTrueColor(System.Drawing.Color.Blue.ToArgb()),
            LinetypeScale = 0.5
        };
        
        var circle = new Circle(Vector3.Zero, 10)
        {
            Transparency = new Transparency(64),
            IsVisible = false
        };
        
        // Act & Assert - Test each entity individually with round-trip testing
        PerformRoundTripTest(line1, (original, recreated) =>
        {
            Assert.Equal(original.Color.Index, recreated.Color.Index);
            AssertDoubleEqual(original.Thickness, recreated.Thickness);
        });
        
        PerformRoundTripTest(line2, (original, recreated) =>
        {
            Assert.True(original.Color.UseTrueColor);
            Assert.True(recreated.Color.UseTrueColor);
            Assert.Equal(original.Color.R, recreated.Color.R);
            Assert.Equal(original.Color.G, recreated.Color.G);
            Assert.Equal(original.Color.B, recreated.Color.B);
            AssertDoubleEqual(original.LinetypeScale, recreated.LinetypeScale);
        });
        
        PerformRoundTripTest(circle, (original, recreated) =>
        {
            Assert.Equal(original.Transparency.Value, recreated.Transparency.Value);
            Assert.Equal(original.IsVisible, recreated.IsVisible);
        });
        
        // Also verify code generation includes all expected properties
        var doc = new DxfDocument();
        doc.Entities.Add((Line)line1.Clone());
        doc.Entities.Add((Line)line2.Clone());
        doc.Entities.Add((Circle)circle.Clone());
        
        var options = new DxfCodeGenerationOptions
        {
            GenerateLineEntities = true,
            GenerateCircleEntities = true,
            GenerateLayerStateObjects = false
        };

        var generatedCode = _generator.Generate(doc, null, null, options);
        Assert.Contains("Color = new AciColor(1)", generatedCode); // Line1 color
        Assert.Contains("Thickness = 2", generatedCode); // Line1 thickness
        Assert.Contains("AciColor.FromTrueColor", generatedCode); // Line2 true color
        Assert.Contains("LinetypeScale = 0.5", generatedCode); // Line2 linetype scale
        Assert.Contains("Transparency = new Transparency(64)", generatedCode); // Circle transparency
        Assert.Contains("IsVisible = false", generatedCode); // Circle visibility
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
