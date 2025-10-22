# DxfToCSharp

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet" alt=".NET 9.0" />
  <img src="https://img.shields.io/badge/License-MIT-blue?style=flat-square" alt="License" />
  <img src="https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey?style=flat-square" alt="Platform" />
</p>

<p align="center">
  <a href="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/ci-cd.yml">
    <img src="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/ci-cd.yml/badge.svg" alt="CI/CD Pipeline" />
  </a>
  <a href="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/code-coverage.yml">
    <img src="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/code-coverage.yml/badge.svg" alt="Code Coverage" />
  </a>
  <a href="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/code-formatting.yml">
    <img src="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/code-formatting.yml/badge.svg" alt="Code Formatting" />
  </a>
</p>

<p align="center">
  <a href="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/codeql.yml">
    <img src="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/codeql.yml/badge.svg" alt="CodeQL Security Analysis" />
  </a>
  <a href="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/documentation.yml">
    <img src="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/documentation.yml/badge.svg" alt="Documentation" />
  </a>
  <a href="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/security.yml">
    <img src="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/security.yml/badge.svg" alt="Security Scan" />
  </a>
</p>

<p align="center">
  <a href="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/static-analysis.yml">
    <img src="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/static-analysis.yml/badge.svg" alt="Static Code Analysis" />
  </a>
  <a href="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/validate.yml">
    <img src="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/validate.yml/badge.svg" alt="Validate Workflow" />
  </a>
  <a href="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/dependabot/dependabot-updates">
    <img src="https://github.com/wieslawsoltes/DxfToCSharp/actions/workflows/dependabot/dependabot-updates/badge.svg" alt="Dependabot Updates" />
  </a>
</p>

## 📦 NuGet Packages

| Package | Version | Downloads |
|---------|---------|----------|
| **DxfToCSharp.Core** | [![NuGet](https://img.shields.io/nuget/v/DxfToCSharp.Core?style=flat-square)](https://www.nuget.org/packages/DxfToCSharp.Core/) | [![NuGet Downloads](https://img.shields.io/nuget/dt/DxfToCSharp.Core?style=flat-square)](https://www.nuget.org/packages/DxfToCSharp.Core/) |
| **DxfToCSharp.Tool** | [![NuGet](https://img.shields.io/nuget/v/DxfToCSharp.Tool?style=flat-square)](https://www.nuget.org/packages/DxfToCSharp.Tool/) | [![NuGet Downloads](https://img.shields.io/nuget/dt/DxfToCSharp.Tool?style=flat-square)](https://www.nuget.org/packages/DxfToCSharp.Tool/) |
| **DxfToCSharp.Compilation** | [![NuGet](https://img.shields.io/nuget/v/DxfToCSharp.Compilation?style=flat-square)](https://www.nuget.org/packages/DxfToCSharp.Compilation/) | [![NuGet Downloads](https://img.shields.io/nuget/dt/DxfToCSharp.Compilation?style=flat-square)](https://www.nuget.org/packages/DxfToCSharp.Compilation/) |

## 🚀 Overview

**DxfToCSharp** is a powerful .NET library and toolset for generating C# code from DXF (Drawing Exchange Format) files. Built on top of the robust [netDxf](https://github.com/haplokuon/netDxf) library, it provides seamless conversion of CAD drawings into executable C# code.

### ✨ Key Features

- 🎯 **High-Performance Code Generation** - Convert DXF files to clean, readable C# code
- 🔧 **Multiple Interfaces** - GUI application, command-line tool, and programmatic API
- 📐 **Comprehensive DXF Support** - Supports all major DXF entities and drawing elements
- 🎨 **Modern UI** - Beautiful Avalonia-based desktop application
- ⚡ **Cross-Platform** - Runs on Windows, macOS, and Linux
- 🧪 **Well-Tested** - Comprehensive test suite with high code coverage

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

## 🔧 Dependencies

| Package | Version | Purpose |
|---------|---------|----------|
| [netDxf.netstandard](https://www.nuget.org/packages/netDxf.netstandard/) | 3.0.1 | DXF file parsing and manipulation |
| [Microsoft.CodeAnalysis.CSharp](https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp/) | 4.14.0 | C# code analysis and compilation |
| [Avalonia](https://www.nuget.org/packages/Avalonia/) | 11.3.4 | Cross-platform UI framework |
| [System.CommandLine](https://www.nuget.org/packages/System.CommandLine/) | 2.0.0-beta4 | Command-line parsing |

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](.github/CONTRIBUTING.md) for details.

### Development Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/wieslawsoltes/DxfToCSharp.git
   cd DxfToCSharp
   ```

2. **Build the solution**
   ```bash
   dotnet build
   ```

3. **Run tests**
   ```bash
   dotnet test
   ```

## 📄 License

This project is licensed under the **MIT**.

See the [LICENSE](LICENSE) file for the complete license text.

## 👨‍💻 Author

**Wiesław Šoltés**
- GitHub: [@wieslawsoltes](https://github.com/wieslawsoltes)
- Website: [GitHub Profile](https://github.com/wieslawsoltes)

## 🙏 Acknowledgments

- [netDxf](https://github.com/haplokuon/netDxf) - Excellent DXF library by Daniel Carvajal
- [Avalonia](https://avaloniaui.net/) - Amazing cross-platform UI framework
- [Microsoft Roslyn](https://github.com/dotnet/roslyn) - Powerful C# compiler platform

## 📊 Project Stats

![GitHub stars](https://img.shields.io/github/stars/wieslawsoltes/DxfToCSharp?style=social)
![GitHub forks](https://img.shields.io/github/forks/wieslawsoltes/DxfToCSharp?style=social)
![GitHub issues](https://img.shields.io/github/issues/wieslawsoltes/DxfToCSharp)
![GitHub pull requests](https://img.shields.io/github/issues-pr/wieslawsoltes/DxfToCSharp)

---

<p align="center">
  Made with ❤️ by <a href="https://github.com/wieslawsoltes">Wiesław Šoltés</a>
</p>
