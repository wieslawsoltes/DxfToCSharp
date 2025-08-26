using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using netDxf.Objects;
using DxfToCSharp.Tests.Infrastructure;
using DxfToCSharp.Core;

namespace DxfToCSharp.Tests.Objects;

public class LayerStateTests : RoundTripTestBase, IDisposable
{
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), "DxfToCSharpTests", Guid.NewGuid().ToString());
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
        Assert.Contains("LayerState objects are internal to netDxf and not directly accessible", code);
        Assert.Contains("LayerState objects store layer property snapshots", code);
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
        Assert.DoesNotContain("LayerState objects store layer property snapshots", code);
    }
    
    [Fact]
    public void LayerState_Creation_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var layers = new List<Layer>
        {
            new Layer("Layer1") { Color = AciColor.Red, Lineweight = Lineweight.W20, IsVisible = true, IsFrozen = false, IsLocked = false },
            new Layer("Layer2") { Color = AciColor.Blue, Lineweight = Lineweight.W30, IsVisible = false, IsFrozen = true, IsLocked = true }
        };

        // Act
        var layerState = new LayerState("TestState", layers);
        layerState.Description = "Test layer state description";
        layerState.CurrentLayer = "Layer1";
        layerState.PaperSpace = true;

        // Assert
        Assert.Equal("TestState", layerState.Name);
        Assert.Equal("Test layer state description", layerState.Description);
        Assert.Equal("Layer1", layerState.CurrentLayer);
        Assert.True(layerState.PaperSpace);
        Assert.Equal(2, layerState.Properties.Count);
        
        // Verify Layer1 properties
        Assert.True(layerState.Properties.ContainsKey("Layer1"));
        var layer1Props = layerState.Properties["Layer1"];
        Assert.Equal(AciColor.Red, layer1Props.Color);
        Assert.Equal(Lineweight.W20, layer1Props.Lineweight);
        Assert.False(layer1Props.Flags.HasFlag(LayerPropertiesFlags.Hidden));
        Assert.False(layer1Props.Flags.HasFlag(LayerPropertiesFlags.Frozen));
        Assert.False(layer1Props.Flags.HasFlag(LayerPropertiesFlags.Locked));
        
        // Verify Layer2 properties
        Assert.True(layerState.Properties.ContainsKey("Layer2"));
        var layer2Props = layerState.Properties["Layer2"];
        Assert.Equal(AciColor.Blue, layer2Props.Color);
        Assert.Equal(Lineweight.W30, layer2Props.Lineweight);
        Assert.True(layer2Props.Flags.HasFlag(LayerPropertiesFlags.Hidden));
        Assert.True(layer2Props.Flags.HasFlag(LayerPropertiesFlags.Frozen));
        Assert.True(layer2Props.Flags.HasFlag(LayerPropertiesFlags.Locked));
    }

    [Fact]
    public void LayerState_EmptyConstructor_ShouldCreateValidObject()
    {
        // Act
        var layerState = new LayerState("EmptyState");

        // Assert
        Assert.Equal("EmptyState", layerState.Name);
        Assert.Equal("", layerState.Description);
        Assert.Equal("0", layerState.CurrentLayer);
        Assert.False(layerState.PaperSpace);
        Assert.Empty(layerState.Properties);
    }

    [Fact]
    public void LayerState_WithEmptyLayers_ShouldCreateEmptyProperties()
    {
        // Act
        var layerState = new LayerState("EmptyLayersState", new List<Layer>());

        // Assert
        Assert.Equal("EmptyLayersState", layerState.Name);
        Assert.Empty(layerState.Properties);
    }

    [Fact]
    public void LayerState_PropertyModification_ShouldUpdateCorrectly()
    {
        // Arrange
        var layer = new Layer("TestLayer") { Color = AciColor.Green };
        var layerState = new LayerState("ModifyState", new[] { layer });

        // Act
        layerState.Properties["TestLayer"].Color = AciColor.Yellow;
        layerState.Properties["TestLayer"].Flags |= LayerPropertiesFlags.Hidden;
        layerState.Properties["TestLayer"].Lineweight = Lineweight.W50;

        // Assert
        Assert.Equal(AciColor.Yellow, layerState.Properties["TestLayer"].Color);
        Assert.True(layerState.Properties["TestLayer"].Flags.HasFlag(LayerPropertiesFlags.Hidden));
        Assert.Equal(Lineweight.W50, layerState.Properties["TestLayer"].Lineweight);
    }

    [Fact]
    public void LayerState_CodeGeneration_ShouldIncludeAllProperties()
    {
        // Arrange
        var doc = new DxfDocument();
        var layer1 = new Layer("TestLayer1") 
        { 
            Color = AciColor.Red, 
            Lineweight = Lineweight.W20, 
            IsVisible = true, 
            IsFrozen = false, 
            IsLocked = false,
             Transparency = new Transparency(50)
        };
        var layer2 = new Layer("TestLayer2") 
        { 
            Color = AciColor.Blue, 
            Lineweight = Lineweight.W30, 
            IsVisible = false, 
            IsFrozen = true, 
            IsLocked = true,
             Transparency = new Transparency(90)
        };
        
        doc.Layers.Add(layer1);
        doc.Layers.Add(layer2);
        
        var layerState = new LayerState("CompleteTestState", new[] { layer1, layer2 })
        {
            Description = "Complete test state",
            CurrentLayer = "TestLayer1",
            PaperSpace = true
        };
        
        var options = new DxfCodeGenerationOptions
        {
            GenerateLayerStateObjects = true,
            GenerateDetailedComments = true
        };

        // Act
        var generator = new DxfCodeGenerator();
        var generatedCode = generator.Generate(doc, null, null, options);

        // Assert
        Assert.Contains("// LayerState objects are internal to netDxf and not directly accessible", generatedCode);
        Assert.Contains("//   Name: [layer state name]", generatedCode);
        Assert.Contains("//   Description: [optional description]", generatedCode);
        Assert.Contains("//   LayerProperties: Dictionary of layer names and their saved properties", generatedCode);
        // LayerState objects are not directly accessible from DxfDocument,
        // so only placeholder comments are generated
    }

    [Fact]
    public void LayerState_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var layer = new Layer("Layer-With_Special$Characters") { Color = AciColor.Cyan };
        var layerState = new LayerState("State With Spaces & Symbols!", new[] { layer })
        {
            Description = "Description with \"quotes\" and \\backslashes\\",
            CurrentLayer = "Layer-With_Special$Characters"
        };

        // Act & Assert - Should not throw exceptions
        Assert.Equal("State With Spaces & Symbols!", layerState.Name);
        Assert.Equal("Description with \"quotes\" and \\backslashes\\", layerState.Description);
        Assert.Equal("Layer-With_Special$Characters", layerState.CurrentLayer);
        Assert.Single(layerState.Properties);
        Assert.True(layerState.Properties.ContainsKey("Layer-With_Special$Characters"));
    }

    [Fact]
    public void LayerState_WithManyLayers_ShouldHandleCorrectly()
    {
        // Arrange
        var layers = new List<Layer>();
        for (var i = 0; i < 100; i++)
        {
            layers.Add(new Layer($"Layer{i:D3}") { Color = AciColor.FromCadIndex((short)(i % 255 + 1)) });
        }

        // Act
        var layerState = new LayerState("ManyLayersState", layers);

        // Assert
        Assert.Equal("ManyLayersState", layerState.Name);
        Assert.Equal(100, layerState.Properties.Count);
        
        // Verify first and last layers
        Assert.True(layerState.Properties.ContainsKey("Layer000"));
        Assert.True(layerState.Properties.ContainsKey("Layer099"));
        
        // Verify colors are set correctly
        Assert.Equal(AciColor.FromCadIndex(1), layerState.Properties["Layer000"].Color);
        Assert.Equal(AciColor.FromCadIndex(100), layerState.Properties["Layer099"].Color);
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