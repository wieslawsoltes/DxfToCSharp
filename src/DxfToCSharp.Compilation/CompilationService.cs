using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using netDxf;

namespace DxfToCSharp.Compilation;

public class CompilationService
{
    public class CompilationResult
    {
        public bool Success { get; }
        public string? AssemblyPath { get; }
        public string Output { get; }
        public Assembly? Assembly { get; }

        public CompilationResult(bool success, string? assemblyPath, string output, Assembly? assembly = null)
        {
            Success = success;
            AssemblyPath = assemblyPath;
            Output = output;
            Assembly = assembly;
        }
    }

    /// <summary>
    /// Compiles C# source code to a DLL file on disk
    /// </summary>
    public CompilationResult CompileToFile(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Release);

        var references = GetMetadataReferences();

        var assemblyName = "GeneratedDxf_" + Guid.NewGuid().ToString("N");

        // Secure path construction to prevent path traversal attacks
        var baseTempPath = Path.GetTempPath();
        var tempDir = Path.Combine(baseTempPath, "DxfToCSharp");

        // Validate that the resulting path is still within the temp directory
        var normalizedTempDir = Path.GetFullPath(tempDir);
        var normalizedBasePath = Path.GetFullPath(baseTempPath);
        if (!normalizedTempDir.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid path detected - potential path traversal attempt");
        }

        Directory.CreateDirectory(tempDir);

        // Validate assembly name contains only safe characters
        if (assemblyName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException("Assembly name contains invalid characters");
        }

        var dllPath = Path.Combine(tempDir, assemblyName + ".dll");
        var pdbPath = Path.Combine(tempDir, assemblyName + ".pdb");

        // Validate that the resulting file paths are still within the temp directory
        var normalizedDllPath = Path.GetFullPath(dllPath);
        var normalizedPdbPath = Path.GetFullPath(pdbPath);
        if (!normalizedDllPath.StartsWith(normalizedTempDir, StringComparison.OrdinalIgnoreCase) ||
            !normalizedPdbPath.StartsWith(normalizedTempDir, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid file path detected - potential path traversal attempt");
        }

        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            references,
            compilationOptions);

        using var dllStream = new FileStream(dllPath, FileMode.Create, FileAccess.Write);
        using var pdbStream = new FileStream(pdbPath, FileMode.Create, FileAccess.Write);
        var emitResult = compilation.Emit(dllStream, pdbStream);

