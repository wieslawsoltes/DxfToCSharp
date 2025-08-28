using DxfToCSharp.Compilation;
using DxfToCSharp.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.CSharp;
using netDxf;
using netDxf.Entities;
using System.Text;

namespace DxfToCSharp.Tests.Infrastructure;

public abstract class RoundTripTestBase
{
    protected readonly DxfCodeGenerator _generator;
    protected readonly CompilationService _compilationService;
    protected readonly string _tempDirectory;

    protected RoundTripTestBase()
    {
        _generator = new DxfCodeGenerator();
        _compilationService = new CompilationService();
        _tempDirectory = Path.Join(Path.GetTempPath(), "DxfToCSharpTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    /// <summary>
    /// Performs a complete round-trip test for a DXF entity:
    /// 1. Create DXF document with entity
    /// 2. Save to file
    /// 3. Load from file
    /// 4. Generate C# code
    /// 5. Compile and execute code
    /// 6. Validate the recreated document
    /// </summary>
    protected void PerformRoundTripTest<T>(T originalEntity, Action<T, T> validator) where T : EntityObject
    {
        PerformRoundTripTest(originalEntity, validator, null!);
    }

    /// <summary>
    /// Performs a complete round-trip test for a DXF entity with custom generation options:
    /// 1. Create DXF document with entity
    /// 2. Save to file
    /// 3. Load from file
    /// 4. Generate C# code
    /// 5. Compile and execute code
    /// 6. Validate the recreated document
    /// </summary>
    protected void PerformRoundTripTest<T>(T originalEntity, Action<T, T> validator, DxfCodeGenerationOptions options) where T : EntityObject
    {
        // Step 1: Create DXF document with the entity
        var originalDoc = new DxfDocument();
        originalDoc.Entities.Add(originalEntity);

        // Step 2: Save to DXF file
        var originalDxfPath = Path.Join(_tempDirectory, "original.dxf");
        originalDoc.Save(originalDxfPath);

        // Step 3: Load DXF file
        var loadedDoc = DxfDocument.Load(originalDxfPath);
        Assert.NotNull(loadedDoc);
        Assert.Single(loadedDoc.Entities.All);

        // Step 4: Generate C# code from the loaded document
        var generatedCode = options != null
            ? _generator.Generate(loadedDoc, originalDxfPath, null, options)
            : _generator.Generate(loadedDoc, originalDxfPath);
        Assert.NotNull(generatedCode);
        Assert.NotEmpty(generatedCode);

        // Validate code indentation and brace balance before compiling
        ValidateIndentationAndBraces(generatedCode);
        // Step 5: Compile and execute the generated code
        var recreatedDoc = CompileAndExecuteCode(generatedCode);
        Assert.NotNull(recreatedDoc);
        Assert.Single(recreatedDoc.Entities.All);

        // Step 6: Validate the recreated entity matches the original
        var recreatedEntity = recreatedDoc.Entities.All.First();
        Assert.IsType<T>(recreatedEntity);
        validator(originalEntity, (T)recreatedEntity);

        // Step 7: Save recreated document and verify it can be loaded
        var recreatedDxfPath = Path.Join(_tempDirectory, "recreated.dxf");
        recreatedDoc.Save(recreatedDxfPath);
        var finalDoc = DxfDocument.Load(recreatedDxfPath);
        Assert.NotNull(finalDoc);
        Assert.Single(finalDoc.Entities.All);
    }

    /// <summary>
    /// Performs a complete round-trip test for a DXF entity where the output type may differ from input:
    /// 1. Create DXF document with entity
    /// 2. Save to file
    /// 3. Load from file
    /// 4. Generate C# code
    /// 5. Compile and execute code
    /// 6. Validate the recreated document
    /// </summary>
    protected void PerformRoundTripTest<TInput, TOutput>(TInput originalEntity, Action<TInput, TOutput> validator)
        where TInput : EntityObject
        where TOutput : EntityObject
    {
        // Step 1: Create DXF document with the entity
        var originalDoc = new DxfDocument();
        originalDoc.Entities.Add(originalEntity);

        // Step 2: Save to DXF file
        var originalDxfPath = Path.Join(_tempDirectory, "original.dxf");
        originalDoc.Save(originalDxfPath);

        // Step 3: Load DXF file
        var loadedDoc = DxfDocument.Load(originalDxfPath);
        Assert.NotNull(loadedDoc);
        Assert.Single(loadedDoc.Entities.All);

        // Step 4: Generate C# code from the loaded document
        var generatedCode = _generator.Generate(loadedDoc, originalDxfPath);
        Assert.NotNull(generatedCode);
        Assert.NotEmpty(generatedCode);

        // Validate code indentation and brace balance before compiling
        ValidateIndentationAndBraces(generatedCode);

        // Step 5: Compile and execute the generated code
        var recreatedDoc = CompileAndExecuteCode(generatedCode);
        Assert.NotNull(recreatedDoc);
        Assert.Single(recreatedDoc.Entities.All);

        // Step 6: Validate the recreated entity matches the expected output type
        var recreatedEntity = recreatedDoc.Entities.All.First();
        Assert.IsType<TOutput>(recreatedEntity);
        validator(originalEntity, (TOutput)recreatedEntity);

        // Step 7: Save recreated document and verify it can be loaded
        var recreatedDxfPath = Path.Join(_tempDirectory, "recreated.dxf");
        recreatedDoc.Save(recreatedDxfPath);
        var finalDoc = DxfDocument.Load(recreatedDxfPath);
        Assert.NotNull(finalDoc);
        Assert.Single(finalDoc.Entities.All);
    }

    /// <summary>
    /// Compiles and executes the generated C# code to create a DxfDocument
    /// </summary>
    protected DxfDocument CompileAndExecuteCode(string code)
    {
        // Compile to memory using shared service
        var result = _compilationService.CompileToMemory(code);

        if (!result.Success)
        {
            File.WriteAllText("compilation_error.txt", $"COMPILATION ERROR:\n{result.Output}\n\nGENERATED CODE:\n{code}");
            throw new InvalidOperationException($"Compilation failed:\n{result.Output}\n\nGenerated code:\n{code}");
        }

        Assert.NotNull(result.Assembly);

        // Execute the Create method
        var document = _compilationService.ExecuteCreateMethod(result.Assembly);
        Assert.NotNull(document);

        return document;
    }

    /// <summary>
    /// Validates generated C# code formatting using Roslyn:
    /// - Parses with Roslyn and asserts no parse errors
    /// - Ensures indentation uses spaces only and multiples of 4
    /// - Relies on the parser for brace balance (no string hacks)
    /// </summary>
    protected void ValidateIndentationAndBraces(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var diagnostics = tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        if (diagnostics.Count > 0)
        {
            var diagText = string.Join("\n", diagnostics.Select(d => d.ToString()));
            Assert.Fail($"Roslyn parse errors detected in generated code:\n{diagText}");
        }

        var text = tree.GetText();
        var issues = new List<string>();
        foreach (var line in text.Lines)
        {
            var lineText = text.ToString(line.Span);
            if (string.IsNullOrWhiteSpace(lineText)) continue;

            // Extract leading indentation
            var indentLength = 0;
            var hasTab = false;
            for (int i = 0; i < lineText.Length; i++)
            {
                var ch = lineText[i];
                if (ch == ' ') { indentLength++; }
                else if (ch == '\t') { hasTab = true; indentLength++; }
                else break;
            }

            var trimmed = lineText.TrimStart();
            if (trimmed.StartsWith("///")) continue; // ignore XML doc comments

            if (hasTab)
            {
                issues.Add($"Line {line.LineNumber + 1}: Tabs found in indentation.");
            }

            if (indentLength % 4 != 0)
            {
                issues.Add($"Line {line.LineNumber + 1}: Indentation not multiple of 4 spaces (found {indentLength}).");
            }
        }

        if (issues.Count > 0)
        {
            var message = string.Join("\n", issues);
            Assert.Fail($"Generated code formatting validation failed:\n{message}");
        }

        // Compare generator output with Roslyn-normalized output to ensure formatting is canonical
        var root = tree.GetRoot();
        string formatted;
        try
        {
            formatted = DxfCodeGenerator.Format(root).ToFullString();
        }
        catch
        {
            formatted = root.NormalizeWhitespace().ToFullString();
        }

        static string NormalizeForCompare(string s)
        {
            // Normalize line endings and trim trailing whitespace per line
            var lines = s.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            var kept = new List<string>(lines.Length);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].TrimEnd();
                if (string.IsNullOrWhiteSpace(line)) continue; // ignore blank lines to allow harmless separation
                kept.Add(line);
            }
            return string.Join("\n", kept);
        }

        var originalNorm = NormalizeForCompare(code);
        var formattedNorm = NormalizeForCompare(formatted);

        if (!string.Equals(originalNorm, formattedNorm, StringComparison.Ordinal))
        {
            // Compute a concise diff context
            var oLines = originalNorm.Split('\n');
            var fLines = formattedNorm.Split('\n');
            var max = Math.Min(oLines.Length, fLines.Length);
            int diffAt = -1;
            for (int i = 0; i < max; i++)
            {
                if (!string.Equals(oLines[i], fLines[i], StringComparison.Ordinal)) { diffAt = i; break; }
            }
            if (diffAt == -1 && oLines.Length != fLines.Length)
            {
                diffAt = max;
            }
            var contextStart = Math.Max(0, diffAt - 3);
            var contextEnd = Math.Min(Math.Max(oLines.Length, fLines.Length) - 1, diffAt + 3);

            var expectedSnippet = new StringBuilder();
            var actualSnippet = new StringBuilder();
            for (int i = contextStart; i <= contextEnd; i++)
            {
                var o = i < oLines.Length ? oLines[i] : "<EOF>";
                var f = i < fLines.Length ? fLines[i] : "<EOF>";
                expectedSnippet.AppendLine($"O{ i+1,4 }: {o}");
                actualSnippet.AppendLine($"F{ i+1,4 }: {f}");
            }

            // Write artifacts for inspection
            try
            {
                var oPath = Path.Join(_tempDirectory, "generated_original.cs");
                var fPath = Path.Join(_tempDirectory, "generated_roslyn_formatted.cs");
                File.WriteAllText(oPath, originalNorm);
                File.WriteAllText(fPath, formattedNorm);
            }
            catch { /* ignore I/O errors */ }

            // Allow opting out of failure via env if needed

            Assert.Fail("Generated code differs from Roslyn-normalized formatting.\n" +
                        "Original (O) vs Formatted (F) around first difference:\n" +
                        expectedSnippet + "----\n" + actualSnippet);
        }
    }

