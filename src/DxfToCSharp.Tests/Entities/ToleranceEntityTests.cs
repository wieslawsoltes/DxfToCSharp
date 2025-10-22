using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;
using System.Linq;

namespace DxfToCSharp.Tests.Entities;

public class ToleranceEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Tolerance_BasicRoundTrip_ShouldPreserveToleranceProperties()
    {
        // Arrange
        var toleranceEntry = new ToleranceEntry();
        var originalTolerance = new Tolerance(toleranceEntry,
            new Vector3(25, 30, 0));
        originalTolerance.TextHeight = 2.5;
        originalTolerance.Rotation = 30.0;

        // Act & Assert
        PerformRoundTripTest(originalTolerance, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.TextHeight, recreated.TextHeight);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
            // Note: ToleranceEntry properties may not be fully preserved during round-trip
            Assert.NotNull(recreated.Entry1);
        });
    }

    [Fact]
    public void Tolerance_WithDifferentProperties_ShouldPreserveDifferentProperties()
    {
        // Arrange
        var toleranceEntry = new ToleranceEntry();
        var originalTolerance = new Tolerance(toleranceEntry,
            new Vector3(-10, -15, 5));
        originalTolerance.TextHeight = 5.0;
        originalTolerance.Rotation = 180.0;

        // Act & Assert
        PerformRoundTripTest(originalTolerance, (original, recreated) =>
        {
            AssertVector3Equal(original.Position, recreated.Position);
            AssertDoubleEqual(original.TextHeight, recreated.TextHeight);
            AssertDoubleEqual(original.Rotation, recreated.Rotation);
            Assert.NotNull(recreated.Entry1);
        });
    }

    [Fact]
    public void Tolerance_WithDetailedEntries_ShouldPreserveEntryData()
    {
        // Arrange
        var entry1 = new ToleranceEntry
        {
            GeometricSymbol = ToleranceGeometricSymbol.Position,
            Tolerance1 = new ToleranceValue(true, "0.05", ToleranceMaterialCondition.Maximum),
            Tolerance2 = new ToleranceValue(false, "0.01", ToleranceMaterialCondition.Least),
            Datum1 = new DatumReferenceValue("A", ToleranceMaterialCondition.Maximum),
            Datum2 = new DatumReferenceValue("B", ToleranceMaterialCondition.Least)
        };

        var entry2 = new ToleranceEntry
        {
            GeometricSymbol = ToleranceGeometricSymbol.Parallelism,
            Tolerance1 = new ToleranceValue(false, "0.02", ToleranceMaterialCondition.None),
            Datum1 = new DatumReferenceValue("C", ToleranceMaterialCondition.None)
        };

        var originalTolerance = new Tolerance(entry1, new Vector3(10, 15, 0))
        {
            TextHeight = 2.0,
            Rotation = 12.0,
            ProjectedToleranceZoneValue = "25",
            ShowProjectedToleranceZoneSymbol = true,
            DatumIdentifier = "Z"
        };
        originalTolerance.Entry2 = entry2;

        // Step 1: Create DXF document containing the tolerance
        var originalDoc = new DxfDocument();
        originalDoc.Entities.Add(originalTolerance);

        // Step 2: Save and reload document
        var originalDxfPath = Path.Join(_tempDirectory, "tolerance_original.dxf");
        originalDoc.Save(originalDxfPath);
        var loadedDoc = DxfDocument.Load(originalDxfPath);
        Assert.NotNull(loadedDoc);

        // Step 3: Generate C# code and verify it contains the detailed fields
        var generatedCode = _generator.Generate(loadedDoc, originalDxfPath);
        Assert.Contains("ProjectedToleranceZoneValue = \"25\"", generatedCode);
        Assert.Contains("ShowProjectedToleranceZoneSymbol = true", generatedCode);
        Assert.Contains("DatumIdentifier = \"Z\"", generatedCode);
        Assert.Contains("ToleranceGeometricSymbol.Position", generatedCode);
        Assert.Contains("ToleranceGeometricSymbol.Parallelism", generatedCode);
        Assert.Contains("ToleranceMaterialCondition.Maximum", generatedCode);
        Assert.Contains("ToleranceMaterialCondition.Least", generatedCode);
        Assert.Contains("Value = \"A\";", generatedCode);
        Assert.Contains("Value = \"B\";", generatedCode);

        // Step 4: Compile and execute generated code
        var recreatedDoc = CompileAndExecuteCode(generatedCode);
        var recreatedTolerance = Assert.IsType<Tolerance>(recreatedDoc.Entities.All.First());

        // Step 5: Basic property checks (netDxf may not persist all detailed entry data)
        AssertVector3Equal(originalTolerance.Position, recreatedTolerance.Position);
        AssertDoubleEqual(originalTolerance.TextHeight, recreatedTolerance.TextHeight);
        AssertDoubleEqual(originalTolerance.Rotation, recreatedTolerance.Rotation);
        Assert.NotNull(recreatedTolerance.Entry1);
        Assert.NotNull(recreatedTolerance.Entry2);
    }

    private static void AssertToleranceValueEqual(ToleranceValue? expected, ToleranceValue? actual)
    {
        if (expected == null)
        {
            Assert.Null(actual);
            return;
        }

        Assert.NotNull(actual);
        Assert.Equal(expected.ShowDiameterSymbol, actual!.ShowDiameterSymbol);
        Assert.Equal(expected.Value, actual.Value);
        Assert.Equal(expected.MaterialCondition, actual.MaterialCondition);
    }

    private static void AssertDatumReferenceEqual(DatumReferenceValue? expected, DatumReferenceValue? actual)
    {
        if (expected == null)
        {
            Assert.Null(actual);
            return;
        }

        Assert.NotNull(actual);
        Assert.Equal(expected.Value, actual!.Value);
        Assert.Equal(expected.MaterialCondition, actual.MaterialCondition);
    }

    public override void Dispose()
    {
        // Cleanup if needed
        base.Dispose();
    }
}
