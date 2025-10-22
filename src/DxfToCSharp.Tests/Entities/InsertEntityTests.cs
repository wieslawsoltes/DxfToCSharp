using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Tables;

namespace DxfToCSharp.Tests.Entities;

public class InsertEntityTests : RoundTripTestBase
{
    [Fact]
    public void Insert_BasicRoundTrip_ShouldPreserveInsertProperties()
    {
        // Arrange
        var blockEntities = new List<EntityObject>
        {
            new Line(new Vector2(0, 0), new Vector2(10, 10)),
            new Circle(new Vector3(5, 5, 0), 2.5)
        };
        var block = new Block("TestBlock", blockEntities);
        var originalInsert = new Insert(block, new Vector3(10, 20, 0));

        // Act & Assert
        PerformRoundTripTest(originalInsert, (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertVector3Equal(original.Scale, recreated.Scale);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
        });
    }

    [Fact]
    public void Insert_WithScale_ShouldPreserveScale()
    {
        // Arrange
        var blockEntities = new List<EntityObject>
        {
            new Line(new Vector2(0, 0), new Vector2(10, 5))
        };
        var block = new Block("ScaledBlock", blockEntities);
        var originalInsert = new Insert(block, new Vector3(0, 0, 0))
        {
            Scale = new Vector3(2.0, 1.5, 1.0)
        };

        // Act & Assert
        PerformRoundTripTest(originalInsert, (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertVector3Equal(original.Scale, recreated.Scale);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
        });
    }

    [Fact]
    public void Insert_WithRotation_ShouldPreserveRotation()
    {
        // Arrange
        var blockEntities = new List<EntityObject>
        {
            new Line(new Vector2(0, 0), new Vector2(10, 0))
        };
        var block = new Block("RotatedBlock", blockEntities);
        var originalInsert = new Insert(block, new Vector3(15, 15, 0))
        {
            Rotation = 45.0 // 45 degrees
        };

        // Act & Assert
        PerformRoundTripTest(originalInsert, (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertVector3Equal(original.Scale, recreated.Scale);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
        });
    }

    [Fact]
    public void Insert_WithScaleAndRotation_ShouldPreserveBoth()
    {
        // Arrange
        var blockEntities = new List<EntityObject>
        {
            new Circle(new Vector3(0, 0, 0), 5.0),
            new Line(new Vector2(-5, 0), new Vector2(5, 0))
        };
        var block = new Block("TransformedBlock", blockEntities);
        var originalInsert = new Insert(block, new Vector3(25, 25, 0))
        {
            Scale = new Vector3(1.5, 0.8, 1.0),
            Rotation = 30.0
        };

        // Act & Assert
        PerformRoundTripTest(originalInsert, (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertVector3Equal(original.Scale, recreated.Scale);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
        });
    }

    [Fact]
    public void Insert_WithAttributes_ShouldPreserveAttributes()
    {
        // Arrange
        var blockEntities = new List<EntityObject>
        {
            new Line(new Vector2(0, 0), new Vector2(10, 10))
        };

        var attributeDefinitions = new List<AttributeDefinition>
        {
            new AttributeDefinition("PART_NUMBER")
            {
                Prompt = "Enter part number:",
                Value = "P001",
                Position = new Vector3(5, 2, 0),
                Height = 2.0
            },
            new AttributeDefinition("DESCRIPTION")
            {
                Prompt = "Enter description:",
                Value = "Test Part",
                Position = new Vector3(5, 4, 0),
                Height = 1.5
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
            Assert.Equal(original.Attributes.Count, recreated.Attributes.Count);

            foreach (var originalAtt in original.Attributes)
            {
                var recreatedAtt = recreated.Attributes.FirstOrDefault(a => a.Tag == originalAtt.Tag);
                Assert.NotNull(recreatedAtt);
                Assert.Equal(originalAtt.Tag, recreatedAtt.Tag);
                Assert.Equal(originalAtt.Value, recreatedAtt.Value);
                AssertVector3Equal(originalAtt.Position, recreatedAtt.Position);
                AssertDoubleEqual(originalAtt.Height, recreatedAtt.Height);
            }
        });
    }

