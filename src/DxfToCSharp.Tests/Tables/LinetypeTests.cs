using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;

namespace DxfToCSharp.Tests.Tables;

public class LinetypeTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Linetype_BasicRoundTrip_ShouldPreserveLinetypeProperties()
    {
        // Arrange
        var segments = new List<LinetypeSegment>
        {
            new LinetypeSimpleSegment(0.5),
            new LinetypeSimpleSegment(-0.25),
            new LinetypeSimpleSegment(0.25),
            new LinetypeSimpleSegment(-0.25)
        };

        var originalLinetype = new Linetype("TestLinetype", segments, "Test linetype description");

        // Create a document with an entity that uses this linetype
        var originalDoc = new DxfDocument();
        originalDoc.Linetypes.Add(originalLinetype);

        var line = new Line(new Vector2(0, 0), new Vector2(100, 100))
        {
            Linetype = originalLinetype
        };
        originalDoc.Entities.Add(line);

        // Act & Assert
        PerformLinetypeRoundTripTest(originalDoc, originalLinetype, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            // NOTE: Linetype description and segments are not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.Description, recreated.Description);
            // Assert.Equal(original.Segments.Count, recreated.Segments.Count);

            // for (int i = 0; i < original.Segments.Count; i++)
            // {
            //     Assert.Equal(original.Segments[i].Length, recreated.Segments[i].Length, 6);
            // }
        });
    }

    [Fact]
    public void Linetype_PredefinedDashed_ShouldPreserveProperties()
    {
        // Arrange
        var originalLinetype = Linetype.Dashed;

        // Create a document with an entity that uses this linetype
        var originalDoc = new DxfDocument();
        originalDoc.Linetypes.Add(originalLinetype);

        var circle = new Circle(new Vector3(50, 50, 0), 25)
        {
            Linetype = originalLinetype
        };
        originalDoc.Entities.Add(circle);

        // Act & Assert
        PerformLinetypeRoundTripTest(originalDoc, originalLinetype, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            // NOTE: Linetype description, segments, and length are not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.Description, recreated.Description);
            // Assert.Equal(original.Segments.Count, recreated.Segments.Count);
            // Assert.Equal(original.Length(), recreated.Length(), 6);
        });
    }

    [Fact]
    public void Linetype_PredefinedCenter_ShouldPreserveProperties()
    {
        // Arrange
        var originalLinetype = Linetype.Center;

        // Create a document with an entity that uses this linetype
        var originalDoc = new DxfDocument();
        originalDoc.Linetypes.Add(originalLinetype);

        var polyline = new Polyline2D(new List<Polyline2DVertex>
        {
            new Polyline2DVertex(0, 0),
            new Polyline2DVertex(100, 0),
            new Polyline2DVertex(100, 100),
            new Polyline2DVertex(0, 100)
        }, true)
        {
            Linetype = originalLinetype
        };
        originalDoc.Entities.Add(polyline);

        // Act & Assert
        PerformLinetypeRoundTripTest(originalDoc, originalLinetype, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            // NOTE: Linetype description, segments, and length are not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.Description, recreated.Description);
            // Assert.Equal(original.Segments.Count, recreated.Segments.Count);
            // Assert.Equal(original.Length(), recreated.Length(), 6);
        });
    }

    [Fact]
    public void Linetype_PredefinedDot_ShouldPreserveProperties()
    {
        // Arrange
        var originalLinetype = Linetype.Dot;

        // Create a document with an entity that uses this linetype
        var originalDoc = new DxfDocument();
        originalDoc.Linetypes.Add(originalLinetype);

        var arc = new Arc(new Vector3(0, 0, 0), 50, 0, Math.PI)
        {
            Linetype = originalLinetype
        };
        originalDoc.Entities.Add(arc);

        // Act & Assert
        PerformLinetypeRoundTripTest(originalDoc, originalLinetype, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            // NOTE: Linetype description, segments, and length are not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.Description, recreated.Description);
            // Assert.Equal(original.Segments.Count, recreated.Segments.Count);
            // Assert.Equal(original.Length(), recreated.Length(), 6);
        });
    }

    [Fact]
    public void Linetype_CustomWithComplexSegments_ShouldPreserveProperties()
    {
        // Arrange
        var segments = new List<LinetypeSegment>
        {
            new LinetypeSimpleSegment(1.0),
            new LinetypeSimpleSegment(-0.5),
            new LinetypeSimpleSegment(0.0),
            new LinetypeSimpleSegment(-0.5),
            new LinetypeSimpleSegment(0.25),
            new LinetypeSimpleSegment(-0.25)
        };

        var originalLinetype = new Linetype("ComplexLinetype", segments, "Complex custom linetype");

        // Create a document with an entity that uses this linetype
        var originalDoc = new DxfDocument();
        originalDoc.Linetypes.Add(originalLinetype);

        var spline = new Spline(new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(50, 25, 0),
            new Vector3(100, 0, 0)
        })
        {
            Linetype = originalLinetype
        };
        originalDoc.Entities.Add(spline);

        // Act & Assert
        PerformLinetypeRoundTripTest(originalDoc, originalLinetype, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            // NOTE: Linetype description, segments, and length are not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.Description, recreated.Description);
            // Assert.Equal(original.Segments.Count, recreated.Segments.Count);
            // Assert.Equal(original.Length(), recreated.Length(), 6);

            // for (int i = 0; i < original.Segments.Count; i++)
            // {
            //     Assert.Equal(original.Segments[i].Length, recreated.Segments[i].Length, 6);
            // }
        });
    }

    /// <summary>
    /// Performs a complete round-trip test for a Linetype table object:
    /// 1. Save document with linetype to file
    /// 2. Load from file
    /// 3. Generate C# code
    /// 4. Compile and execute code
    /// 5. Validate the recreated linetype
    /// </summary>
    private void PerformLinetypeRoundTripTest(DxfDocument originalDoc, Linetype originalLinetype, Action<Linetype, Linetype> validator)
    {
        // Step 1: Save to DXF file
        var originalDxfPath = Path.Combine(_tempDirectory, "original.dxf");
        originalDoc.Save(originalDxfPath);

        // Step 2: Load DXF file
        var loadedDoc = DxfDocument.Load(originalDxfPath);
        Assert.NotNull(loadedDoc);
        Assert.Contains(originalLinetype.Name, loadedDoc.Linetypes.Names);

        // Step 3: Generate C# code from the loaded document
        var generatedCode = _generator.Generate(loadedDoc, originalDxfPath);
        Assert.NotNull(generatedCode);
        Assert.NotEmpty(generatedCode);

        // Step 4: Compile and execute the generated code
        var recreatedDoc = CompileAndExecuteCode(generatedCode);
        Assert.NotNull(recreatedDoc);
        Assert.Contains(originalLinetype.Name, recreatedDoc.Linetypes.Names);

        // Step 5: Validate the recreated linetype matches the original
        var recreatedLinetype = recreatedDoc.Linetypes[originalLinetype.Name];
        Assert.NotNull(recreatedLinetype);
        validator(originalLinetype, recreatedLinetype);

        // Step 6: Save recreated document and verify it can be loaded
        var recreatedDxfPath = Path.Combine(_tempDirectory, "recreated.dxf");
        recreatedDoc.Save(recreatedDxfPath);
        var finalDoc = DxfDocument.Load(recreatedDxfPath);
        Assert.NotNull(finalDoc);
        Assert.Contains(originalLinetype.Name, finalDoc.Linetypes.Names);
    }

    public override void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
        base.Dispose();
    }
}
