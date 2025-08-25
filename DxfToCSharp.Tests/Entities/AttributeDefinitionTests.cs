using System;
using Xunit;
using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Core;
using Attribute = netDxf.Entities.Attribute;

namespace DxfToCSharp.Tests.Entities;

public class AttributeDefinitionTests : IDisposable
{
    private readonly DxfCodeGenerator _generator;

    public AttributeDefinitionTests()
    {
        _generator = new DxfCodeGenerator();
    }

    [Fact]
    public void AttributeDefinition_InBlock_GeneratesCorrectly()
    {
        // Arrange
        var doc = new DxfDocument();
        var block = new Block("TestBlock");
        
        var attributeDefinition = new AttributeDefinition("TAG1", 2.5, TextStyle.Default)
        {
            Prompt = "Prompt Text",
            Value = "Default Value",
            Position = new Vector3(10, 20, 0),
            Height = 2.5,
            Flags = AttributeFlags.Hidden
        };
        
        block.AttributeDefinitions.Add(attributeDefinition);
        doc.Blocks.Add(block);
        
        // Add an Insert entity to reference the block so it gets marked as "used"
        var insert = new Insert(block, new Vector3(0, 0, 0));
        doc.Entities.Add(insert);
        
        var options = new DxfCodeGenerationOptions
        {
            GenerateAttributeDefinitionEntities = true,
            GenerateDetailedComments = true
        };

        // Act
        var generatedCode = _generator.Generate(doc, null, null, options);

        // Assert
        Assert.NotNull(generatedCode);
        
        // Debug: Print the generated code to see what's actually generated
        System.Console.WriteLine("Generated code:");
        System.Console.WriteLine(generatedCode);
        
        // Test that AttributeDefinition entities are generated correctly
        Assert.Contains("AttributeDefinition", generatedCode);
        Assert.Contains("TAG1", generatedCode);
        Assert.Contains("Prompt Text", generatedCode);
        Assert.Contains("Position", generatedCode);
        Assert.Contains("Height", generatedCode);
    }

    [Fact]
    public void Attribute_WithDefinition_GeneratesCorrectly()
    {
        // Arrange
        var doc = new DxfDocument();
        
        var attributeDefinition = new AttributeDefinition("ATTR_TAG", 1.5, TextStyle.Default);
        var attribute = new Attribute(attributeDefinition)
        {
            Value = "Test Value",
            Position = new Vector3(5, 10, 0)
        };
        
        // Create a block with the attribute definition
        var dummyBlock = new Block("DummyBlock");
        dummyBlock.AttributeDefinitions.Add(attributeDefinition);
        doc.Blocks.Add(dummyBlock);
        
        // Create an Insert entity which will automatically create attributes from definitions
        var insert = new Insert(dummyBlock)
        {
            Position = Vector3.Zero
        };
        
        // Modify the attribute value
        var attr = insert.Attributes.AttributeWithTag("ATTR_TAG");
        if (attr != null)
        {
            attr.Value = "Test Value";
            attr.Position = new Vector3(5, 10, 0);
        }
        
        doc.Entities.Add(insert);
        
        var options = new DxfCodeGenerationOptions
        {
            GenerateAttributeEntities = true,
            GenerateAttributeDefinitionEntities = true
        };

        // Act
        var generatedCode = _generator.Generate(doc, null, null, options);

        // Assert
        Assert.NotNull(generatedCode);
        Assert.Contains("Attribute", generatedCode);
        Assert.Contains("ATTR_TAG", generatedCode);
        Assert.Contains("Test Value", generatedCode);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}