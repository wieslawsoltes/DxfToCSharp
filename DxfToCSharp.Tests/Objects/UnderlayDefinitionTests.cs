using netDxf;
using netDxf.Objects;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Objects;

public class UnderlayDefinitionTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void UnderlayPdfDefinition_BasicProperties_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var pdfDef = new UnderlayPdfDefinition("TestPdf", "test.pdf");
        originalDoc.UnderlayPdfDefinitions.Add(pdfDef);
        
        // Add at least one entity to make the document valid
        originalDoc.Entities.Add(new netDxf.Entities.Line(netDxf.Vector3.Zero, new netDxf.Vector3(1, 1, 0)));

        // Act & Assert
        PerformObjectRoundTripTest(originalDoc, pdfDef, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
            Assert.Equal(original.File, loaded.File);
        });
    }

    [Fact]
    public void UnderlayDwfDefinition_BasicProperties_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var dwfDef = new UnderlayDwfDefinition("TestDwf", "test.dwf");
        originalDoc.UnderlayDwfDefinitions.Add(dwfDef);
        
        // Add at least one entity to make the document valid
        originalDoc.Entities.Add(new netDxf.Entities.Line(netDxf.Vector3.Zero, new netDxf.Vector3(1, 1, 0)));

        // Act & Assert
        PerformObjectRoundTripTest(originalDoc, dwfDef, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
            Assert.Equal(original.File, loaded.File);
        });
    }

    [Fact]
    public void UnderlayDgnDefinition_BasicProperties_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var dgnDef = new UnderlayDgnDefinition("TestDgn", "test.dgn");
        originalDoc.UnderlayDgnDefinitions.Add(dgnDef);
        
        // Add at least one entity to make the document valid
        originalDoc.Entities.Add(new netDxf.Entities.Line(netDxf.Vector3.Zero, new netDxf.Vector3(1, 1, 0)));

        // Act & Assert
        PerformObjectRoundTripTest(originalDoc, dgnDef, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
            Assert.Equal(original.File, loaded.File);
        });
    }

    [Fact]
    public void UnderlayDefinitions_MultipleTypes_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var pdfDef = new UnderlayPdfDefinition("PdfDoc", "document.pdf");
        var dwfDef = new UnderlayDwfDefinition("DwfDrawing", "drawing.dwf");
        var dgnDef = new UnderlayDgnDefinition("DgnDesign", "design.dgn");
        
        originalDoc.UnderlayPdfDefinitions.Add(pdfDef);
        originalDoc.UnderlayDwfDefinitions.Add(dwfDef);
        originalDoc.UnderlayDgnDefinitions.Add(dgnDef);
        
        // Add at least one entity to make the document valid
        originalDoc.Entities.Add(new netDxf.Entities.Line(netDxf.Vector3.Zero, new netDxf.Vector3(1, 1, 0)));

        // Act & Assert
        PerformMultipleUnderlayTypesRoundTripTest(originalDoc, pdfDef, dwfDef, dgnDef, (originalPdf, originalDwf, originalDgn, loadedDoc) =>
        {
            // Verify PDF definition
            var loadedPdf = loadedDoc.UnderlayPdfDefinitions.Items.FirstOrDefault(p => p.Name == originalPdf.Name);
            Assert.NotNull(loadedPdf);
            Assert.Equal(originalPdf.Name, loadedPdf.Name);
            Assert.Equal(originalPdf.File, loadedPdf.File);
            
            // Verify DWF definition
            var loadedDwf = loadedDoc.UnderlayDwfDefinitions.Items.FirstOrDefault(d => d.Name == originalDwf.Name);
            Assert.NotNull(loadedDwf);
            Assert.Equal(originalDwf.Name, loadedDwf.Name);
            Assert.Equal(originalDwf.File, loadedDwf.File);
            
            // Verify DGN definition
            var loadedDgn = loadedDoc.UnderlayDgnDefinitions.Items.FirstOrDefault(d => d.Name == originalDgn.Name);
            Assert.NotNull(loadedDgn);
            Assert.Equal(originalDgn.Name, loadedDgn.Name);
            Assert.Equal(originalDgn.File, loadedDgn.File);
        });
    }

    [Fact]
    public void UnderlayDefinition_WithSpecialCharacters_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var pdfDef = new UnderlayPdfDefinition("Pdf_Name-123", "path/to/file name-123.pdf");
        originalDoc.UnderlayPdfDefinitions.Add(pdfDef);
        
        // Add at least one entity to make the document valid
        originalDoc.Entities.Add(new netDxf.Entities.Line(netDxf.Vector3.Zero, new netDxf.Vector3(1, 1, 0)));

        // Act & Assert
        PerformObjectRoundTripTest(originalDoc, pdfDef, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
            Assert.Equal(original.File, loaded.File);
        });
    }

    private void PerformObjectRoundTripTest<T>(DxfDocument originalDoc, T originalObject, Action<T, T> validator) where T : UnderlayDefinition
    {
        // Step 1: Save original document to DXF file
        var originalDxfPath = Path.Combine(_tempDirectory, "original.dxf");
        originalDoc.Save(originalDxfPath);

        // Step 2: Load DXF file
        var loadedDoc = DxfDocument.Load(originalDxfPath);
        Assert.NotNull(loadedDoc);

        // Step 3: Find the corresponding object in the loaded document
        T? loadedObject = null;
        if (typeof(T) == typeof(UnderlayPdfDefinition))
        {
            var originalPdf = originalObject as UnderlayPdfDefinition;
            loadedObject = loadedDoc.UnderlayPdfDefinitions.Items.FirstOrDefault(p => p.Name == originalPdf?.Name) as T;
        }
        else if (typeof(T) == typeof(UnderlayDwfDefinition))
        {
            var originalDwf = originalObject as UnderlayDwfDefinition;
            loadedObject = loadedDoc.UnderlayDwfDefinitions.Items.FirstOrDefault(d => d.Name == originalDwf?.Name) as T;
        }
        else if (typeof(T) == typeof(UnderlayDgnDefinition))
        {
            var originalDgn = originalObject as UnderlayDgnDefinition;
            loadedObject = loadedDoc.UnderlayDgnDefinitions.Items.FirstOrDefault(d => d.Name == originalDgn?.Name) as T;
        }
        
        Assert.NotNull(loadedObject);

        // Step 4: Generate C# code from the loaded document
        var generatedCode = _generator.Generate(loadedDoc, originalDxfPath);
        Assert.NotNull(generatedCode);
        Assert.NotEmpty(generatedCode);

        // Step 5: Compile and execute the generated code
        var recreatedDoc = CompileAndExecuteCode(generatedCode);
        Assert.NotNull(recreatedDoc);

        // Step 6: Find the corresponding object in the recreated document
        T? recreatedObject = null;
        if (typeof(T) == typeof(UnderlayPdfDefinition))
        {
            var originalPdf = originalObject as UnderlayPdfDefinition;
            recreatedObject = recreatedDoc.UnderlayPdfDefinitions.Items.FirstOrDefault(p => p.Name == originalPdf?.Name) as T;
        }
        else if (typeof(T) == typeof(UnderlayDwfDefinition))
        {
            var originalDwf = originalObject as UnderlayDwfDefinition;
            recreatedObject = recreatedDoc.UnderlayDwfDefinitions.Items.FirstOrDefault(d => d.Name == originalDwf?.Name) as T;
        }
        else if (typeof(T) == typeof(UnderlayDgnDefinition))
        {
            var originalDgn = originalObject as UnderlayDgnDefinition;
            recreatedObject = recreatedDoc.UnderlayDgnDefinitions.Items.FirstOrDefault(d => d.Name == originalDgn?.Name) as T;
        }
        
        Assert.NotNull(recreatedObject);

        // Step 7: Validate the recreated object matches the original
        validator(originalObject, recreatedObject);

        // Step 8: Save recreated document and verify it can be loaded
        var recreatedDxfPath = Path.Combine(_tempDirectory, "recreated.dxf");
        recreatedDoc.Save(recreatedDxfPath);
        var finalDoc = DxfDocument.Load(recreatedDxfPath);
        Assert.NotNull(finalDoc);
    }

    private void PerformMultipleUnderlayTypesRoundTripTest(
        DxfDocument originalDoc, 
        UnderlayPdfDefinition originalPdf, 
        UnderlayDwfDefinition originalDwf, 
        UnderlayDgnDefinition originalDgn,
        Action<UnderlayPdfDefinition, UnderlayDwfDefinition, UnderlayDgnDefinition, DxfDocument> validator)
    {
        // Step 1: Save original document to DXF file
        var originalDxfPath = Path.Combine(_tempDirectory, "original.dxf");
        originalDoc.Save(originalDxfPath);

        // Step 2: Load DXF file
        var loadedDoc = DxfDocument.Load(originalDxfPath);
        Assert.NotNull(loadedDoc);

        // Step 3: Generate C# code from the loaded document
        var generatedCode = _generator.Generate(loadedDoc, originalDxfPath);
        Assert.NotNull(generatedCode);
        Assert.NotEmpty(generatedCode);

        // Step 4: Compile and execute the generated code
        var recreatedDoc = CompileAndExecuteCode(generatedCode);
        Assert.NotNull(recreatedDoc);

        // Step 5: Validate the recreated objects match the originals
        validator(originalPdf, originalDwf, originalDgn, recreatedDoc);

        // Step 6: Save recreated document and verify it can be loaded
        var recreatedDxfPath = Path.Combine(_tempDirectory, "recreated.dxf");
        recreatedDoc.Save(recreatedDxfPath);
        var finalDoc = DxfDocument.Load(recreatedDxfPath);
        Assert.NotNull(finalDoc);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}