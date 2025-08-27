---
title: DxfToCSharp Documentation
---

# DxfToCSharp Documentation

Welcome to the DxfToCSharp documentation! DxfToCSharp is a powerful .NET library and toolset for generating C# code from DXF (Drawing Exchange Format) files.

## 🚀 Quick Navigation

- **[Getting Started](docs/getting-started.md)** - Learn how to install and use DxfToCSharp
- **[API Reference](api/index.md)** - Detailed API documentation
- **[Examples](docs/examples.md)** - Code examples and usage patterns
- **[Contributing](docs/contributing.md)** - How to contribute to the project

## ✨ Key Features

🎯 **High-Performance Code Generation** - Convert DXF files to clean, readable C# code
🔧 **Multiple Interfaces** - GUI application, command-line tool, and programmatic API
📐 **Comprehensive DXF Support** - Supports all major DXF entities and drawing elements
🎨 **Modern UI** - Beautiful Avalonia-based desktop application
⚡ **Cross-Platform** - Runs on Windows, macOS, and Linux
🧪 **Well-Tested** - Comprehensive test suite with high code coverage

## 🛠️ Installation

### Library (NuGet)

```bash
# Core library
dotnet add package DxfToCSharp.Core

# Compilation services
dotnet add package DxfToCSharp.Compilation
```

### Command Line Tool

```bash
# Install as global tool
dotnet tool install -g DxfToCSharp.Tool

# Use the tool
dxf2cs input.dxf output.cs
```

## 📖 Quick Start

### Using the Library

```csharp
using DxfToCSharp.Core;
using netDxf;

// Load DXF document
var dxfDocument = DxfDocument.Load("drawing.dxf");

// Configure generation options
var options = new DxfCodeGenerationOptions
{
    Namespace = "MyProject.Drawings",
    ClassName = "GeneratedDrawing"
};

// Generate C# code
var generator = new DxfCodeGenerator();
string csharpCode = generator.GenerateCode(dxfDocument, options);

// Save to file
File.WriteAllText("GeneratedDrawing.cs", csharpCode);
```

### Using the Command Line Tool

```bash
# Basic usage
dxf2cs input.dxf output.cs

# With custom options
dxf2cs input.dxf output.cs --namespace "MyProject" --class "Drawing"
```

## 🏗️ Architecture

The project is organized into several focused packages:

- **DxfToCSharp.Core** - Core code generation engine
- **DxfToCSharp.Compilation** - Compilation and validation services
- **DxfToCSharp.Tool** - Command-line interface
- **DxfToCSharp** - Desktop GUI application
- **DxfToCSharp.Tests** - Comprehensive test suite

## 📄 License

This project is licensed under the GNU Affero General Public License v3.0 (AGPL-3.0).

## 👨‍💻 Author

**Wiesław Šoltés**
- GitHub: [@wieslawsoltes](https://github.com/wieslawsoltes)
- Website: [GitHub Profile](https://github.com/wieslawsoltes)

---

Made with ❤️ by Wiesław Šoltés