using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using Xunit.Abstractions;

namespace DxfToCSharp.Tests.Tables;

public class UCSTests : RoundTripTestBase, IDisposable
{
    private readonly ITestOutputHelper _output;

    public UCSTests(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void UCS_BasicProperties_ShouldPreserveUCSProperties()
    {
        // Arrange
        var originalUCS = new UCS("TestUCS")
        {
            Origin = new Vector3(10, 20, 30)
        };
        originalUCS.SetAxis(Vector3.UnitX, Vector3.UnitY);

        // Create a document with entities that use this UCS
        var originalDoc = new DxfDocument();
        originalDoc.UCSs.Add(originalUCS);

        // Add some entities to make the UCS meaningful
        var line = new Line(new Vector3(0, 0, 0), new Vector3(100, 100, 0));
        originalDoc.Entities.Add(line);

        var circle = new Circle(new Vector3(50, 50, 0), 25);
        originalDoc.Entities.Add(circle);

        // Act & Assert
        PerformUCSRoundTripTest(originalDoc, originalUCS, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            Assert.Equal(original.Origin.X, recreated.Origin.X, 6);
            Assert.Equal(original.Origin.Y, recreated.Origin.Y, 6);
            Assert.Equal(original.Origin.Z, recreated.Origin.Z, 6);
            Assert.Equal(original.XAxis.X, recreated.XAxis.X, 6);
            Assert.Equal(original.XAxis.Y, recreated.XAxis.Y, 6);
            Assert.Equal(original.XAxis.Z, recreated.XAxis.Z, 6);
            Assert.Equal(original.YAxis.X, recreated.YAxis.X, 6);
            Assert.Equal(original.YAxis.Y, recreated.YAxis.Y, 6);
            Assert.Equal(original.YAxis.Z, recreated.YAxis.Z, 6);
            Assert.Equal(original.ZAxis.X, recreated.ZAxis.X, 6);
            Assert.Equal(original.ZAxis.Y, recreated.ZAxis.Y, 6);
            Assert.Equal(original.ZAxis.Z, recreated.ZAxis.Z, 6);
        });
    }

