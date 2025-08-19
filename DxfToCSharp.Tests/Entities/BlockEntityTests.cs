using System.Linq;
using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;
using Xunit;

namespace DxfToCSharp.Tests.Entities;

public class BlockEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Block_BasicRoundTrip_ShouldPreserveBlockProperties()
    {
        // Arrange
        var entities = new List<EntityObject>
        {
            new Line(new Vector2(0, 0), new Vector2(10, 10)),
            new Circle(new Vector3(5, 5, 0), 2.5)
        };
        var originalBlock = new Block("TestBlock", entities);
        originalBlock.Origin = new Vector3(1, 1, 0);
        originalBlock.Description = "Test block description";

        // Create an insert to test the block
        var originalInsert = new Insert(originalBlock, new Vector3(10, 10, 0));

        // Act & Assert
        PerformRoundTripTest(originalInsert, (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);
            Assert.Equal(original.Block.Description, recreated.Block.Description);
            AssertVector3Equal(original.Block.Origin, recreated.Block.Origin);
            Assert.Equal(original.Block.Entities.Count, recreated.Block.Entities.Count);
            AssertVector3Equal(original.Position, recreated.Position);
        });
    }

    [Fact]
    public void Block_WithMultipleEntities_ShouldPreserveAllEntities()
    {
        // Arrange
        var entities = new List<EntityObject>
        {
            new Line(new Vector2(0, 0), new Vector2(10, 0)),
            new Line(new Vector2(10, 0), new Vector2(10, 10)),
            new Line(new Vector2(10, 10), new Vector2(0, 10)),
            new Line(new Vector2(0, 10), new Vector2(0, 0)),
            new Circle(new Vector3(5, 5, 0), 2.0)
        };
        var originalBlock = new Block("RectangleWithCircle", entities);
        var originalInsert = new Insert(originalBlock, new Vector3(20, 20, 0));

        // Act & Assert
        PerformRoundTripTest(originalInsert, (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);
            Assert.Equal(original.Block.Entities.Count, recreated.Block.Entities.Count);
            
            // Check that we have the expected entity types
            var originalLines = original.Block.Entities.OfType<Line>().Count();
            var originalCircles = original.Block.Entities.OfType<Circle>().Count();
            var recreatedLines = recreated.Block.Entities.OfType<Line>().Count();
            var recreatedCircles = recreated.Block.Entities.OfType<Circle>().Count();
            
            Assert.Equal(originalLines, recreatedLines);
            Assert.Equal(originalCircles, recreatedCircles);
        });
    }

    [Fact]
    public void Block_WithAttributeDefinitions_ShouldPreserveAttributes()
    {
        // Arrange
        var entities = new List<EntityObject>
        {
            new Line(new Vector2(0, 0), new Vector2(10, 10))
        };
        
        var attributeDefinitions = new List<AttributeDefinition>
        {
            new AttributeDefinition("TAG1")
            {
                Prompt = "Enter value 1:",
                Value = "Default1",
                Position = new Vector3(5, 2, 0),
                Height = 2.0
            },
            new AttributeDefinition("TAG2")
            {
                Prompt = "Enter value 2:",
                Value = "Default2",
                Position = new Vector3(5, 4, 0),
                Height = 1.5
            }
        };
        
        var originalBlock = new Block("BlockWithAttributes", entities, attributeDefinitions);
        var originalInsert = new Insert(originalBlock, new Vector3(0, 0, 0));

        // Act & Assert
        PerformRoundTripTest(originalInsert, (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);
            Assert.Equal(original.Block.AttributeDefinitions.Count, recreated.Block.AttributeDefinitions.Count);
            Assert.Equal(original.Attributes.Count, recreated.Attributes.Count);
            
            // Check attribute definitions
            foreach (var originalAttDef in original.Block.AttributeDefinitions.Values)
            {
                Assert.True(recreated.Block.AttributeDefinitions.ContainsTag(originalAttDef.Tag));
                var recreatedAttDef = recreated.Block.AttributeDefinitions[originalAttDef.Tag];
                Assert.Equal(originalAttDef.Tag, recreatedAttDef.Tag);
                Assert.Equal(originalAttDef.Prompt, recreatedAttDef.Prompt);
                Assert.Equal(originalAttDef.Value, recreatedAttDef.Value);
                AssertDoubleEqual(originalAttDef.Height, recreatedAttDef.Height);
            }
        });
    }

    [Fact]
    public void Block_WithCustomOrigin_ShouldPreserveOrigin()
    {
        // Arrange
        var entities = new List<EntityObject>
        {
            new Circle(new Vector3(0, 0, 0), 5.0)
        };
        var originalBlock = new Block("CircleBlock", entities)
        {
            Origin = new Vector3(2.5, 2.5, 0),
            Description = "Circle with custom origin"
        };
        var originalInsert = new Insert(originalBlock, new Vector3(15, 15, 0));

        // Act & Assert
        PerformRoundTripTest(originalInsert, (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);
            Assert.Equal(original.Block.Description, recreated.Block.Description);
            AssertVector3Equal(original.Block.Origin, recreated.Block.Origin);
            AssertVector3Equal(original.Position, recreated.Position);
        });
    }

    [Fact]
    public void Block_EmptyBlock_ShouldPreserveBasicProperties()
    {
        // Arrange
        var originalBlock = new Block("EmptyBlock")
        {
            Description = "An empty block for testing",
            Origin = new Vector3(1, 2, 3)
        };
        var originalInsert = new Insert(originalBlock, new Vector3(10, 20, 30));

        // Act & Assert
        PerformRoundTripTest(originalInsert, (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);
            Assert.Equal(original.Block.Description, recreated.Block.Description);
            AssertVector3Equal(original.Block.Origin, recreated.Block.Origin);
            Assert.Equal(original.Block.Entities.Count, recreated.Block.Entities.Count);
            AssertVector3Equal(original.Position, recreated.Position);
        });
    }
}