# Contributing to DxfToCSharp

We welcome contributions to DxfToCSharp! Here's how you can help:

## Development Setup

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/DxfToCSharp.git
   cd DxfToCSharp
   ```
3. **Install .NET 9 SDK** from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
4. **Initialize submodules** (for netDxf dependency):
   ```bash
   git submodule update --init --recursive
   ```
5. **Restore dependencies**:
   ```bash
   dotnet restore
   ```
6. **Build the solution**:
   ```bash
   dotnet build
   ```

## Project Structure

The solution consists of several projects:

- **DxfToCSharp.Core** - Core library with code generation logic
- **DxfToCSharp.Compilation** - Code compilation services
- **DxfToCSharp.Tool** - Command-line tool
- **DxfToCSharp** - GUI application (Avalonia UI)
- **DxfToCSharp.Tests** - Unit tests

## Running Tests

Run all tests:
```bash
dotnet test
```

Run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

Run specific test project:
```bash
dotnet test DxfToCSharp.Tests/
```

## Code Style

We use EditorConfig and dotnet format for consistent code style:

```bash
# Check formatting
dotnet format --verify-no-changes

# Apply formatting
dotnet format
```

### Coding Guidelines

- Use C# naming conventions
- Add XML documentation for public APIs
- Write unit tests for new functionality
- Keep methods focused and small
- Use meaningful variable and method names

## Making Changes

### 1. Create a Feature Branch

```bash
git checkout -b feature/your-feature-name
```

### 2. Make Your Changes

- Write clean, well-documented code
- Add tests for new functionality
- Update documentation if needed
- Follow existing code patterns

### 3. Test Your Changes

```bash
# Run all tests
dotnet test

# Test the CLI tool
dotnet run --project DxfToCSharp.Tool -- input.dxf output.cs

# Test the GUI (if applicable)
dotnet run --project DxfToCSharp
```

### 4. Commit Your Changes

```bash
git add .
git commit -m "Add feature: description of your changes"
```

Use clear, descriptive commit messages:
- `Add feature: support for spline entities`
- `Fix bug: handle empty DXF files gracefully`
- `Update docs: add examples for batch processing`

### 5. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Then create a pull request on GitHub with:
- Clear description of changes
- Reference to any related issues
- Screenshots (if UI changes)
- Test results

## Types of Contributions

### Bug Fixes

- Report bugs via GitHub Issues
- Include minimal reproduction steps
- Provide sample DXF files if relevant
- Fix bugs with accompanying tests

### New Features

- Discuss major features in GitHub Issues first
- Add support for new DXF entities
- Improve code generation options
- Enhance the GUI or CLI tools

### Documentation

- Improve API documentation
- Add more examples
- Fix typos and clarify instructions
- Update README files

### Testing

- Add test cases for edge cases
- Improve test coverage
- Add integration tests
- Test with various DXF files

## Development Workflow

### Adding New Entity Support

1. **Identify the entity** in netDxf library
2. **Add generation option** in `DxfCodeGenerationOptions`
3. **Implement generator method** in `DxfCodeGenerator`
4. **Add unit tests** in `DxfToCSharp.Tests`
5. **Update documentation** with examples

### Example: Adding Circle Entity Support

```csharp
// 1. Add option in DxfCodeGenerationOptions.cs
public bool GenerateCircleEntities { get; set; } = false;

// 2. Add method in DxfCodeGenerator.cs
private void GenerateCircleEntities(StringBuilder sb, DxfDocument document)
{
    if (!options.GenerateCircleEntities) return;
    
    foreach (var circle in document.Circles)
    {
        // Generate C# code for circle
    }
}

// 3. Add test in CircleEntityTests.cs
[Fact]
public void GenerateCircleEntities_ShouldCreateValidCode()
{
    // Test implementation
}
```

## Release Process

Maintainers handle releases, but contributors should:

1. **Update version numbers** in project files
2. **Update CHANGELOG.md** with new features/fixes
3. **Ensure all tests pass** on all platforms
4. **Update documentation** for new features

## Getting Help

- **GitHub Issues** - For bugs and feature requests
- **GitHub Discussions** - For questions and general discussion
- **Code Review** - Maintainers will review pull requests

## Code of Conduct

Please be respectful and constructive in all interactions. We want to maintain a welcoming environment for all contributors.

## Recognition

Contributors are recognized in:
- Git commit history
- Release notes
- Contributors section (if added)

Thank you for contributing to DxfToCSharp! 🎉