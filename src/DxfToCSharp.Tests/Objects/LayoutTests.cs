using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Collections;
using netDxf.Objects;

namespace DxfToCSharp.Tests.Objects;

public class LayoutTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Layout_BasicProperties_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var layout = new Layout("TestLayout");
        originalDoc.Layouts.Add(layout);

        // Add at least one entity to make the document valid
        originalDoc.Entities.Add(new netDxf.Entities.Line(netDxf.Vector3.Zero, new netDxf.Vector3(1, 1, 0)));

        // Act & Assert
        PerformObjectRoundTripTest(originalDoc, layout, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
            // Note: Other layout properties may not be preserved during DXF round-trip
            // as they depend on the specific DXF implementation and version
        });
    }

    [Fact]
    public void Layout_WithSpecialName_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var layout = new Layout("Layout_Test-123");
        originalDoc.Layouts.Add(layout);

        // Add at least one entity to make the document valid
        originalDoc.Entities.Add(new netDxf.Entities.Line(netDxf.Vector3.Zero, new netDxf.Vector3(1, 1, 0)));

        // Act & Assert
        PerformObjectRoundTripTest(originalDoc, layout, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
        });
    }

    [Fact]
    public void Layout_WithCustomProperties_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var layout = new Layout("CustomLayout")
        {
            TabOrder = 5,
            MinLimit = new netDxf.Vector2(-100, -100),
            MaxLimit = new netDxf.Vector2(100, 100),
            MinExtents = new netDxf.Vector3(-50, -50, 0),
            MaxExtents = new netDxf.Vector3(50, 50, 0),
            BasePoint = new netDxf.Vector3(10, 20, 0),
            Elevation = 5.0,
            UcsOrigin = new netDxf.Vector3(1, 2, 3),
            UcsXAxis = new netDxf.Vector3(1, 0, 0),
            UcsYAxis = new netDxf.Vector3(0, 1, 0)
            // Note: IsPaperSpace is read-only and determined by the layout type
        };
        originalDoc.Layouts.Add(layout);

        // Add at least one entity to make the document valid
        originalDoc.Entities.Add(new netDxf.Entities.Line(netDxf.Vector3.Zero, new netDxf.Vector3(1, 1, 0)));

        // Act & Assert
        PerformObjectRoundTripTest(originalDoc, layout, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
            Assert.Equal(original.TabOrder, loaded.TabOrder);
            Assert.Equal(original.MinLimit, loaded.MinLimit);
            Assert.Equal(original.MaxLimit, loaded.MaxLimit);
            Assert.Equal(original.MinExtents, loaded.MinExtents);
            Assert.Equal(original.MaxExtents, loaded.MaxExtents);
            Assert.Equal(original.BasePoint, loaded.BasePoint);
            Assert.Equal(original.Elevation, loaded.Elevation, 6);
            Assert.Equal(original.UcsOrigin, loaded.UcsOrigin);
            Assert.Equal(original.UcsXAxis, loaded.UcsXAxis);
            Assert.Equal(original.UcsYAxis, loaded.UcsYAxis);
            Assert.Equal(original.IsPaperSpace, loaded.IsPaperSpace);
        });
    }

    [Fact]
    public void Layout_MultipleLayouts_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var layout1 = new Layout("Layout1");
        var layout2 = new Layout("Layout2");
        var layout3 = new Layout("Layout3");

        originalDoc.Layouts.Add(layout1);
        originalDoc.Layouts.Add(layout2);
        originalDoc.Layouts.Add(layout3);

        // Add at least one entity to make the document valid
        originalDoc.Entities.Add(new netDxf.Entities.Line(netDxf.Vector3.Zero, new netDxf.Vector3(1, 1, 0)));

        // Act & Assert
        PerformMultipleObjectsRoundTripTest(originalDoc, new[] { layout1, layout2, layout3 }, (originals, loaded) =>
        {
            // Exclude the default Model layout from the count
            var customLayouts = loaded.Where(l => !string.Equals(l.Name, "Model", StringComparison.OrdinalIgnoreCase)).ToList();
            Assert.Equal(originals.Length, customLayouts.Count);

            foreach (var original in originals)
            {
                var loadedLayout = loaded.FirstOrDefault(l => l.Name == original.Name);
                Assert.NotNull(loadedLayout);
                Assert.Equal(original.Name, loadedLayout.Name);
            }
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
        if (typeof(T) == typeof(Layout))
        {
            var originalLayout = originalObject as Layout;
            loadedObject = loadedDoc.Layouts.FirstOrDefault(l => l.Name == originalLayout?.Name) as T;
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
        if (typeof(T) == typeof(Layout))
        {
            var originalLayout = originalObject as Layout;
            recreatedObject = recreatedDoc.Layouts.FirstOrDefault(l => l.Name == originalLayout?.Name) as T;
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

    private void PerformMultipleObjectsRoundTripTest(DxfDocument originalDoc, Layout[] originalObjects, Action<Layout[], Layouts> validator)
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
        validator(originalObjects, recreatedDoc.Layouts);

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