    [Fact]
    public void UCS_WithCustomAxes_ShouldPreserveAxisDirections()
    {
        // Arrange - Create a UCS with custom perpendicular axes
        var xDirection = Vector3.Normalize(new Vector3(1, 1, 0));
        var yDirection = Vector3.Normalize(new Vector3(-1, 1, 0));
        var originalUCS = new UCS("CustomAxesUCS", new Vector3(5, 10, 15), xDirection, yDirection);

        // Create a document with entities that use this UCS
        var originalDoc = new DxfDocument();
        originalDoc.UCSs.Add(originalUCS);

        // Add entities to demonstrate the UCS usage
        var polyline = new Polyline2D(new[]
        {
            new Vector2(0, 0),
            new Vector2(50, 0),
            new Vector2(50, 50),
            new Vector2(0, 50)
        }, true);
        originalDoc.Entities.Add(polyline);

        // Act & Assert
        PerformUCSRoundTripTest(originalDoc, originalUCS, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            Assert.Equal(original.Origin.X, recreated.Origin.X, 6);
            Assert.Equal(original.Origin.Y, recreated.Origin.Y, 6);
            Assert.Equal(original.Origin.Z, recreated.Origin.Z, 6);
            Assert.Equal(original.XAxis.X, recreated.XAxis.X, 6);
            Assert.Equal(original.XAxis.Y, recreated.XAxis.Y, 6);
            Assert.Equal(original.XAxis.Z, recreated.XAxis.Z, 6);
            Assert.Equal(original.YAxis.X, recreated.YAxis.X, 6);
            Assert.Equal(original.YAxis.Y, recreated.YAxis.Y, 6);
            Assert.Equal(original.YAxis.Z, recreated.YAxis.Z, 6);
            Assert.Equal(original.ZAxis.X, recreated.ZAxis.X, 6);
            Assert.Equal(original.ZAxis.Y, recreated.ZAxis.Y, 6);
            Assert.Equal(original.ZAxis.Z, recreated.ZAxis.Z, 6);
        });
    }

    [Fact]
    public void UCS_FromNormal_ShouldPreserveNormalBasedUCS()
    {
        // Arrange - Create a UCS from a normal vector
        var normal = new Vector3(0, 0, 1); // Z-up normal
        var originalUCS = UCS.FromNormal("NormalUCS", new Vector3(100, 200, 300), normal);

        // Create a document with entities that use this UCS
        var originalDoc = new DxfDocument();
        originalDoc.UCSs.Add(originalUCS);

        // Add a 3D face to demonstrate the UCS
        var face3d = new Face3D(
            new Vector3(0, 0, 0),
            new Vector3(100, 0, 0),
            new Vector3(100, 100, 0),
            new Vector3(0, 100, 0)
        );
        originalDoc.Entities.Add(face3d);

        // Act & Assert
        PerformUCSRoundTripTest(originalDoc, originalUCS, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            Assert.Equal(original.Origin.X, recreated.Origin.X, 6);
            Assert.Equal(original.Origin.Y, recreated.Origin.Y, 6);
            Assert.Equal(original.Origin.Z, recreated.Origin.Z, 6);
            Assert.Equal(original.XAxis.X, recreated.XAxis.X, 6);
            Assert.Equal(original.XAxis.Y, recreated.XAxis.Y, 6);
            Assert.Equal(original.XAxis.Z, recreated.XAxis.Z, 6);
            Assert.Equal(original.YAxis.X, recreated.YAxis.X, 6);
            Assert.Equal(original.YAxis.Y, recreated.YAxis.Y, 6);
            Assert.Equal(original.YAxis.Z, recreated.YAxis.Z, 6);
            Assert.Equal(original.ZAxis.X, recreated.ZAxis.X, 6);
            Assert.Equal(original.ZAxis.Y, recreated.ZAxis.Y, 6);
            Assert.Equal(original.ZAxis.Z, recreated.ZAxis.Z, 6);
        });
    }

    [Fact]
    public void UCS_FromNormalWithRotation_ShouldPreserveRotatedUCS()
    {
        // Arrange - Create a UCS from a normal vector with rotation
        var normal = Vector3.Normalize(new Vector3(1, 1, 1)); // Diagonal normal
        var rotation = Math.PI / 4; // 45 degrees
        var originalUCS = UCS.FromNormal("RotatedUCS", new Vector3(-50, -100, 50), normal, rotation);

        // Create a document with entities that use this UCS
        var originalDoc = new DxfDocument();
        originalDoc.UCSs.Add(originalUCS);

        // Add a text entity to demonstrate the UCS
        var text = new Text("UCS Test", new Vector3(0, 0, 0), 5.0)
        {
            Color = AciColor.Red
        };
        originalDoc.Entities.Add(text);

        // Act & Assert
        PerformUCSRoundTripTest(originalDoc, originalUCS, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            Assert.Equal(original.Origin.X, recreated.Origin.X, 6);
            Assert.Equal(original.Origin.Y, recreated.Origin.Y, 6);
            Assert.Equal(original.Origin.Z, recreated.Origin.Z, 6);
            Assert.Equal(original.XAxis.X, recreated.XAxis.X, 6);
            Assert.Equal(original.XAxis.Y, recreated.XAxis.Y, 6);
            Assert.Equal(original.XAxis.Z, recreated.XAxis.Z, 6);
            Assert.Equal(original.YAxis.X, recreated.YAxis.X, 6);
            Assert.Equal(original.YAxis.Y, recreated.YAxis.Y, 6);
            Assert.Equal(original.YAxis.Z, recreated.YAxis.Z, 6);
            Assert.Equal(original.ZAxis.X, recreated.ZAxis.X, 6);
            Assert.Equal(original.ZAxis.Y, recreated.ZAxis.Y, 6);
            Assert.Equal(original.ZAxis.Z, recreated.ZAxis.Z, 6);
        });
    }

    [Fact]
    public void UCS_FromXAxisAndPointOnXYPlane_ShouldPreserveUCS()
    {
        // Arrange - Create a UCS from X-axis and a point on XY plane
        var xDirection = Vector3.Normalize(new Vector3(1, 0, 1));
        var pointOnPlane = new Vector3(0, 1, 0);
        var originalUCS = UCS.FromXAxisAndPointOnXYplane("XAxisPlaneUCS", new Vector3(25, 50, 75), xDirection, pointOnPlane);

        // Create a document with entities that use this UCS
        var originalDoc = new DxfDocument();
        originalDoc.UCSs.Add(originalUCS);

        // Add an arc to demonstrate the UCS
        var arc = new Arc(new Vector3(0, 0, 0), 30, 0, Math.PI)
        {
            Color = AciColor.Blue
        };
        originalDoc.Entities.Add(arc);

        // Act & Assert
        PerformUCSRoundTripTest(originalDoc, originalUCS, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            Assert.Equal(original.Origin.X, recreated.Origin.X, 6);
            Assert.Equal(original.Origin.Y, recreated.Origin.Y, 6);
            Assert.Equal(original.Origin.Z, recreated.Origin.Z, 6);
            Assert.Equal(original.XAxis.X, recreated.XAxis.X, 6);
            Assert.Equal(original.XAxis.Y, recreated.XAxis.Y, 6);
            Assert.Equal(original.XAxis.Z, recreated.XAxis.Z, 6);
            Assert.Equal(original.YAxis.X, recreated.YAxis.X, 6);
            Assert.Equal(original.YAxis.Y, recreated.YAxis.Y, 6);
            Assert.Equal(original.YAxis.Z, recreated.YAxis.Z, 6);
            Assert.Equal(original.ZAxis.X, recreated.ZAxis.X, 6);
            Assert.Equal(original.ZAxis.Y, recreated.ZAxis.Y, 6);
            Assert.Equal(original.ZAxis.Z, recreated.ZAxis.Z, 6);
        });
    }

    [Fact]
    public void UCS_WithZeroOrigin_ShouldPreserveProperties()
    {
        // Arrange - Create a UCS with zero origin (world coordinate system aligned)
        var originalUCS = new UCS("WorldAlignedUCS")
        {
            Origin = Vector3.Zero
        };
        originalUCS.SetAxis(Vector3.UnitX, Vector3.UnitY);

        // Create a document with entities that use this UCS
        var originalDoc = new DxfDocument();
        originalDoc.UCSs.Add(originalUCS);

        // Add a spline to demonstrate the UCS
        var controlPoints = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(25, 50, 0),
            new Vector3(75, 25, 0),
            new Vector3(100, 0, 0)
        };
        var spline = new Spline(controlPoints, null, 3, false);
        originalDoc.Entities.Add(spline);

        // Act & Assert
        PerformUCSRoundTripTest(originalDoc, originalUCS, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            Assert.Equal(original.Origin.X, recreated.Origin.X, 6);
            Assert.Equal(original.Origin.Y, recreated.Origin.Y, 6);
            Assert.Equal(original.Origin.Z, recreated.Origin.Z, 6);
            Assert.Equal(original.XAxis.X, recreated.XAxis.X, 6);
            Assert.Equal(original.XAxis.Y, recreated.XAxis.Y, 6);
            Assert.Equal(original.XAxis.Z, recreated.XAxis.Z, 6);
            Assert.Equal(original.YAxis.X, recreated.YAxis.X, 6);
            Assert.Equal(original.YAxis.Y, recreated.YAxis.Y, 6);
            Assert.Equal(original.YAxis.Z, recreated.YAxis.Z, 6);
            Assert.Equal(original.ZAxis.X, recreated.ZAxis.X, 6);
            Assert.Equal(original.ZAxis.Y, recreated.ZAxis.Y, 6);
            Assert.Equal(original.ZAxis.Z, recreated.ZAxis.Z, 6);
        });
    }

    /// <summary>
    /// Performs a complete round-trip test for a UCS table object:
    /// 1. Save document with UCS to file
    /// 2. Load from file
    /// 3. Generate C# code
    /// 4. Compile and execute code
    /// 5. Validate the recreated UCS
    /// </summary>
    private void PerformUCSRoundTripTest(DxfDocument originalDoc, UCS originalUCS, Action<UCS, UCS> validator)
    {
        // Step 1: Save to DXF file
        var originalDxfPath = Path.Combine(_tempDirectory, "original.dxf");
        originalDoc.Save(originalDxfPath);

        // Step 2: Load DXF file
        var loadedDoc = DxfDocument.Load(originalDxfPath);
        Assert.NotNull(loadedDoc);

        // Debug: Print all UCS names in the loaded document
        _output.WriteLine($"Loaded UCS count: {loadedDoc.UCSs.Count}");
        foreach (var ucs in loadedDoc.UCSs.Items)
        {
            _output.WriteLine($"  Found UCS: {ucs.Name}");
        }
        _output.WriteLine($"Looking for UCS: {originalUCS.Name}");

        // Debug: Check what Names collection contains
        _output.WriteLine($"UCSs.Names count: {loadedDoc.UCSs.Names.Count()}");
        foreach (var name in loadedDoc.UCSs.Names)
        {
            _output.WriteLine($"  Name in Names collection: {name}");
        }

        // Try using Contains method instead of Assert.Contains
        _output.WriteLine($"Contains check: {loadedDoc.UCSs.Contains(originalUCS.Name)}");
        Assert.True(loadedDoc.UCSs.Contains(originalUCS.Name), $"UCS '{originalUCS.Name}' not found in loaded document");

        // Step 3: Generate C# code from the loaded document
        var generatedCode = _generator.Generate(loadedDoc, originalDxfPath);
        Assert.NotNull(generatedCode);
        Assert.NotEmpty(generatedCode);

        // Debug: Print the generated code to see if UCS is included
        _output.WriteLine("Generated code:");
        _output.WriteLine(generatedCode);

        // Step 4: Compile and execute the generated code
        var recreatedDoc = CompileAndExecuteCode(generatedCode);
        Assert.NotNull(recreatedDoc);

        // Debug: Print all UCS names in the recreated document
        _output.WriteLine($"Recreated UCS count: {recreatedDoc.UCSs.Count}");
        foreach (var ucs in recreatedDoc.UCSs.Items)
        {
            _output.WriteLine($"  Found recreated UCS: {ucs.Name}");
        }

        Assert.Contains(originalUCS.Name, recreatedDoc.UCSs.Names);

        // Step 5: Validate the recreated UCS matches the original
        var recreatedUCS = recreatedDoc.UCSs[originalUCS.Name];
        Assert.NotNull(recreatedUCS);
        validator(originalUCS, recreatedUCS);

        // Step 6: Save recreated document and verify it can be loaded
        var recreatedDxfPath = Path.Combine(_tempDirectory, "recreated.dxf");
        recreatedDoc.Save(recreatedDxfPath);
        var finalDoc = DxfDocument.Load(recreatedDxfPath);
        Assert.NotNull(finalDoc);
        Assert.Contains(originalUCS.Name, finalDoc.UCSs.Names);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}
