# Getting Started with DxfToCSharp

DxfToCSharp is a .NET library for converting DXF files to C# code.

## Installation

Install the package via NuGet:

```bash
dotnet add package DxfToCSharp
```

## Basic Usage

```csharp
using DxfToCSharp.Core;

// Create a code generator with options
var options = new DxfCodeGenerationOptions
{
    GenerateLineEntities = true,
    GenerateArcEntities = true,
    GenerateCircleEntities = true
};

var generator = new DxfCodeGenerator(options);

// Generate C# code from DXF file
string dxfFilePath = "sample.dxf";
string generatedCode = generator.GenerateCode(dxfFilePath);

Console.WriteLine(generatedCode);
```

## Next Steps

- Check out the [API Reference](api-reference.md)
- See [Examples](examples.md) for more detailed usage
- Learn about [Contributing](contributing.md) to the project