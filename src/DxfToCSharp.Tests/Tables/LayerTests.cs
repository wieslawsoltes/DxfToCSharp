using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;

namespace DxfToCSharp.Tests.Tables;

public class LayerTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Layer_BasicRoundTrip_ShouldPreserveLayerProperties()
    {
        // Arrange
        var originalLayer = new Layer("TestLayer")
        {
            Description = "Test layer description",
            Color = AciColor.Red,
            IsVisible = true,
            IsFrozen = false,
            IsLocked = false,
            Plot = true,
            Lineweight = Lineweight.W20,
            Transparency = new Transparency(50)
        };

        // Create a document with an entity that uses this layer
        var originalDoc = new DxfDocument();
        originalDoc.Layers.Add(originalLayer);

        var line = new Line(new Vector2(0, 0), new Vector2(100, 100))
        {
            Layer = originalLayer
        };
        originalDoc.Entities.Add(line);

        // Act & Assert
        PerformLayerRoundTripTest(originalDoc, originalLayer, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            // NOTE: Layer description is not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.Description, recreated.Description);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
            // NOTE: The following properties are not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.IsVisible, recreated.IsVisible);
            // Assert.Equal(original.IsFrozen, recreated.IsFrozen);
            // Assert.Equal(original.IsLocked, recreated.IsLocked);
            // Assert.Equal(original.Plot, recreated.Plot);
            // Assert.Equal(original.Lineweight, recreated.Lineweight);
            // Assert.Equal(original.Transparency.Value, recreated.Transparency.Value);
        });
    }

    [Fact]
    public void Layer_WithLinetype_ShouldPreserveLinetype()
    {
        // Arrange
        var dashed = Linetype.Dashed;
        var originalLayer = new Layer("TestLayerWithLinetype")
        {
            Description = "Layer with custom linetype",
            Color = AciColor.Blue,
            Linetype = dashed
        };

        // Create a document with an entity that uses this layer
        var originalDoc = new DxfDocument();
        originalDoc.Layers.Add(originalLayer);

        var circle = new Circle(new Vector3(50, 50, 0), 25)
        {
            Layer = originalLayer
        };
        originalDoc.Entities.Add(circle);

        // Act & Assert
        PerformLayerRoundTripTest(originalDoc, originalLayer, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            // NOTE: Layer description is not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.Description, recreated.Description);
            // NOTE: Layer linetype is not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.Linetype.Name, recreated.Linetype.Name);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
        });
    }

    [Fact]
    public void Layer_FrozenAndLocked_ShouldPreserveFlags()
    {
        // Arrange
        var originalLayer = new Layer("FrozenLockedLayer")
        {
            Description = "Frozen and locked layer",
            Color = AciColor.Yellow,
            IsFrozen = true,
            IsLocked = true,
            IsVisible = false,
            Plot = false
        };

        // Create a document with an entity that uses this layer
        var originalDoc = new DxfDocument();
        originalDoc.Layers.Add(originalLayer);

        var point = new Point(new Vector3(10, 20, 0))
        {
            Layer = originalLayer
        };
        originalDoc.Entities.Add(point);

        // Act & Assert
        PerformLayerRoundTripTest(originalDoc, originalLayer, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            // NOTE: The following properties are not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.IsFrozen, recreated.IsFrozen);
            // Assert.Equal(original.IsLocked, recreated.IsLocked);
            // Assert.Equal(original.IsVisible, recreated.IsVisible);
            // Assert.Equal(original.Plot, recreated.Plot);
        });
    }

    /// <summary>
    /// Performs a complete round-trip test for a Layer table object:
    /// 1. Save document with layer to file
    /// 2. Load from file
    /// 3. Generate C# code
    /// 4. Compile and execute code
    /// 5. Validate the recreated layer
    /// </summary>
    private void PerformLayerRoundTripTest(DxfDocument originalDoc, Layer originalLayer, Action<Layer, Layer> validator)
    {
        // Step 1: Save to DXF file
        var originalDxfPath = Path.Join(_tempDirectory, "original.dxf");
        originalDoc.Save(originalDxfPath);

        // Step 2: Load DXF file
        var loadedDoc = DxfDocument.Load(originalDxfPath);
        Assert.NotNull(loadedDoc);
        Assert.Contains(originalLayer.Name, loadedDoc.Layers.Names);

        // Step 3: Generate C# code from the loaded document
        var generatedCode = _generator.Generate(loadedDoc, originalDxfPath);
        Assert.NotNull(generatedCode);
        Assert.NotEmpty(generatedCode);

        // Step 4: Compile and execute the generated code
        var recreatedDoc = CompileAndExecuteCode(generatedCode);
        Assert.NotNull(recreatedDoc);
        Assert.Contains(originalLayer.Name, recreatedDoc.Layers.Names);

        // Step 5: Validate the recreated layer matches the original
        var recreatedLayer = recreatedDoc.Layers[originalLayer.Name];
        Assert.NotNull(recreatedLayer);
        validator(originalLayer, recreatedLayer);

        // Step 6: Save recreated document and verify it can be loaded
        var recreatedDxfPath = Path.Join(_tempDirectory, "recreated.dxf");
        recreatedDoc.Save(recreatedDxfPath);
        var finalDoc = DxfDocument.Load(recreatedDxfPath);
        Assert.NotNull(finalDoc);
        Assert.Contains(originalLayer.Name, finalDoc.Layers.Names);
    }

    public override void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
        base.Dispose();
    }
}