    /// <summary>
    /// Helper method to compare double values with tolerance
    /// </summary>
    protected static void AssertDoubleEqual(double expected, double actual, double tolerance = 1e-10)
    {
        Assert.True(Math.Abs(expected - actual) < tolerance,
            $"Expected {expected}, but got {actual}. Difference: {Math.Abs(expected - actual)}");
    }

    /// <summary>
    /// Helper method to compare Vector3 values with tolerance
    /// </summary>
    protected static void AssertVector3Equal(Vector3 expected, Vector3 actual, double tolerance = 1e-10)
    {
        AssertDoubleEqual(expected.X, actual.X, tolerance);
        AssertDoubleEqual(expected.Y, actual.Y, tolerance);
        AssertDoubleEqual(expected.Z, actual.Z, tolerance);
    }

    /// <summary>
    /// Helper method to compare Vector2 values with tolerance
    /// </summary>
    protected static void AssertVector2Equal(Vector2 expected, Vector2 actual, double tolerance = 1e-10)
    {
        AssertDoubleEqual(expected.X, actual.X, tolerance);
        AssertDoubleEqual(expected.Y, actual.Y, tolerance);
    }

    /// <summary>
    /// Cleanup temporary files
    /// </summary>
    public virtual void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
        catch (IOException)
        {
            // Ignore cleanup errors - directory may be in use
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore cleanup errors - insufficient permissions
        }
    }
}
