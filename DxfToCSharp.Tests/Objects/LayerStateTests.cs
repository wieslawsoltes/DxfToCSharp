using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using netDxf.Objects;
using DxfToCSharp.Tests.Infrastructure;
using DxfToCSharp.Core;
using Xunit;
using System;
using System.IO;
using System.Linq;

namespace DxfToCSharp.Tests.Objects;

public class LayerStateTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void LayerState_GenerationOptions_ShouldBeRespected()
    {
        // Arrange
        var doc = new DxfDocument();
        
        // Create a simple document with layers
        var layer1 = new Layer("TestLayer1");
        var layer2 = new Layer("TestLayer2");
        doc.Layers.Add(layer1);
        doc.Layers.Add(layer2);
        
        // Add some entities to use the layers
        doc.Entities.Add(new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0)) { Layer = layer1 });
        doc.Entities.Add(new Line(new Vector3(5, 5, 0), new Vector3(15, 15, 0)) { Layer = layer2 });
        
        var generator = new DxfCodeGenerator();
        
        // Test with LayerState generation enabled
        var optionsEnabled = new DxfCodeGenerationOptions
        {
            GenerateLayerStateObjects = true,
            GenerateDetailedComments = true
        };
        var codeEnabled = generator.Generate(doc, null, null, optionsEnabled);
        
        // Test with LayerState generation disabled
        var optionsDisabled = new DxfCodeGenerationOptions
        {
            GenerateLayerStateObjects = false,
            GenerateDetailedComments = true
        };
        var codeDisabled = generator.Generate(doc, null, null, optionsDisabled);
        
        // Assert
        Assert.Contains("LayerState objects", codeEnabled);
        Assert.DoesNotContain("LayerState objects", codeDisabled);
    }
    
    [Fact]
    public void LayerState_WithDetailedComments_ShouldIncludeComments()
    {
        // Arrange
        var doc = new DxfDocument();
        var layer = new Layer("TestLayer");
        doc.Layers.Add(layer);
        doc.Entities.Add(new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0)) { Layer = layer });
        
        var generator = new DxfCodeGenerator();
        var options = new DxfCodeGenerationOptions
        {
            GenerateLayerStateObjects = true,
            GenerateDetailedComments = true
        };
        
        // Act
        var code = generator.Generate(doc, null, null, options);
        
        // Assert
        Assert.Contains("LayerState objects (stored in dictionaries - not directly accessible)", code);
    }
    
    [Fact]
    public void LayerState_WithoutDetailedComments_ShouldNotIncludeComments()
    {
        // Arrange
        var doc = new DxfDocument();
        var layer = new Layer("TestLayer");
        doc.Layers.Add(layer);
        doc.Entities.Add(new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0)) { Layer = layer });
        
        var generator = new DxfCodeGenerator();
        var options = new DxfCodeGenerationOptions
        {
            GenerateLayerStateObjects = true,
            GenerateDetailedComments = false
        };
        
        // Act
        var code = generator.Generate(doc, null, null, options);
        
        // Assert
        Assert.DoesNotContain("LayerState objects (stored in dictionaries - not directly accessible)", code);
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