using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;

namespace DxfToCSharp.Tests.Entities;

public class HatchEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Hatch_SolidFillRectangle_ShouldPreserveGeometry()
    {
        // Arrange
        var boundaryEntities = new List<EntityObject>
        {
            new Line(new Vector2(0, 0), new Vector2(10, 0)),
            new Line(new Vector2(10, 0), new Vector2(10, 5)),
            new Line(new Vector2(10, 5), new Vector2(0, 5)),
            new Line(new Vector2(0, 5), new Vector2(0, 0))
        };

        var boundaryPath = new HatchBoundaryPath(boundaryEntities);
        var originalHatch = new Hatch(HatchPattern.Solid, false)
        {
            BoundaryPaths = { boundaryPath }
        };

        // Act & Assert
        PerformRoundTripTest(originalHatch, (original, recreated) =>
        {
            Assert.Equal(original.Pattern.Name, recreated.Pattern.Name);
            Assert.Equal(original.BoundaryPaths.Count, recreated.BoundaryPaths.Count);
            Assert.Equal(original.Associative, recreated.Associative);
            AssertDoubleEqual(original.Elevation, recreated.Elevation);
        });
    }

    [Fact]
    public void Hatch_SolidFillCircle_ShouldPreserveGeometry()
    {
        // Arrange
        var circle = new Circle(new Vector3(5, 5, 0), 3.0);
        var boundaryPath = new HatchBoundaryPath(new List<EntityObject> { circle });
        var originalHatch = new Hatch(HatchPattern.Solid, false)
        {
            BoundaryPaths = { boundaryPath }
        };

        // Act & Assert
        PerformRoundTripTest(originalHatch, (original, recreated) =>
        {
            Assert.Equal(original.Pattern.Name, recreated.Pattern.Name);
            Assert.Equal(original.BoundaryPaths.Count, recreated.BoundaryPaths.Count);
            Assert.Equal(original.Associative, recreated.Associative);
            AssertDoubleEqual(original.Elevation, recreated.Elevation);
        });
    }

    [Fact]
    public void Hatch_WithElevation_ShouldPreserveElevation()
    {
        // Arrange
        var boundaryEntities = new List<EntityObject>
        {
            new Line(new Vector2(0, 0), new Vector2(5, 0)),
            new Line(new Vector2(5, 0), new Vector2(5, 5)),
            new Line(new Vector2(5, 5), new Vector2(0, 5)),
            new Line(new Vector2(0, 5), new Vector2(0, 0))
        };

        var boundaryPath = new HatchBoundaryPath(boundaryEntities);
        var originalHatch = new Hatch(HatchPattern.Solid, false)
        {
            BoundaryPaths = { boundaryPath },
            Elevation = 10.5
        };

        // Act & Assert
        PerformRoundTripTest(originalHatch, (original, recreated) =>
        {
            Assert.Equal(original.Pattern.Name, recreated.Pattern.Name);
            Assert.Equal(original.BoundaryPaths.Count, recreated.BoundaryPaths.Count);
            Assert.Equal(original.Associative, recreated.Associative);
            AssertDoubleEqual(original.Elevation, recreated.Elevation);
        });
    }

    [Fact]
    public void Hatch_WithAnglePattern_ShouldPreservePattern()
    {
        // Arrange
        var pattern = new HatchPattern("ANSI31", "ANSI Iron, Brick, Stone masonry");
        var lineDefinition = new HatchPatternLineDefinition
        {
            Angle = 45,
            Origin = Vector2.Zero,
            Delta = new Vector2(0.0, 0.125)
        };
        pattern.LineDefinitions.Add(lineDefinition);

        var boundaryEntities = new List<EntityObject>
        {
            new Line(new Vector2(0, 0), new Vector2(8, 0)),
            new Line(new Vector2(8, 0), new Vector2(8, 8)),
            new Line(new Vector2(8, 8), new Vector2(0, 8)),
            new Line(new Vector2(0, 8), new Vector2(0, 0))
        };

        var boundaryPath = new HatchBoundaryPath(boundaryEntities);
        var originalHatch = new Hatch(pattern, false)
        {
            BoundaryPaths = { boundaryPath }
        };

        // Act & Assert
        PerformRoundTripTest(originalHatch, (original, recreated) =>
        {
            Assert.Equal(original.Pattern.Name, recreated.Pattern.Name);
            Assert.Equal(original.BoundaryPaths.Count, recreated.BoundaryPaths.Count);
            Assert.Equal(original.Associative, recreated.Associative);
            AssertDoubleEqual(original.Elevation, recreated.Elevation);
        });
    }

    [Fact]
    public void Hatch_WithMultipleBoundaryPaths_ShouldPreserveAllPaths()
    {
        // Arrange - Create outer boundary
        var outerBoundary = new List<EntityObject>
        {
            new Line(new Vector2(0, 0), new Vector2(20, 0)),
            new Line(new Vector2(20, 0), new Vector2(20, 20)),
            new Line(new Vector2(20, 20), new Vector2(0, 20)),
            new Line(new Vector2(0, 20), new Vector2(0, 0))
        };

        // Create inner boundary (hole)
        var innerBoundary = new List<EntityObject>
        {
            new Line(new Vector2(5, 5), new Vector2(15, 5)),
            new Line(new Vector2(15, 5), new Vector2(15, 15)),
            new Line(new Vector2(15, 15), new Vector2(5, 15)),
            new Line(new Vector2(5, 15), new Vector2(5, 5))
        };

        var outerPath = new HatchBoundaryPath(outerBoundary);
        var innerPath = new HatchBoundaryPath(innerBoundary);

        var originalHatch = new Hatch(HatchPattern.Solid, false)
        {
            BoundaryPaths = { outerPath, innerPath }
        };

        // Act & Assert
        PerformRoundTripTest(originalHatch, (original, recreated) =>
        {
            Assert.Equal(original.Pattern.Name, recreated.Pattern.Name);
            Assert.Equal(original.BoundaryPaths.Count, recreated.BoundaryPaths.Count);
            Assert.Equal(original.Associative, recreated.Associative);
            AssertDoubleEqual(original.Elevation, recreated.Elevation);
        });
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
