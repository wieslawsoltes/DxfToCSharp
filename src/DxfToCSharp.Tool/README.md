# DxfToCSharp.Tool

A .NET global tool for generating C# code from DXF files using the DxfToCSharp.Core library.

## Installation

Install the tool globally using the .NET CLI:

```bash
dotnet tool install -g DxfToCSharp.Tool
```

## Usage

The tool is available as `dxf2cs` command after installation.

### Basic Usage

```bash
# Convert a single DXF file
dxf2cs -i drawing.dxf

# Convert multiple DXF files
dxf2cs -i drawing1.dxf drawing2.dxf drawing3.dxf

# Convert all DXF files in a directory
dxf2cs -i /path/to/dxf/files

# Convert all DXF files in a directory recursively
dxf2cs -i /path/to/dxf/files --recursive
```

### Advanced Options

```bash
# Specify output directory
dxf2cs -i drawing.dxf -o /path/to/output

# Custom class name
dxf2cs -i drawing.dxf --class-name MyDrawingGenerator

# Add namespace
dxf2cs -i drawing.dxf --namespace MyCompany.Drawings

# Skip generating certain parts
dxf2cs -i drawing.dxf --no-header --no-tables --no-objects

# Verbose output
dxf2cs -i drawing.dxf --verbose
```

## Command Line Options

### Input/Output Options
- `-i, --input <files/directories>` - Input DXF file(s) or directory(ies) to process (required)
- `-o, --output <directory>` - Output directory (default: same as input file directory)
- `-r, --recursive` - Process directories recursively
- `-v, --verbose` - Enable verbose output

### Code Generation Options
- `-c, --class-name <name>` - Custom class name for generated code
- `-n, --namespace <namespace>` - Namespace for generated code
- `--no-header` - Skip generating header comments
- `--no-using` - Skip generating using statements
- `--no-class` - Generate only the method body without class wrapper
- `--no-tables` - Skip generating table definitions (layers, linetypes, etc.)
- `--no-entities` - Skip generating entities
- `--no-objects` - Skip generating objects

## Examples

### Convert a single file with custom settings
```bash
dxf2cs -i architectural-plan.dxf \
       --class-name ArchitecturalPlanGenerator \
       --namespace MyCompany.CAD.Drawings \
       --output ./generated
```

### Batch convert all DXF files in a project
```bash
dxf2cs -i ./drawings --recursive \
       --namespace MyProject.Drawings \
       --output ./src/Generated \
       --verbose
```

### Generate minimal code (no tables, no objects)
```bash
dxf2cs -i simple-drawing.dxf \
       --no-tables \
       --no-objects \
       --no-header
```

## Output

The tool generates C# files with the same name as the input DXF files but with a `.cs` extension. Each generated file contains:

- Using statements (unless `--no-using` is specified)
- Header comments with generation info (unless `--no-header` is specified)
- A static class with a `Create()` method that returns a `DxfDocument` (unless `--no-class` is specified)
- Code to recreate all the entities, tables, and objects from the original DXF file

## Requirements

- .NET 6.0 or later
- The generated code requires the `netDxf.netstandard` NuGet package

## License

MIT License - see the LICENSE file for details.