        if (!emitResult.Success)
        {
            var diagnostics = string.Join(Environment.NewLine,
                emitResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.ToString()));
            return new CompilationResult(false, null, diagnostics);
        }

        return new CompilationResult(true, dllPath, "Compilation succeeded.");
    }

    /// <summary>
    /// Compiles C# source code to memory and returns the loaded assembly
    /// </summary>
    public CompilationResult CompileToMemory(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create(
            "DynamicAssembly",
            new[] { syntaxTree },
            GetMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var errors = string.Join("\n", result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));
            return new CompilationResult(false, null, $"Compilation failed:\n{errors}\n\nGenerated code:\n{sourceCode}");
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());
        return new CompilationResult(true, null, "Compilation succeeded.", assembly);
    }

    /// <summary>
    /// Executes the Create method from a compiled assembly and returns a DxfDocument
    /// </summary>
    public DxfDocument? ExecuteCreateMethod(Assembly assembly)
    {
        // Find the first public static class with a static Create method that returns DxfDocument
        var type = assembly.GetTypes()
            .FirstOrDefault(t => t is { IsClass: true, IsSealed: true, IsAbstract: true } && // static class
                                 t.GetMethod("Create", BindingFlags.Public | BindingFlags.Static) != null);

        if (type == null)
            throw new InvalidOperationException("No static class with a 'Create' method found in compiled assembly.");

        var method = type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
        if (method == null)
            throw new InvalidOperationException($"Static method 'Create' not found on type '{type.Name}'.");

        var result = method.Invoke(null, null) as DxfDocument;
        return result;
    }

    /// <summary>
    /// Executes the Create method from a compiled assembly file and returns a DxfDocument
    /// </summary>
    public DxfDocument? ExecuteCreateMethod(string assemblyPath)
    {
        var assembly = Assembly.LoadFile(assemblyPath);
        return ExecuteCreateMethod(assembly);
    }

    /// <summary>
    /// Runs the Create method from an assembly file and saves the result to a DXF file
    /// </summary>
    public string RunCreateMethod(string assemblyPath)
    {
        try
        {
            var dxf = ExecuteCreateMethod(assemblyPath);
            if (dxf != null)
            {
                // Secure path construction to prevent path traversal attacks
                var baseTempPath = Path.GetTempPath();
                var fileName = "Generated_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".dxf";

                // Validate filename contains only safe characters
                if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    throw new ArgumentException("Generated filename contains invalid characters");
                }

                var tempDir = Path.Combine(baseTempPath, "DxfToCSharp");
                var tempFile = Path.Combine(tempDir, fileName);

                // Validate that the resulting paths are still within the temp directory
                var normalizedTempDir = Path.GetFullPath(tempDir);
                var normalizedBasePath = Path.GetFullPath(baseTempPath);
                var normalizedTempFile = Path.GetFullPath(tempFile);

                if (!normalizedTempDir.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase) ||
                    !normalizedTempFile.StartsWith(normalizedTempDir, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Invalid path detected - potential path traversal attempt");
                }

                Directory.CreateDirectory(Path.GetDirectoryName(tempFile)!);
                dxf.Save(tempFile);
                return "Executed successfully. Saved DXF to: " + tempFile;
            }
            return "Create method returned null.";
        }
        catch (TargetInvocationException tie)
        {
            return "Runtime error: " + tie.InnerException;
        }
        catch (TargetParameterCountException ex)
        {
            return "Method parameter error: " + ex.Message;
        }
        catch (ArgumentException ex)
        {
            return "Invalid argument: " + ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            return "Invalid operation: " + ex.Message;
        }
        catch (UnauthorizedAccessException ex)
        {
            return "Access denied: " + ex.Message;
        }
        catch (IOException ex)
        {
            return "I/O error: " + ex.Message;
        }
        catch (Exception ex)
        {
            return "Unexpected error: " + ex.Message;
        }
    }

    /// <summary>
    /// Gets the necessary references for compilation, including System.Drawing.Primitives
    /// </summary>
    private static MetadataReference[] GetMetadataReferences()
    {
        var references = new List<MetadataReference>();

        // Helper method to safely add metadata reference
        void TryAddReference(Assembly assembly)
        {
            if (assembly != null && !string.IsNullOrEmpty(assembly.Location) && File.Exists(assembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        // Core system assemblies
        TryAddReference(typeof(object).Assembly);
        TryAddReference(typeof(Console).Assembly);
        TryAddReference(typeof(List<>).Assembly);
        TryAddReference(typeof(Enumerable).Assembly);
        TryAddReference(typeof(Uri).Assembly);

        // netDxf
        TryAddReference(typeof(DxfDocument).Assembly);

        // Add System.Runtime and System.Collections
        try
        {
            TryAddReference(Assembly.Load("System.Runtime"));
            TryAddReference(Assembly.Load("System.Collections"));
        }
        catch (FileNotFoundException)
        {
            // Ignore if assembly not available
        }
        catch (FileLoadException)
        {
            // Ignore if assembly cannot be loaded
        }
        catch (BadImageFormatException)
        {
            // Ignore if assembly format is invalid
        }

        // Add System.Drawing.Primitives reference for Color type
        try
        {
            TryAddReference(Assembly.Load("System.Drawing.Primitives"));
        }
        catch (FileNotFoundException)
        {
            // Try alternative approach for System.Drawing.Primitives
            try
            {
                var drawingAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "System.Drawing.Primitives");
                if (drawingAssembly != null)
                {
                    TryAddReference(drawingAssembly);
                }
            }
            catch (FileNotFoundException)
            {
                // Ignore if System.Drawing.Primitives is not available
            }
            catch (FileLoadException)
            {
                // Ignore if System.Drawing.Primitives cannot be loaded
            }
        }
        catch (FileLoadException)
        {
            // Ignore if System.Drawing.Primitives cannot be loaded
        }
        catch (BadImageFormatException)
        {
            // Ignore if System.Drawing.Primitives format is invalid
        }

        // Add reference to netstandard if available
        try
        {
            TryAddReference(Assembly.Load("netstandard"));
        }
        catch (FileNotFoundException)
        {
            // Ignore if netstandard is not available
        }
        catch (FileLoadException)
        {
            // Ignore if netstandard cannot be loaded
        }
        catch (BadImageFormatException)
        {
            // Ignore if netstandard format is invalid
        }

        // Add entry assembly if available (for Avalonia app)
        try
        {
            var appAsm = Assembly.GetEntryAssembly();
            if (appAsm != null)
            {
                TryAddReference(appAsm);
            }
        }
        catch (FileNotFoundException)
        {
            // Ignore if entry assembly not available
        }
        catch (FileLoadException)
        {
            // Ignore if entry assembly cannot be loaded
        }
        catch (BadImageFormatException)
        {
            // Ignore if entry assembly format is invalid
        }

        // Add any other relevant loaded assemblies
        foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                if (!string.IsNullOrEmpty(loaded.Location) && File.Exists(loaded.Location))
                {
                    // Avoid duplicates
                    if (!references.Any(r => r is PortableExecutableReference per && per.FilePath == loaded.Location))
                    {
                        // Include specific assemblies that might be useful
                        var name = loaded.GetName().Name;
                        if (name is "System.Private.CoreLib" or "System.Runtime" or "netstandard")
                        {
                            TryAddReference(loaded);
                        }
                    }
                }
            }
            catch (NotSupportedException)
            {
                // Some dynamic assemblies don't have a location
            }
            catch (FileNotFoundException)
            {
                // Assembly file not found
            }
            catch (InvalidOperationException)
            {
                // Assembly operation not supported
            }
        }

        return references.ToArray();
    }
}