    [Fact]
    public void Insert_WithPropertyOverrides_ShouldPreserveOverrides()
    {
        // Arrange
        var blockEntities = new List<EntityObject>
        {
            new Line(new Vector2(0, 0), new Vector2(10, 0))
        };
        var block = new Block("OverrideBlock", blockEntities);

        var customLayer = new Layer("InsertLayer")
        {
            Color = new AciColor(2),
            Lineweight = Lineweight.W40
        };

        var linetypeSegments = new List<LinetypeSegment>
        {
            new LinetypeSimpleSegment(0.4),
            new LinetypeSimpleSegment(-0.2)
        };
        var customLinetype = new Linetype("InsertLinetype", linetypeSegments, "Insert linetype");

        var originalInsert = new Insert(block, new Vector3(5, 5, 0))
        {
            Layer = customLayer,
            Color = new AciColor(120),
            Linetype = customLinetype,
            Lineweight = Lineweight.W20,
            LinetypeScale = 0.75,
            Transparency = new Transparency(30)
        };

        // Act & Assert
        PerformRoundTripTest(originalInsert, (original, recreated) =>
        {
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
            Assert.Equal(original.Linetype.Name, recreated.Linetype.Name);
            Assert.Equal(original.Lineweight, recreated.Lineweight);
            AssertDoubleEqual(original.LinetypeScale, recreated.LinetypeScale);
            Assert.Equal(original.Transparency.Value, recreated.Transparency.Value);
        });
    }

    [Fact]
    public void Insert_MultipleInserts_ShouldPreserveIndependently()
    {
        // Arrange - This test creates a document with multiple inserts of the same block
        var blockEntities = new List<EntityObject>
        {
            new Circle(new Vector3(0, 0, 0), 3.0)
        };
        var block = new Block("CircleBlock", blockEntities);

        var insert1 = new Insert(block, new Vector3(0, 0, 0))
        {
            Scale = new Vector3(1.0, 1.0, 1.0),
            Rotation = 0.0
        };
        var insert2 = new Insert(block, new Vector3(20, 0, 0))
        {
            Scale = new Vector3(2.0, 2.0, 1.0),
            Rotation = 45.0
        };
        var insert3 = new Insert(block, new Vector3(0, 20, 0))
        {
            Scale = new Vector3(0.5, 1.5, 1.0),
            Rotation = 90.0
        };

        // Create a document with multiple inserts
        var originalDoc = new DxfDocument();
        originalDoc.Entities.Add(insert1);
        originalDoc.Entities.Add(insert2);
        originalDoc.Entities.Add(insert3);

        // Save and reload
        var tempPath = Path.Join(Path.GetTempPath(), "multiple_inserts_test.dxf");
        originalDoc.Save(tempPath);
        var loadedDoc = DxfDocument.Load(tempPath);

        // Act & Assert - Test each insert individually
        PerformRoundTripTest((Insert)insert1.Clone(), (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertVector3Equal(original.Scale, recreated.Scale);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
        });

        PerformRoundTripTest((Insert)insert2.Clone(), (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertVector3Equal(original.Scale, recreated.Scale);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
        });

        PerformRoundTripTest((Insert)insert3.Clone(), (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertVector3Equal(original.Scale, recreated.Scale);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
        });
    }

    [Fact]
    public void Insert_WithNegativeScale_ShouldPreserveNegativeScale()
    {
        // Arrange
        var blockEntities = new List<EntityObject>
        {
            new Line(new Vector2(0, 0), new Vector2(5, 5))
        };
        var block = new Block("MirroredBlock", blockEntities);
        var originalInsert = new Insert(block, new Vector3(10, 10, 0))
        {
            Scale = new Vector3(-1.0, 1.0, 1.0) // Mirror in X direction
        };

        // Act & Assert
        PerformRoundTripTest(originalInsert, (original, recreated) =>
        {
            Assert.Equal(original.Block.Name, recreated.Block.Name);
            AssertVector3Equal(original.Position, recreated.Position);
            AssertVector3Equal(original.Scale, recreated.Scale);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
        });
    }
}
