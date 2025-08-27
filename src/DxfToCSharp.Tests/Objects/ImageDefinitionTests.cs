using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Objects;
using netDxf.Units;

namespace DxfToCSharp.Tests.Objects;

public class ImageDefinitionTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void ImageDefinition_BasicProperties_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var imageDef = new ImageDefinition("TestImage", "test_image.jpg", 800, 72.0, 600, 72.0, ImageResolutionUnits.Inches);
        originalDoc.ImageDefinitions.Add(imageDef);

        // Add at least one entity to make the document valid
        originalDoc.Entities.Add(new netDxf.Entities.Line(netDxf.Vector3.Zero, new netDxf.Vector3(1, 1, 0)));

        // Act & Assert
        PerformObjectRoundTripTest(originalDoc, imageDef, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
            Assert.Equal(original.File, loaded.File);
            Assert.Equal(original.Width, loaded.Width);
            Assert.Equal(original.Height, loaded.Height);
        });
    }

    [Fact]
    public void ImageDefinition_WithDifferentFormats_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var pngImage = new ImageDefinition("PngImage", "image.png", 1024, 72.0, 768, 72.0, ImageResolutionUnits.Inches);
        var bmpImage = new ImageDefinition("BmpImage", "image.bmp", 640, 72.0, 480, 72.0, ImageResolutionUnits.Inches);
        var tiffImage = new ImageDefinition("TiffImage", "image.tiff", 1920, 72.0, 1080, 72.0, ImageResolutionUnits.Inches);

        originalDoc.ImageDefinitions.Add(pngImage);
        originalDoc.ImageDefinitions.Add(bmpImage);
        originalDoc.ImageDefinitions.Add(tiffImage);

        // Add at least one entity to make the document valid
        originalDoc.Entities.Add(new netDxf.Entities.Line(netDxf.Vector3.Zero, new netDxf.Vector3(1, 1, 0)));

        // Act & Assert
        PerformMultipleObjectsRoundTripTest(originalDoc, new[] { pngImage, bmpImage, tiffImage }, (originals, loaded) =>
        {
            Assert.Equal(originals.Length, loaded.Count);

            foreach (var original in originals)
            {
                var loadedImageDef = loaded.FirstOrDefault(img => img.Name == original.Name);
                Assert.NotNull(loadedImageDef);
                Assert.Equal(original.Name, loadedImageDef.Name);
                Assert.Equal(original.File, loadedImageDef.File);
                Assert.Equal(original.Width, loadedImageDef.Width);
                Assert.Equal(original.Height, loadedImageDef.Height);
            }
        });
    }

    [Fact]
    public void ImageDefinition_WithSpecialCharacters_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var imageDef = new ImageDefinition("Image_Name-123", "path/to/image file-123.jpg", 500, 72.0, 300, 72.0, ImageResolutionUnits.Inches);
        originalDoc.ImageDefinitions.Add(imageDef);

        // Add at least one entity to make the document valid
        originalDoc.Entities.Add(new netDxf.Entities.Line(netDxf.Vector3.Zero, new netDxf.Vector3(1, 1, 0)));

        // Act & Assert
        PerformObjectRoundTripTest(originalDoc, imageDef, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
            Assert.Equal(original.File, loaded.File);
            Assert.Equal(original.Width, loaded.Width);
            Assert.Equal(original.Height, loaded.Height);
        });
    }

    [Fact]
    public void ImageDefinition_LargeDimensions_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var imageDef = new ImageDefinition("LargeImage", "large_image.tiff", 4096, 72.0, 4096, 72.0, ImageResolutionUnits.Inches);
        originalDoc.ImageDefinitions.Add(imageDef);

        // Add at least one entity to make the document valid
        originalDoc.Entities.Add(new netDxf.Entities.Line(netDxf.Vector3.Zero, new netDxf.Vector3(1, 1, 0)));

        // Act & Assert
        PerformObjectRoundTripTest(originalDoc, imageDef, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
            Assert.Equal(original.File, loaded.File);
            Assert.Equal(original.Width, loaded.Width);
            Assert.Equal(original.Height, loaded.Height);
        });
    }

    private void PerformObjectRoundTripTest<T>(DxfDocument originalDoc, T originalObject, Action<T, T> validator) where T : class
    {
        // Step 1: Save original document to DXF file
        var originalDxfPath = Path.Join(_tempDirectory, "original.dxf");
        originalDoc.Save(originalDxfPath);

        // Step 2: Load DXF file
        var loadedDoc = DxfDocument.Load(originalDxfPath);
        Assert.NotNull(loadedDoc);

        // Step 3: Find the corresponding object in the loaded document
        T? loadedObject = null;
        if (typeof(T) == typeof(ImageDefinition))
        {
            var originalImageDef = originalObject as ImageDefinition;
            loadedObject = loadedDoc.ImageDefinitions.Items.FirstOrDefault(img => img.Name == originalImageDef?.Name) as T;
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
        if (typeof(T) == typeof(ImageDefinition))
        {
            var originalImageDef = originalObject as ImageDefinition;
            recreatedObject = recreatedDoc.ImageDefinitions.Items.FirstOrDefault(img => img.Name == originalImageDef?.Name) as T;
        }

        Assert.NotNull(recreatedObject);

        // Step 7: Validate the recreated object matches the original
        validator(originalObject, recreatedObject);

        // Step 8: Save recreated document and verify it can be loaded
        var recreatedDxfPath = Path.Join(_tempDirectory, "recreated.dxf");
        recreatedDoc.Save(recreatedDxfPath);
        var finalDoc = DxfDocument.Load(recreatedDxfPath);
        Assert.NotNull(finalDoc);
    }

    private void PerformMultipleObjectsRoundTripTest(DxfDocument originalDoc, ImageDefinition[] originalObjects, Action<ImageDefinition[], ICollection<ImageDefinition>> validator)
    {
        // Step 1: Save original document to DXF file
        var originalDxfPath = Path.Join(_tempDirectory, "original.dxf");
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
        validator(originalObjects, recreatedDoc.ImageDefinitions.Items);

        // Step 6: Save recreated document and verify it can be loaded
        var recreatedDxfPath = Path.Join(_tempDirectory, "recreated.dxf");
        recreatedDoc.Save(recreatedDxfPath);
        var finalDoc = DxfDocument.Load(recreatedDxfPath);
        Assert.NotNull(finalDoc);
    }

    public override void Dispose()
    {
        // Cleanup if needed
        base.Dispose();
    }
}
