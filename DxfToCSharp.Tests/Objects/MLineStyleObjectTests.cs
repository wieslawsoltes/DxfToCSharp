using netDxf;
using netDxf.Entities;
using DxfToCSharp.Tests.Infrastructure;
using DxfToCSharp.Core;

namespace DxfToCSharp.Tests.Objects;

public class MLineStyleObjectTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void MLineStyleObject_GenerationOptions_ShouldBeRespected()
    {
        // Arrange
        var doc = new DxfDocument();
        
        // Create a simple document with entities
        doc.Entities.Add(new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0)));
        doc.Entities.Add(new Circle(new Vector3(5, 5, 0), 2.5));
        
        var generator = new DxfCodeGenerator();
        
        // Test with MLineStyle object generation enabled
        var optionsEnabled = new DxfCodeGenerationOptions
        {
            GenerateMLineStyleObjects = true,
            GenerateDetailedComments = true
        };
        var codeEnabled = generator.Generate(doc, null, null, optionsEnabled);
        
        // Test with MLineStyle object generation disabled
        var optionsDisabled = new DxfCodeGenerationOptions
        {
            GenerateMLineStyleObjects = false,
            GenerateDetailedComments = true
        };
        var codeDisabled = generator.Generate(doc, null, null, optionsDisabled);
        
        // Assert
        Assert.Contains("MLineStyle objects", codeEnabled);
        Assert.DoesNotContain("MLineStyle objects", codeDisabled);
    }
    
    [Fact]
    public void MLineStyleObject_WithDetailedComments_ShouldIncludeComments()
    {
        // Arrange
        var doc = new DxfDocument();
        doc.Entities.Add(new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0)));
        
        var generator = new DxfCodeGenerator();
        var options = new DxfCodeGenerationOptions
        {
            GenerateMLineStyleObjects = true,
            GenerateDetailedComments = true
        };
        
        // Act
        var code = generator.Generate(doc, null, null, options);
        
        // Assert
        Assert.Contains("MLineStyle objects (stored in dictionaries - not directly accessible)", code);
    }
    
    [Fact]
    public void MLineStyleObject_WithoutDetailedComments_ShouldNotIncludeComments()
    {
        // Arrange
        var doc = new DxfDocument();
        doc.Entities.Add(new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0)));
        
        var generator = new DxfCodeGenerator();
        var options = new DxfCodeGenerationOptions
        {
            GenerateMLineStyleObjects = true,
            GenerateDetailedComments = false
        };
        
        // Act
        var code = generator.Generate(doc, null, null, options);
        
        // Assert
        Assert.DoesNotContain("MLineStyle objects (stored in dictionaries - not directly accessible)", code);
    }
    
    public void Dispose()
    {
        // Cleanup temp directory
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}