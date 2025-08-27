using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Tables;

namespace DxfToCSharp.Tests.Entities;

public class AttributeDefinitionTests : RoundTripTestBase, IDisposable
{

    [Fact]
    public void AttributeDefinition_BasicRoundTrip_ShouldPreserveProperties()
    {
        // Arrange
        var blockEntities = new List<EntityObject>
        {
            new Line(new Vector2(0, 0), new Vector2(10, 10))
        };

        var attributeDefinitions = new List<AttributeDefinition>
        {
            new AttributeDefinition("TAG1", 2.5, TextStyle.Default)
            {
                Prompt = "Prompt Text",
                Value = "Default Value",
                Position = new Vector3(10, 20, 0),
                Height = 2.5,
                Flags = AttributeFlags.Hidden
            }
        };

        var block = new Block("TestBlock", blockEntities, attributeDefinitions);
        var originalInsert = new Insert(block, new Vector3(0, 0, 0));

        // Act & Assert
        PerformRoundTripTest(originalInsert, (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);
            Assert.Equal(original.Block.AttributeDefinitions.Count, recreated.Block.AttributeDefinitions.Count);

            var originalAttDef = original.Block.AttributeDefinitions["TAG1"];
            var recreatedAttDef = recreated.Block.AttributeDefinitions["TAG1"];

            Assert.Equal(originalAttDef.Tag, recreatedAttDef.Tag);
            Assert.Equal(originalAttDef.Prompt, recreatedAttDef.Prompt);
            Assert.Equal(originalAttDef.Value, recreatedAttDef.Value);
            AssertVector3Equal(originalAttDef.Position, recreatedAttDef.Position);
            AssertDoubleEqual(originalAttDef.Height, recreatedAttDef.Height);
            Assert.Equal(originalAttDef.Flags, recreatedAttDef.Flags);
        });
    }

    [Fact]
    public void AttributeDefinition_WithMultipleAttributes_ShouldPreserveAll()
    {
        // Arrange
        var blockEntities = new List<EntityObject>
        {
            new Circle(new Vector3(0, 0, 0), 5.0)
        };

        var attributeDefinitions = new List<AttributeDefinition>
        {
            new AttributeDefinition("PART_NUMBER", 1.5, TextStyle.Default)
            {
                Prompt = "Enter part number:",
                Value = "P001",
                Position = new Vector3(5, 2, 0),
                Height = 1.5,
                Flags = AttributeFlags.None
            },
            new AttributeDefinition("DESCRIPTION", 1.2, TextStyle.Default)
            {
                Prompt = "Enter description:",
                Value = "Test Part",
                Position = new Vector3(5, 4, 0),
                Height = 1.2,
                Flags = AttributeFlags.Hidden
            }
        };

        var block = new Block("PartBlock", blockEntities, attributeDefinitions);
        var originalInsert = new Insert(block, new Vector3(0, 0, 0));

        // Modify attribute values
        originalInsert.Attributes.AttributeWithTag("PART_NUMBER").Value = "P123";
        originalInsert.Attributes.AttributeWithTag("DESCRIPTION").Value = "Modified Description";

        // Act & Assert
        PerformRoundTripTest(originalInsert, (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);
            Assert.Equal(original.Block.AttributeDefinitions.Count, recreated.Block.AttributeDefinitions.Count);
            Assert.Equal(original.Attributes.Count, recreated.Attributes.Count);

            // Check attribute definitions
            foreach (var originalAttDef in original.Block.AttributeDefinitions.Values)
            {
                var recreatedAttDef = recreated.Block.AttributeDefinitions[originalAttDef.Tag];
                Assert.NotNull(recreatedAttDef);
                Assert.Equal(originalAttDef.Tag, recreatedAttDef.Tag);
                Assert.Equal(originalAttDef.Prompt, recreatedAttDef.Prompt);
                AssertVector3Equal(originalAttDef.Position, recreatedAttDef.Position);
                AssertDoubleEqual(originalAttDef.Height, recreatedAttDef.Height);
                Assert.Equal(originalAttDef.Flags, recreatedAttDef.Flags);
            }

            // Check attribute values
            foreach (var originalAttr in original.Attributes)
            {
                var recreatedAttr = recreated.Attributes.AttributeWithTag(originalAttr.Tag);
                Assert.NotNull(recreatedAttr);
                Assert.Equal(originalAttr.Tag, recreatedAttr.Tag);
                Assert.Equal(originalAttr.Value, recreatedAttr.Value);
                AssertVector3Equal(originalAttr.Position, recreatedAttr.Position);
            }
        });
    }

    [Fact]
    public void AttributeDefinition_WithCustomTextStyle_ShouldPreserveStyle()
    {
        // Arrange
        var blockEntities = new List<EntityObject>
        {
            new Line(new Vector2(0, 0), new Vector2(20, 0))
        };

        var attributeDefinitions = new List<AttributeDefinition>
        {
            new AttributeDefinition("STYLED_TAG", 3.0, TextStyle.Default)
            {
                Prompt = "Enter styled text:",
                Value = "Styled Value",
                Position = new Vector3(10, 5, 0),
                Height = 3.0,
                Flags = AttributeFlags.None
            }
        };

        var block = new Block("StyledBlock", blockEntities, attributeDefinitions);
        var originalInsert = new Insert(block, new Vector3(0, 0, 0));

        // Act & Assert
        PerformRoundTripTest(originalInsert, (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);

            var originalAttDef = original.Block.AttributeDefinitions["STYLED_TAG"];
            var recreatedAttDef = recreated.Block.AttributeDefinitions["STYLED_TAG"];

            Assert.Equal(originalAttDef.Tag, recreatedAttDef.Tag);
            Assert.Equal(originalAttDef.Prompt, recreatedAttDef.Prompt);
            Assert.Equal(originalAttDef.Value, recreatedAttDef.Value);
            AssertVector3Equal(originalAttDef.Position, recreatedAttDef.Position);
            AssertDoubleEqual(originalAttDef.Height, recreatedAttDef.Height);
            Assert.Equal(originalAttDef.Flags, recreatedAttDef.Flags);

            // Note: TextStyle properties may not be fully preserved in round-trip
            // due to DXF format limitations, but the style name should be preserved
            Assert.Equal(originalAttDef.Style.Name, recreatedAttDef.Style.Name);
        });
    }

    [Fact]
    public void AttributeDefinition_WithRotationAndAlignment_ShouldPreserveTransformation()
    {
        // Arrange
        var blockEntities = new List<EntityObject>
          {
              new Solid(new Vector2(0, 0), new Vector2(10, 0), new Vector2(10, 5), new Vector2(0, 5))
          };

        var attributeDefinitions = new List<AttributeDefinition>
        {
            new AttributeDefinition("ROTATED_TAG", 2.0, TextStyle.Default)
            {
                Prompt = "Enter rotated text:",
                Value = "Rotated Value",
                Position = new Vector3(5, 2.5, 0),
                Height = 2.0,
                Rotation = 45.0, // 45 degrees
                Alignment = TextAlignment.MiddleCenter,
                Flags = AttributeFlags.None
            }
        };

        var block = new Block("RotatedBlock", blockEntities, attributeDefinitions);
        var originalInsert = new Insert(block, new Vector3(15, 15, 0));

        // Act & Assert
        PerformRoundTripTest(originalInsert, (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);

            var originalAttDef = original.Block.AttributeDefinitions["ROTATED_TAG"];
            var recreatedAttDef = recreated.Block.AttributeDefinitions["ROTATED_TAG"];

            Assert.Equal(originalAttDef.Tag, recreatedAttDef.Tag);
            Assert.Equal(originalAttDef.Prompt, recreatedAttDef.Prompt);
            Assert.Equal(originalAttDef.Value, recreatedAttDef.Value);
            AssertVector3Equal(originalAttDef.Position, recreatedAttDef.Position);
            AssertDoubleEqual(originalAttDef.Height, recreatedAttDef.Height);
            AssertDoubleEqual(originalAttDef.Rotation, recreatedAttDef.Rotation);
            Assert.Equal(originalAttDef.Alignment, recreatedAttDef.Alignment);
            Assert.Equal(originalAttDef.Flags, recreatedAttDef.Flags);
        });
    }

    public new void Dispose()
    {
        base.Dispose();
    }
}
