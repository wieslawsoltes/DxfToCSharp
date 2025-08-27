# Examples

This section contains practical examples of using DxfToCSharp.

## Basic DXF Parsing

### Simple Line Generation

```csharp
using DxfToCSharp.Core;

// Create generator with line entities enabled
var options = new DxfCodeGenerationOptions
{
    GenerateLineEntities = true
};

var generator = new DxfCodeGenerator(options);
string code = generator.GenerateCode("drawing_with_lines.dxf");

Console.WriteLine(code);
```

### Multiple Entity Types

```csharp
using DxfToCSharp.Core;

// Enable multiple entity types
var options = new DxfCodeGenerationOptions
{
    GenerateLineEntities = true,
    GenerateArcEntities = true,
    GenerateCircleEntities = true,
    GenerateTextEntities = true,
    GeneratePolylineEntities = true
};

var generator = new DxfCodeGenerator(options);
string code = generator.GenerateCode("complex_drawing.dxf");

// Save generated code to file
File.WriteAllText("GeneratedCode.cs", code);
```

## Code Generation with Custom Options

### Including Comments and Metadata

```csharp
using DxfToCSharp.Core;

var options = new DxfCodeGenerationOptions
{
    GenerateLineEntities = true,
    GenerateArcEntities = true,
    IncludeComments = true,
    IncludeMetadata = true,
    UseRegions = true
};

var generator = new DxfCodeGenerator(options);
string code = generator.GenerateCode("technical_drawing.dxf");
```

### Custom Namespace and Class Names

```csharp
using DxfToCSharp.Core;

var options = new DxfCodeGenerationOptions
{
    GenerateLineEntities = true,
    Namespace = "MyProject.GeneratedDrawings",
    ClassName = "TechnicalDrawing"
};

var generator = new DxfCodeGenerator(options);
string code = generator.GenerateCode("blueprint.dxf");
```

## Advanced Usage

### Batch Processing Multiple Files

```csharp
using DxfToCSharp.Core;
using System.IO;

var options = new DxfCodeGenerationOptions
{
    GenerateLineEntities = true,
    GenerateArcEntities = true,
    GenerateCircleEntities = true
};

var generator = new DxfCodeGenerator(options);
string inputDirectory = "./dxf_files";
string outputDirectory = "./generated_code";

// Process all DXF files in directory
foreach (string dxfFile in Directory.GetFiles(inputDirectory, "*.dxf"))
{
    string fileName = Path.GetFileNameWithoutExtension(dxfFile);
    string code = generator.GenerateCode(dxfFile);
    
    string outputPath = Path.Combine(outputDirectory, $"{fileName}.cs");
    File.WriteAllText(outputPath, code);
    
    Console.WriteLine($"Generated: {outputPath}");
}
```

### Code Compilation and Execution

```csharp
using DxfToCSharp.Core;
using DxfToCSharp.Compilation;

// Generate code
var generator = new DxfCodeGenerator();
string code = generator.GenerateCode("sample.dxf");

// Compile the generated code
var compilationService = new CompilationService();
var result = compilationService.CompileCode(code);

if (result.Success)
{
    Console.WriteLine("Code compiled successfully!");
    
    // You can now use the compiled assembly
    var assembly = result.Assembly;
    // ... use the assembly as needed
}
else
{
    Console.WriteLine($"Compilation failed:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  {error}");
    }
}
```

### Working with DXF Document Objects

```csharp
using DxfToCSharp.Core;
using netDxf;

// Load DXF document first
DxfDocument dxfDoc = DxfDocument.Load("input.dxf");

// Inspect the document before generation
Console.WriteLine($"Entities count: {dxfDoc.Entities.Count}");
Console.WriteLine($"Layers count: {dxfDoc.Layers.Count}");

// Generate code from document object
var generator = new DxfCodeGenerator();
string code = generator.GenerateCode(dxfDoc);

// Clean up
dxfDoc.Dispose();
```

## Error Handling

### Robust File Processing

```csharp
using DxfToCSharp.Core;
using System;

try
{
    var generator = new DxfCodeGenerator();
    string code = generator.GenerateCode("problematic.dxf");
    
    if (string.IsNullOrEmpty(code))
    {
        Console.WriteLine("Warning: Generated code is empty");
    }
    else
    {
        Console.WriteLine("Code generation successful");
    }
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"DXF file not found: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Invalid DXF format: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

## Integration Examples

### Console Application

```csharp
using DxfToCSharp.Core;
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: DxfToCSharp <input.dxf> <output.cs>");
            return;
        }
        
        string inputFile = args[0];
        string outputFile = args[1];
        
        var generator = new DxfCodeGenerator();
        string code = generator.GenerateCode(inputFile);
        
        File.WriteAllText(outputFile, code);
        Console.WriteLine($"Generated C# code saved to: {outputFile}");
    }
}
```

### Web API Integration

```csharp
using DxfToCSharp.Core;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DxfController : ControllerBase
{
    [HttpPost("convert")]
    public async Task<IActionResult> ConvertDxf(IFormFile dxfFile)
    {
        if (dxfFile == null || dxfFile.Length == 0)
            return BadRequest("No file uploaded");
            
        // Save uploaded file temporarily
        var tempPath = Path.GetTempFileName();
        using (var stream = new FileStream(tempPath, FileMode.Create))
        {
            await dxfFile.CopyToAsync(stream);
        }
        
        try
        {
            var generator = new DxfCodeGenerator();
            string code = generator.GenerateCode(tempPath);
            
            return Ok(new { GeneratedCode = code });
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}
```