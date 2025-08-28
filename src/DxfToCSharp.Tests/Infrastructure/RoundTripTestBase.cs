using DxfToCSharp.Compilation;
using DxfToCSharp.Core;
using netDxf;
using netDxf.Entities;

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
