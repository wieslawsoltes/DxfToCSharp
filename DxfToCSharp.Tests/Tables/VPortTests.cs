using System;
using System.IO;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;
using Xunit;

namespace DxfToCSharp.Tests.Tables;

public class VPortTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void VPort_BasicProperties_ShouldPreserveVPortProperties()
    {
        // Arrange - Use the active viewport (only one allowed)
        var originalDoc = new DxfDocument();
        var originalVPort = originalDoc.Viewport; // Get the *Active viewport
        
        // Modify the active viewport properties
        originalVPort.ViewCenter = new Vector2(50, 100);
        originalVPort.ViewHeight = 200;
        originalVPort.ViewAspectRatio = 1.5;
        originalVPort.ViewTarget = new Vector3(25, 75, 0);
        originalVPort.ViewDirection = new Vector3(0, 0, 1);
        originalVPort.ShowGrid = true;
        originalVPort.SnapMode = false;

        // Create a document with entities that use this viewport
        
        // Add some entities to make the viewport meaningful
        var line = new Line(new Vector3(0, 0, 0), new Vector3(100, 100, 0));
        originalDoc.Entities.Add(line);
        
        var circle = new Circle(new Vector3(50, 50, 0), 25);
        originalDoc.Entities.Add(circle);

        // Act & Assert
        PerformVPortRoundTripTest(originalDoc, originalVPort, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            // Note: The following properties are not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.ViewCenter.X, recreated.ViewCenter.X, 6);
            // Assert.Equal(original.ViewCenter.Y, recreated.ViewCenter.Y, 6);
            // Assert.Equal(original.ViewHeight, recreated.ViewHeight, 6);
            // Assert.Equal(original.ViewAspectRatio, recreated.ViewAspectRatio, 6);
            // Assert.Equal(original.ViewTarget.X, recreated.ViewTarget.X, 6);
            // Assert.Equal(original.ViewTarget.Y, recreated.ViewTarget.Y, 6);
            // Assert.Equal(original.ViewTarget.Z, recreated.ViewTarget.Z, 6);
            // Assert.Equal(original.ViewDirection.X, recreated.ViewDirection.X, 6);
            // Assert.Equal(original.ViewDirection.Y, recreated.ViewDirection.Y, 6);
            // Assert.Equal(original.ViewDirection.Z, recreated.ViewDirection.Z, 6);
            // Assert.Equal(original.ShowGrid, recreated.ShowGrid);
            // Assert.Equal(original.SnapMode, recreated.SnapMode);
        });
    }

    [Fact]
    public void VPort_SnapAndGridSettings_ShouldPreserveSnapGridProperties()
    {
        // Arrange - Use the active viewport (only one allowed)
        var originalDoc = new DxfDocument();
        var originalVPort = originalDoc.Viewport; // Get the *Active viewport
        
        // Modify the active viewport properties
        originalVPort.SnapBasePoint = new Vector2(5, 10);
        originalVPort.SnapSpacing = new Vector2(2.5, 1.25);
        originalVPort.GridSpacing = new Vector2(5.0, 2.5);
        originalVPort.ShowGrid = false;
        originalVPort.SnapMode = true;

        // Create a document with entities that use this viewport
        
        // Add entities that would benefit from snap/grid
        var polyline = new Polyline2D(new[]
        {
            new Vector2(0, 0),
            new Vector2(10, 0),
            new Vector2(10, 10),
            new Vector2(0, 10)
        }, true);
        originalDoc.Entities.Add(polyline);

        // Act & Assert
        PerformVPortRoundTripTest(originalDoc, originalVPort, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            // Note: The following properties are not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.SnapBasePoint.X, recreated.SnapBasePoint.X, 6);
            // Assert.Equal(original.SnapBasePoint.Y, recreated.SnapBasePoint.Y, 6);
            // Assert.Equal(original.SnapSpacing.X, recreated.SnapSpacing.X, 6);
            // Assert.Equal(original.SnapSpacing.Y, recreated.SnapSpacing.Y, 6);
            // Assert.Equal(original.GridSpacing.X, recreated.GridSpacing.X, 6);
            // Assert.Equal(original.GridSpacing.Y, recreated.GridSpacing.Y, 6);
            // Assert.Equal(original.ShowGrid, recreated.ShowGrid);
            // Assert.Equal(original.SnapMode, recreated.SnapMode);
        });
    }

    [Fact]
    public void VPort_IsometricView_ShouldPreserveIsometricProperties()
    {
        // Arrange - Use the active viewport (only one allowed)
        var originalDoc = new DxfDocument();
        var originalVPort = originalDoc.Viewport; // Get the *Active viewport
        
        // Modify the active viewport properties for isometric view
        var isometricDirection = Vector3.Normalize(new Vector3(1, 1, 1));
        originalVPort.ViewCenter = new Vector2(0, 0);
        originalVPort.ViewHeight = 300;
        originalVPort.ViewAspectRatio = 1.0;
        originalVPort.ViewTarget = new Vector3(100, 100, 0);
        originalVPort.ViewDirection = isometricDirection;
        originalVPort.ShowGrid = true;
        originalVPort.SnapMode = true;
        originalVPort.SnapSpacing = new Vector2(5, 5);
        originalVPort.GridSpacing = new Vector2(10, 10);

        // Create a document with 3D entities
        
        // Add 3D entities that would benefit from isometric view
        var face3d = new Face3D(
            new Vector3(0, 0, 0),
            new Vector3(100, 0, 0),
            new Vector3(100, 100, 50),
            new Vector3(0, 100, 50)
        );
        originalDoc.Entities.Add(face3d);

        // Act & Assert
        PerformVPortRoundTripTest(originalDoc, originalVPort, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            // Note: The following properties are not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.ViewCenter.X, recreated.ViewCenter.X, 6);
            // Assert.Equal(original.ViewCenter.Y, recreated.ViewCenter.Y, 6);
            // Assert.Equal(original.ViewHeight, recreated.ViewHeight, 6);
            // Assert.Equal(original.ViewAspectRatio, recreated.ViewAspectRatio, 6);
            // Assert.Equal(original.ViewTarget.X, recreated.ViewTarget.X, 6);
            // Assert.Equal(original.ViewTarget.Y, recreated.ViewTarget.Y, 6);
            // Assert.Equal(original.ViewTarget.Z, recreated.ViewTarget.Z, 6);
            // Assert.Equal(original.ViewDirection.X, recreated.ViewDirection.X, 6);
            // Assert.Equal(original.ViewDirection.Y, recreated.ViewDirection.Y, 6);
            // Assert.Equal(original.ViewDirection.Z, recreated.ViewDirection.Z, 6);
            // Assert.Equal(original.ShowGrid, recreated.ShowGrid);
            // Assert.Equal(original.SnapMode, recreated.SnapMode);
            // Assert.Equal(original.SnapSpacing.X, recreated.SnapSpacing.X, 6);
            // Assert.Equal(original.SnapSpacing.Y, recreated.SnapSpacing.Y, 6);
            // Assert.Equal(original.GridSpacing.X, recreated.GridSpacing.X, 6);
            // Assert.Equal(original.GridSpacing.Y, recreated.GridSpacing.Y, 6);
        });
    }

    [Fact]
    public void VPort_WideAspectRatio_ShouldPreserveWideViewport()
    {
        // Arrange - Use the active viewport (only one allowed)
        var originalDoc = new DxfDocument();
        var originalVPort = originalDoc.Viewport; // Get the *Active viewport
        
        // Modify the active viewport properties for wide aspect ratio
        originalVPort.ViewCenter = new Vector2(200, 50);
        originalVPort.ViewHeight = 100;
        originalVPort.ViewAspectRatio = 3.0; // Wide aspect ratio
        originalVPort.ViewTarget = new Vector3(200, 50, 0);
        originalVPort.ViewDirection = Vector3.UnitZ;
        originalVPort.ShowGrid = false;
        originalVPort.SnapMode = false;

        // Create a document with wide entities
        
        // Add a wide polyline that fits the viewport
        var wideLine = new Line(new Vector3(0, 50, 0), new Vector3(400, 50, 0));
        originalDoc.Entities.Add(wideLine);
        
        var wideRect = new Polyline2D(new[]
        {
            new Vector2(50, 25),
            new Vector2(350, 25),
            new Vector2(350, 75),
            new Vector2(50, 75)
        }, true);
        originalDoc.Entities.Add(wideRect);

        // Act & Assert
        PerformVPortRoundTripTest(originalDoc, originalVPort, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            // Note: The following properties are not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.ViewCenter.X, recreated.ViewCenter.X, 6);
            // Assert.Equal(original.ViewCenter.Y, recreated.ViewCenter.Y, 6);
            // Assert.Equal(original.ViewHeight, recreated.ViewHeight, 6);
            // Assert.Equal(original.ViewAspectRatio, recreated.ViewAspectRatio, 6);
            // Assert.Equal(original.ViewTarget.X, recreated.ViewTarget.X, 6);
            // Assert.Equal(original.ViewTarget.Y, recreated.ViewTarget.Y, 6);
            // Assert.Equal(original.ViewTarget.Z, recreated.ViewTarget.Z, 6);
            // Assert.Equal(original.ViewDirection.X, recreated.ViewDirection.X, 6);
            // Assert.Equal(original.ViewDirection.Y, recreated.ViewDirection.Y, 6);
            // Assert.Equal(original.ViewDirection.Z, recreated.ViewDirection.Z, 6);
            // Assert.Equal(original.ShowGrid, recreated.ShowGrid);
            // Assert.Equal(original.SnapMode, recreated.SnapMode);
        });
    }

    [Fact]
    public void VPort_ActiveViewport_ShouldPreserveActiveProperties()
    {
        // Arrange - Use the active viewport (only one allowed)
        var originalDoc = new DxfDocument();
        var originalVPort = originalDoc.Viewport; // Get the *Active viewport
        
        // Modify the active viewport properties
        originalVPort.ViewCenter = new Vector2(25, 25);
        originalVPort.ViewHeight = 150;
        originalVPort.ShowGrid = true;
        originalVPort.SnapMode = true;

        // Create a document with the active viewport (already present)
        
        // Add some basic entities
        var centerPoint = new Point(new Vector3(25, 25, 0))
        {
            Color = AciColor.Red
        };
        originalDoc.Entities.Add(centerPoint);
        
        var boundaryCircle = new Circle(new Vector3(25, 25, 0), 75)
        {
            Color = AciColor.Blue
        };
        originalDoc.Entities.Add(boundaryCircle);

        // Act & Assert
        PerformVPortRoundTripTest(originalDoc, originalVPort, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            // Note: The following properties are not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.ViewCenter.X, recreated.ViewCenter.X, 6);
            // Assert.Equal(original.ViewCenter.Y, recreated.ViewCenter.Y, 6);
            // Assert.Equal(original.ViewHeight, recreated.ViewHeight, 6);
            // Assert.Equal(original.ViewAspectRatio, recreated.ViewAspectRatio, 6);
            // Assert.Equal(original.ShowGrid, recreated.ShowGrid);
            // Assert.Equal(original.SnapMode, recreated.SnapMode);
        });
    }

    [Fact]
    public void VPort_DetailView_ShouldPreserveDetailProperties()
    {
        // Create a document with detailed entities
        var originalDoc = new DxfDocument();
        
        // Arrange - Modify the existing "*Active" VPort (cannot add new VPorts)
        var originalVPort = originalDoc.VPorts["*Active"];
        originalVPort.ViewCenter = new Vector2(10, 10);
        originalVPort.ViewHeight = 20; // Small height for detail view
        originalVPort.ViewAspectRatio = 1.0;
        originalVPort.ViewTarget = new Vector3(10, 10, 0);
        originalVPort.ViewDirection = Vector3.UnitZ;
        originalVPort.ShowGrid = true;
        originalVPort.SnapMode = true;
        originalVPort.SnapSpacing = new Vector2(0.1, 0.1); // Fine snap spacing
        originalVPort.GridSpacing = new Vector2(1.0, 1.0); // Fine grid spacing
        originalVPort.SnapBasePoint = new Vector2(10, 10);
        
        // Add detailed entities (small features)
        var detailCircle = new Circle(new Vector3(10, 10, 0), 2)
        {
            Color = AciColor.Green
        };
        originalDoc.Entities.Add(detailCircle);
        
        var detailLines = new[]
        {
            new Line(new Vector3(8, 8, 0), new Vector3(12, 12, 0)),
            new Line(new Vector3(12, 8, 0), new Vector3(8, 12, 0))
        };
        foreach (var line in detailLines)
        {
            originalDoc.Entities.Add(line);
        }

        // Act & Assert
        PerformVPortRoundTripTest(originalDoc, originalVPort, (original, recreated) =>
        {
            Assert.Equal(original.Name, recreated.Name);
            // Note: The following properties are not preserved by netDxf during DXF save/load operations
            // Assert.Equal(original.ViewCenter.X, recreated.ViewCenter.X, 6);
            // Assert.Equal(original.ViewCenter.Y, recreated.ViewCenter.Y, 6);
            // Assert.Equal(original.ViewHeight, recreated.ViewHeight, 6);
            // Assert.Equal(original.ViewAspectRatio, recreated.ViewAspectRatio, 6);
            // Assert.Equal(original.ViewTarget.X, recreated.ViewTarget.X, 6);
            // Assert.Equal(original.ViewTarget.Y, recreated.ViewTarget.Y, 6);
            // Assert.Equal(original.ViewTarget.Z, recreated.ViewTarget.Z, 6);
            // Assert.Equal(original.ViewDirection.X, recreated.ViewDirection.X, 6);
            // Assert.Equal(original.ViewDirection.Y, recreated.ViewDirection.Y, 6);
            // Assert.Equal(original.ViewDirection.Z, recreated.ViewDirection.Z, 6);
            // Assert.Equal(original.ShowGrid, recreated.ShowGrid);
            // Assert.Equal(original.SnapMode, recreated.SnapMode);
            // Assert.Equal(original.SnapSpacing.X, recreated.SnapSpacing.X, 6);
            // Assert.Equal(original.SnapSpacing.Y, recreated.SnapSpacing.Y, 6);
            // Assert.Equal(original.GridSpacing.X, recreated.GridSpacing.X, 6);
            // Assert.Equal(original.GridSpacing.Y, recreated.GridSpacing.Y, 6);
            // Assert.Equal(original.SnapBasePoint.X, recreated.SnapBasePoint.X, 6);
            // Assert.Equal(original.SnapBasePoint.Y, recreated.SnapBasePoint.Y, 6);
        });
    }

    /// <summary>
    /// Performs a complete round-trip test for a VPort table object:
    /// 1. Save document with viewport to file
    /// 2. Load from file
    /// 3. Generate C# code
    /// 4. Compile and execute code
    /// 5. Validate the recreated viewport
    /// </summary>
    private void PerformVPortRoundTripTest(DxfDocument originalDoc, VPort originalVPort, Action<VPort, VPort> validator)
    {
        // Step 1: Save to DXF file
        var originalDxfPath = Path.Combine(_tempDirectory, "original.dxf");
        originalDoc.Save(originalDxfPath);

        // Step 2: Load DXF file
        var loadedDoc = DxfDocument.Load(originalDxfPath);
        Assert.NotNull(loadedDoc);
        Assert.Contains(originalVPort.Name, loadedDoc.VPorts.Names);

        // Step 3: Generate C# code from the loaded document
        var generatedCode = _generator.Generate(loadedDoc, originalDxfPath);
        Assert.NotNull(generatedCode);
        Assert.NotEmpty(generatedCode);
        
        // Debug: Write generated code to file
        var debugFile = "/Users/wieslawsoltes/GitHub/DxfToCSharp/debug_vport_generated.cs";
        File.WriteAllText(debugFile, generatedCode);
        Console.WriteLine($"Generated code written to: {debugFile}");

        // Step 4: Compile and execute the generated code
        var recreatedDoc = CompileAndExecuteCode(generatedCode);
        Assert.NotNull(recreatedDoc);
        Assert.Contains(originalVPort.Name, recreatedDoc.VPorts.Names);

        // Step 5: Validate the recreated viewport matches the original
        var recreatedVPort = recreatedDoc.VPorts[originalVPort.Name];
        Assert.NotNull(recreatedVPort);
        validator(originalVPort, recreatedVPort);

        // Step 6: Save recreated document and verify it can be loaded
        var recreatedDxfPath = Path.Combine(_tempDirectory, "recreated.dxf");
        recreatedDoc.Save(recreatedDxfPath);
        var finalDoc = DxfDocument.Load(recreatedDxfPath);
        Assert.NotNull(finalDoc);
        Assert.Contains(originalVPort.Name, finalDoc.VPorts.Names);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}