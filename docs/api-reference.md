# API Reference

This section provides detailed information about the DxfToCSharp API.

## Core Classes

The main classes you'll work with include:

### DxfCodeGenerator

The primary class for generating C# code from DXF files.

**Namespace:** `DxfToCSharp.Core`

**Key Methods:**
- `GenerateCode(string dxfFilePath)` - Generates C# code from a DXF file
- `GenerateCode(DxfDocument document)` - Generates C# code from a DXF document object

### DxfCodeGenerationOptions

Configuration options for code generation.

**Namespace:** `DxfToCSharp.Core`

**Key Properties:**
- `GenerateLineEntities` - Enable/disable line entity generation
- `GenerateArcEntities` - Enable/disable arc entity generation
- `GenerateCircleEntities` - Enable/disable circle entity generation
- `GenerateTextEntities` - Enable/disable text entity generation
- And many more entity-specific options...

### CompilationService

Service for compiling generated C# code.

**Namespace:** `DxfToCSharp.Compilation`

**Key Methods:**
- `CompileCode(string code)` - Compiles C# code and returns compilation result
- `ValidateCode(string code)` - Validates C# code without compilation

## Usage Patterns

### Basic Code Generation

```csharp
var generator = new DxfCodeGenerator();
string code = generator.GenerateCode("input.dxf");
```

### Advanced Configuration

```csharp
var options = new DxfCodeGenerationOptions
{
    GenerateLineEntities = true,
    GenerateArcEntities = false,
    IncludeComments = true
};

var generator = new DxfCodeGenerator(options);
string code = generator.GenerateCode("input.dxf");
```

### Code Compilation

```csharp
var compilationService = new CompilationService();
var result = compilationService.CompileCode(generatedCode);

if (result.Success)
{
    Console.WriteLine("Compilation successful!");
}
else
{
    Console.WriteLine($"Compilation failed: {result.ErrorMessage}");
}
```

For detailed API documentation, see the [API Documentation](../api/) section.