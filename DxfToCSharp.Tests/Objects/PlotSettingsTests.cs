using netDxf;
using netDxf.Entities;
using netDxf.Objects;
using DxfToCSharp.Tests.Infrastructure;
using DxfToCSharp.Core;
using Xunit;
using System;
using System.IO;
using System.Linq;

namespace DxfToCSharp.Tests.Objects;

public class PlotSettingsTests : RoundTripTestBase, IDisposable
{
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
        Assert.Contains("PlotSettings objects (stored in dictionaries - not directly accessible)", code);
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
        Assert.DoesNotContain("PlotSettings objects (stored in dictionaries - not directly accessible)", code);
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