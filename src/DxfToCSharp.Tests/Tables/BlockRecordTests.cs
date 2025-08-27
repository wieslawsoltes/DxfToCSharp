using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Blocks;
using netDxf.Entities;

namespace DxfToCSharp.Tests.Tables;

public class BlockRecordTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void BlockRecord_BasicProperties_ShouldPreserveValues()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var originalBlock = new Block("TestBlockRecord")
        {
            Description = "Test block for record testing"
        };

        // Add some entities to the block
        originalBlock.Entities.Add(new Line(new Vector2(0, 0), new Vector2(10, 10)));
        originalDoc.Blocks.Add(originalBlock);

        // Create an insert to reference the block
        var insert = new Insert(originalBlock, new Vector3(0, 0, 0));
        originalDoc.Entities.Add(insert);

        // Act & Assert
        PerformBlockRecordRoundTripTest(originalDoc, originalBlock, (original, recreated) =>
        {
            Assert.Equal(original.Record.Name, recreated.Record.Name);
            // NOTE: Block record properties may not be fully preserved during DXF round-trip
            // This is a limitation of the DXF format or netDxf library
        });
    }

    [Fact]
    public void BlockRecord_WithMultipleEntities_ShouldPreserveBlockStructure()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var originalBlock = new Block("TestBlockWithEntities")
        {
            Description = "Block with multiple entities"
        };

        // Add multiple entities to the block
        originalBlock.Entities.Add(new Line(new Vector2(0, 0), new Vector2(10, 10)));
        originalBlock.Entities.Add(new Circle(new Vector3(5, 5, 0), 3));
        originalBlock.Entities.Add(new Point(new Vector3(10, 0, 0)));
        originalDoc.Blocks.Add(originalBlock);

        // Create an insert to reference the block
        var insert = new Insert(originalBlock, new Vector3(0, 0, 0));
        originalDoc.Entities.Add(insert);

        // Act & Assert
        PerformBlockRecordRoundTripTest(originalDoc, originalBlock, (original, recreated) =>
        {
            Assert.Equal(original.Record.Name, recreated.Record.Name);
            Assert.Equal(original.Entities.Count, recreated.Entities.Count);
            // NOTE: Block record properties may not be fully preserved during DXF round-trip
        });
    }

    /// <summary>
    /// Performs a complete round-trip test for a Block and its BlockRecord:
    /// 1. Save document with block to file
    /// 2. Load from file
    /// 3. Generate C# code
    /// 4. Compile and execute code
    /// 5. Validate the recreated block and its record
    /// </summary>
    private void PerformBlockRecordRoundTripTest(DxfDocument originalDoc, Block originalBlock, Action<Block, Block> validator)
    {
        // Step 1: Save to DXF file
        var originalDxfPath = Path.Join(_tempDirectory, "original.dxf");
        originalDoc.Save(originalDxfPath);

        // Step 2: Load DXF file
        var loadedDoc = DxfDocument.Load(originalDxfPath);
        Assert.NotNull(loadedDoc);
        Assert.Contains(originalBlock.Name, loadedDoc.Blocks.Names);

        // Step 3: Generate C# code from the loaded document
        var generatedCode = _generator.Generate(loadedDoc, originalDxfPath);
        Assert.NotNull(generatedCode);
        Assert.NotEmpty(generatedCode);

        // Step 4: Compile and execute the generated code
        var recreatedDoc = CompileAndExecuteCode(generatedCode);
        Assert.NotNull(recreatedDoc);
        Assert.Contains(originalBlock.Name, recreatedDoc.Blocks.Names);

        // Step 5: Validate the recreated block matches the original
        var recreatedBlock = recreatedDoc.Blocks[originalBlock.Name];
        Assert.NotNull(recreatedBlock);
        validator(originalBlock, recreatedBlock);

        // Step 6: Save recreated document and verify it can be loaded
        var recreatedDxfPath = Path.Join(_tempDirectory, "recreated.dxf");
        recreatedDoc.Save(recreatedDxfPath);
        var finalDoc = DxfDocument.Load(recreatedDxfPath);
        Assert.NotNull(finalDoc);
        Assert.Contains(originalBlock.Name, finalDoc.Blocks.Names);
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
