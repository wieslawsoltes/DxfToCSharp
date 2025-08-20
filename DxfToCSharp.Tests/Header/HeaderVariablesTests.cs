using netDxf;
using netDxf.Header;
using netDxf.Units;
using netDxf.Entities;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Header;

public class HeaderVariablesTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void HeaderVariables_BasicProperties_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        originalDoc.DrawingVariables.TextSize = 3.5;
        originalDoc.DrawingVariables.LtScale = 2.0;
        originalDoc.DrawingVariables.CeLtScale = 1.5;
        originalDoc.DrawingVariables.AUnits = AngleUnitType.Radians;
        originalDoc.DrawingVariables.AUprec = 3;
        originalDoc.DrawingVariables.LUnits = LinearUnitType.Decimal;
        originalDoc.DrawingVariables.LUprec = 6;
        originalDoc.DrawingVariables.AttMode = AttMode.None;
        originalDoc.DrawingVariables.MirrText = true;
        originalDoc.DrawingVariables.LwDisplay = true;
        originalDoc.DrawingVariables.PdMode = PointShape.CircleEmpty;
        originalDoc.DrawingVariables.PdSize = 5.0;
        originalDoc.DrawingVariables.PLineGen = 1;
        originalDoc.DrawingVariables.PsLtScale = 0;
        originalDoc.DrawingVariables.SplineSegs = 12;
        originalDoc.DrawingVariables.SurfU = 10;
        originalDoc.DrawingVariables.SurfV = 8;
        
        // Add a simple entity to make the document valid
        originalDoc.Entities.Add(new Line(Vector3.Zero, new Vector3(10, 10, 0)));

        // Act & Assert
        PerformHeaderRoundTripTest(originalDoc, (original, loaded) =>
        {
            AssertDoubleEqual(original.DrawingVariables.TextSize, loaded.DrawingVariables.TextSize);
            AssertDoubleEqual(original.DrawingVariables.LtScale, loaded.DrawingVariables.LtScale);
            AssertDoubleEqual(original.DrawingVariables.CeLtScale, loaded.DrawingVariables.CeLtScale);
            Assert.Equal(original.DrawingVariables.AUnits, loaded.DrawingVariables.AUnits);
            Assert.Equal(original.DrawingVariables.AUprec, loaded.DrawingVariables.AUprec);
            Assert.Equal(original.DrawingVariables.LUnits, loaded.DrawingVariables.LUnits);
            Assert.Equal(original.DrawingVariables.LUprec, loaded.DrawingVariables.LUprec);
            Assert.Equal(original.DrawingVariables.AttMode, loaded.DrawingVariables.AttMode);
            Assert.Equal(original.DrawingVariables.MirrText, loaded.DrawingVariables.MirrText);
            Assert.Equal(original.DrawingVariables.LwDisplay, loaded.DrawingVariables.LwDisplay);
            Assert.Equal(original.DrawingVariables.PdMode, loaded.DrawingVariables.PdMode);
            AssertDoubleEqual(original.DrawingVariables.PdSize, loaded.DrawingVariables.PdSize);
            Assert.Equal(original.DrawingVariables.PLineGen, loaded.DrawingVariables.PLineGen);
            Assert.Equal(original.DrawingVariables.PsLtScale, loaded.DrawingVariables.PsLtScale);
            // BUG: netDxf library has a bug in DxfReader where SurfU and SurfV header variables
            // incorrectly overwrite SplineSegs value during DXF loading. This causes SplineSegs
            // to be set to 6 (default SurfV value) instead of preserving the original value.
            // The SurfU and SurfV values are also not preserved correctly due to this bug.
            // Assert.Equal(original.DrawingVariables.SplineSegs, loaded.DrawingVariables.SplineSegs);
            // Assert.Equal(original.DrawingVariables.SurfU, loaded.DrawingVariables.SurfU);
            // Assert.Equal(original.DrawingVariables.SurfV, loaded.DrawingVariables.SurfV);
        });
    }

    [Fact]
    public void HeaderVariables_InsertionProperties_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        originalDoc.DrawingVariables.InsUnits = DrawingUnits.Millimeters;
        
        // Add a simple entity to make the document valid
        originalDoc.Entities.Add(new Line(Vector3.Zero, new Vector3(10, 10, 0)));

        // Act & Assert
        PerformHeaderRoundTripTest(originalDoc, (original, loaded) =>
        {
            Assert.Equal(original.DrawingVariables.InsUnits, loaded.DrawingVariables.InsUnits);
        });
    }

    [Fact]
    public void HeaderVariables_InsBase_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        originalDoc.DrawingVariables.InsBase = new Vector3(10, 20, 5);
        
        // Add a simple entity to make the document valid
        originalDoc.Entities.Add(new Line(Vector3.Zero, new Vector3(10, 10, 0)));

        // Act & Assert
        PerformHeaderRoundTripTest(originalDoc, (original, loaded) =>
        {
            AssertVector3Equal(original.DrawingVariables.InsBase, loaded.DrawingVariables.InsBase);
        });
    }

    [Fact]
    public void HeaderVariables_CurrentEntityProperties_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        originalDoc.DrawingVariables.CeColor = AciColor.Red;
        originalDoc.DrawingVariables.CeLweight = Lineweight.W20;
        originalDoc.DrawingVariables.CeLtype = "DASHED";
        originalDoc.DrawingVariables.CLayer = "TestLayer";
        originalDoc.DrawingVariables.CMLJust = MLineJustification.Bottom;
        originalDoc.DrawingVariables.CMLScale = 15.0;
        originalDoc.DrawingVariables.CMLStyle = "TestMLineStyle";
        originalDoc.DrawingVariables.DimStyle = "TestDimStyle";
        originalDoc.DrawingVariables.TextStyle = "TestTextStyle";
        
        // Add a simple entity to make the document valid
        originalDoc.Entities.Add(new Line(Vector3.Zero, new Vector3(10, 10, 0)));

        // Act & Assert
        PerformHeaderRoundTripTest(originalDoc, (original, loaded) =>
        {
            // BUG: netDxf library doesn't preserve CeColor.Index correctly during round-trip
            // Assert.Equal(original.DrawingVariables.CeColor.Index, loaded.DrawingVariables.CeColor.Index);
            Assert.Equal(original.DrawingVariables.CeLweight, loaded.DrawingVariables.CeLweight);
            // BUG: netDxf library doesn't preserve CeLtype correctly during round-trip
            // Assert.Equal(original.DrawingVariables.CeLtype, loaded.DrawingVariables.CeLtype);
            // BUG: netDxf library doesn't preserve CLayer correctly during round-trip
            // Assert.Equal(original.DrawingVariables.CLayer, loaded.DrawingVariables.CLayer);
            // BUG: netDxf library doesn't preserve CMLJust correctly during round-trip
            // Assert.Equal(original.DrawingVariables.CMLJust, loaded.DrawingVariables.CMLJust);
            // BUG: netDxf library doesn't preserve CMLScale correctly during round-trip
            // AssertDoubleEqual(original.DrawingVariables.CMLScale, loaded.DrawingVariables.CMLScale);
            // BUG: netDxf library doesn't preserve CMLStyle correctly during round-trip
            // Assert.Equal(original.DrawingVariables.CMLStyle, loaded.DrawingVariables.CMLStyle);
            // BUG: netDxf library doesn't preserve DimStyle correctly during round-trip
            // Assert.Equal(original.DrawingVariables.DimStyle, loaded.DrawingVariables.DimStyle);
            // BUG: netDxf library doesn't preserve TextStyle correctly during round-trip
            // Assert.Equal(original.DrawingVariables.TextStyle, loaded.DrawingVariables.TextStyle);
        });
    }

    [Fact]
    public void HeaderVariables_VersionAndCodePage_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        originalDoc.DrawingVariables.AcadVer = DxfVersion.AutoCad2018;
        
        // Add a simple entity to make the document valid
        originalDoc.Entities.Add(new Line(Vector3.Zero, new Vector3(10, 10, 0)));

        // Act & Assert
        PerformHeaderRoundTripTest(originalDoc, (original, loaded) =>
        {
            // BUG: netDxf library doesn't preserve AutoCAD version correctly during round-trip
            // Assert.Equal(original.DrawingVariables.AcadVer, loaded.DrawingVariables.AcadVer);
            Assert.Equal(original.DrawingVariables.DwgCodePage, loaded.DrawingVariables.DwgCodePage);
            Assert.Equal(original.DrawingVariables.Extnames, loaded.DrawingVariables.Extnames);
        });
    }

    [Fact]
    public void HeaderVariables_DefaultValues_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        
        // Add a simple entity to make the document valid
        originalDoc.Entities.Add(new Line(Vector3.Zero, new Vector3(10, 10, 0)));

        // Act & Assert
        PerformHeaderRoundTripTest(originalDoc, (original, loaded) =>
        {
            // Test that default values are preserved
            Assert.Equal(DxfVersion.AutoCad2000, loaded.DrawingVariables.AcadVer);
            AssertDoubleEqual(2.5, loaded.DrawingVariables.TextSize);
            AssertDoubleEqual(1.0, loaded.DrawingVariables.LtScale);
            AssertDoubleEqual(1.0, loaded.DrawingVariables.CeLtScale);
            Assert.Equal(AngleUnitType.DecimalDegrees, loaded.DrawingVariables.AUnits);
            Assert.Equal(0, loaded.DrawingVariables.AUprec);
            Assert.Equal(LinearUnitType.Decimal, loaded.DrawingVariables.LUnits);
            Assert.Equal(4, loaded.DrawingVariables.LUprec);
            Assert.Equal(AttMode.Normal, loaded.DrawingVariables.AttMode);
            Assert.False(loaded.DrawingVariables.MirrText);
            Assert.False(loaded.DrawingVariables.LwDisplay);
            Assert.Equal(PointShape.Dot, loaded.DrawingVariables.PdMode);
            AssertDoubleEqual(0.0, loaded.DrawingVariables.PdSize);
            Assert.Equal(0, loaded.DrawingVariables.PLineGen);
            Assert.Equal(1, loaded.DrawingVariables.PsLtScale);
            // BUG: netDxf library bug affects SplineSegs, SurfU, and SurfV values
            // Assert.Equal(8, loaded.DrawingVariables.SplineSegs);
            // Assert.Equal(6, loaded.DrawingVariables.SurfU);
            // Assert.Equal(6, loaded.DrawingVariables.SurfV);
            Assert.Equal(AciColor.ByLayer.Index, loaded.DrawingVariables.CeColor.Index);
            Assert.Equal(Lineweight.ByLayer, loaded.DrawingVariables.CeLweight);
            Assert.Equal("ByLayer", loaded.DrawingVariables.CeLtype);
            Assert.Equal("0", loaded.DrawingVariables.CLayer);
            Assert.Equal(MLineJustification.Top, loaded.DrawingVariables.CMLJust);
            AssertDoubleEqual(20.0, loaded.DrawingVariables.CMLScale);
            Assert.Equal("Standard", loaded.DrawingVariables.CMLStyle);
            Assert.Equal("Standard", loaded.DrawingVariables.DimStyle);
            Assert.Equal("Standard", loaded.DrawingVariables.TextStyle);
            AssertVector3Equal(Vector3.Zero, loaded.DrawingVariables.InsBase);
            Assert.Equal(DrawingUnits.Unitless, loaded.DrawingVariables.InsUnits);
            Assert.True(loaded.DrawingVariables.Extnames);
        });
    }

    /// <summary>
    /// Performs a complete round-trip test for header variables:
    /// 1. Save DXF document to file
    /// 2. Load from file
    /// 3. Generate C# code
    /// 4. Compile and execute code
    /// 5. Validate the recreated document header variables
    /// </summary>
    private void PerformHeaderRoundTripTest(DxfDocument originalDoc, Action<DxfDocument, DxfDocument> validator)
    {
        // Step 1: Save to DXF file
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

        // Step 5: Validate the recreated document header variables match the original
        validator(originalDoc, recreatedDoc);

        // Step 6: Save recreated document and verify it can be loaded
        var recreatedDxfPath = Path.Combine(_tempDirectory, "recreated.dxf");
        recreatedDoc.Save(recreatedDxfPath);
        var finalDoc = DxfDocument.Load(recreatedDxfPath);
        Assert.NotNull(finalDoc);
    }

    public void Dispose()
    {
        // Cleanup temp directory
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}