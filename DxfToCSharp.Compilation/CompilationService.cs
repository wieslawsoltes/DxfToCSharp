using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using netDxf;

namespace DxfToCSharp.Compilation
{
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
            var tempDir = Path.Combine(Path.GetTempPath(), "DxfToCSharp");
            Directory.CreateDirectory(tempDir);
            var dllPath = Path.Combine(tempDir, assemblyName + ".dll");
            var pdbPath = Path.Combine(tempDir, assemblyName + ".pdb");

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
                .FirstOrDefault(t => t.IsClass && t.IsSealed && t.IsAbstract && // static class
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
                    var tempFile = Path.Combine(Path.GetTempPath(), "DxfToCSharp", "Generated_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".dxf");
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
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        /// <summary>
        /// Gets the necessary references for compilation, including System.Drawing.Primitives
        /// </summary>
        private static MetadataReference[] GetMetadataReferences()
        {
            var references = new List<MetadataReference>
            {
                // Core system assemblies
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location),
                
                // netDxf
                MetadataReference.CreateFromFile(typeof(DxfDocument).Assembly.Location)
            };

            // Add System.Runtime and System.Collections
            try
            {
                references.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location));
                references.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location));
            }
            catch
            {
                // Ignore if not available
            }

            // Add System.Drawing.Primitives reference for Color type
            try
            {
                references.Add(MetadataReference.CreateFromFile(Assembly.Load("System.Drawing.Primitives").Location));
            }
            catch
            {
                // Try alternative approach for System.Drawing.Primitives
                try
                {
                    var drawingAssembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == "System.Drawing.Primitives");
                    if (drawingAssembly != null && !string.IsNullOrEmpty(drawingAssembly.Location))
                    {
                        references.Add(MetadataReference.CreateFromFile(drawingAssembly.Location));
                    }
                }
                catch
                {
                    // Ignore if System.Drawing.Primitives is not available
                }
            }

            // Add reference to netstandard if available
            try
            {
                references.Add(MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location));
            }
            catch
            {
                // Ignore if netstandard is not available
            }

            // Add entry assembly if available (for Avalonia app)
            try
            {
                var appAsm = Assembly.GetEntryAssembly();
                if (appAsm != null && !string.IsNullOrEmpty(appAsm.Location))
                    references.Add(MetadataReference.CreateFromFile(appAsm.Location));
            }
            catch
            {
                // Ignore if not available
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
                                references.Add(MetadataReference.CreateFromFile(loaded.Location));
                            }
                        }
                    }
                }
                catch
                {
                    // Some dynamic assemblies don't have a location
                }
            }

            return references.ToArray();
        }
    }
}