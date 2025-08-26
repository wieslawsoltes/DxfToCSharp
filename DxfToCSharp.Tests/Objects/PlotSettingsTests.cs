using netDxf;
using netDxf.Entities;
using DxfToCSharp.Tests.Infrastructure;
using DxfToCSharp.Core;

namespace DxfToCSharp.Tests.Objects;

public class PlotSettingsTests : RoundTripTestBase, IDisposable
{
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), "DxfToCSharpTests", Guid.NewGuid().ToString());
    [Fact]
    public void PlotSettings_GenerationOptions_ShouldBeRespected()
    {
        // Arrange
        var doc = new DxfDocument();
        
        // Create a simple document with entities
        doc.Entities.Add(new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0)));
        doc.Entities.Add(new Circle(new Vector3(5, 5, 0), 2.5));
        
        var generator = new DxfCodeGenerator();
        
        // Test with PlotSettings generation enabled
        var optionsEnabled = new DxfCodeGenerationOptions
        {
            GeneratePlotSettingsObjects = true,
            GenerateDetailedComments = true
        };
        var codeEnabled = generator.Generate(doc, null, null, optionsEnabled);
        
        // Test with PlotSettings generation disabled
        var optionsDisabled = new DxfCodeGenerationOptions
        {
            GeneratePlotSettingsObjects = false,
            GenerateDetailedComments = true
        };
        var codeDisabled = generator.Generate(doc, null, null, optionsDisabled);
        
        // Assert
        Assert.Contains("PlotSettings objects", codeEnabled);
        Assert.DoesNotContain("PlotSettings objects", codeDisabled);
    }
    
    [Fact]
    public void PlotSettings_WithDetailedComments_ShouldIncludeComments()
    {
        // Arrange
        var doc = new DxfDocument();
        doc.Entities.Add(new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0)));
        
        var generator = new DxfCodeGenerator();
        var options = new DxfCodeGenerationOptions
        {
            GeneratePlotSettingsObjects = true,
            GenerateDetailedComments = true
        };
        
        // Act
        var code = generator.Generate(doc, null, null, options);
        
        // Assert
        Assert.Contains("PlotSettings objects are internal to netDxf and not directly accessible", code);
        Assert.Contains("PlotSettings objects store plot configuration data", code);
    }
    
    [Fact]
    public void PlotSettings_WithoutDetailedComments_ShouldNotIncludeComments()
    {
        // Arrange
        var doc = new DxfDocument();
        doc.Entities.Add(new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0)));
        
        var generator = new DxfCodeGenerator();
        var options = new DxfCodeGenerationOptions
        {
            GeneratePlotSettingsObjects = true,
            GenerateDetailedComments = false
        };
        
        // Act
        var code = generator.Generate(doc, null, null, options);
        
        // Assert
        Assert.DoesNotContain("PlotSettings objects store plot configuration data", code);
    }
    
    [Fact]
    public void PlotSettings_CodeGeneration_ShouldIncludeAllProperties()
    {
        // Arrange
        var doc = new DxfDocument();
        doc.Entities.Add(new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0)));
        
        var options = new DxfCodeGenerationOptions
        {
            GeneratePlotSettingsObjects = true,
            GenerateDetailedComments = true
        };

        // Act
        var generator = new DxfCodeGenerator();
        var generatedCode = generator.Generate(doc, null, null, options);

        // Assert
        Assert.Contains("// PlotSettings objects are internal to netDxf and not directly accessible", generatedCode);
        Assert.Contains("//   PlotConfigurationName: [plotter configuration name]", generatedCode);
        Assert.Contains("//   PaperSize: [paper size name]", generatedCode);
        Assert.Contains("//   PlotArea: [plot area type]", generatedCode);
        // PlotSettings objects are not directly accessible from DxfDocument,
        // so only placeholder comments are generated
    }

    [Fact]
    public void PlotSettings_GenerationDisabled_ShouldNotGenerateCode()
    {
        // Arrange
        var doc = new DxfDocument();
        doc.Entities.Add(new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0)));
        
        var options = new DxfCodeGenerationOptions
        {
            GeneratePlotSettingsObjects = false,
            GenerateDetailedComments = true
        };

        // Act
        var generator = new DxfCodeGenerator();
        var generatedCode = generator.Generate(doc, null, null, options);

        // Assert
        Assert.DoesNotContain("PlotSettings", generatedCode);
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