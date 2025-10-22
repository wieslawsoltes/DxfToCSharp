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
    public void Hatch_CustomPattern_ShouldPreservePatternDefinition()
    {
        // Arrange
        var pattern = new HatchPattern("CustomPattern", "Custom angled pattern")
        {
            Type = HatchType.Custom,
            Angle = 30.0,
            Scale = 2.0,
            Origin = new Vector2(1.5, -2.5)
        };

        var lineDefinition = new HatchPatternLineDefinition
        {
            Angle = 90.0,
            Origin = new Vector2(0.5, 0.25),
            Delta = new Vector2(0.1, 0.4)
        };
        lineDefinition.DashPattern.AddRange(new[] { 0.5, -0.25, 0.1 });
        pattern.LineDefinitions.Add(lineDefinition);

        var boundaryEntities = new List<EntityObject>
        {
            new Circle(new Vector3(0, 0, 0), 10.0)
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
            Assert.True(
                recreated.Pattern.Type == original.Pattern.Type ||
                recreated.Pattern.Type == HatchType.UserDefined,
                $"Expected pattern type {original.Pattern.Type} or UserDefined but got {recreated.Pattern.Type}");
            Assert.Equal(original.Pattern.LineDefinitions.Count, recreated.Pattern.LineDefinitions.Count);
            AssertDoubleEqual(original.Pattern.Angle, recreated.Pattern.Angle);
            AssertDoubleEqual(original.Pattern.Scale, recreated.Pattern.Scale);
            AssertVector2Equal(original.Pattern.Origin, recreated.Pattern.Origin);

            var originalDef = original.Pattern.LineDefinitions[0];
            var recreatedDef = recreated.Pattern.LineDefinitions[0];
            AssertDoubleEqual(originalDef.Angle, recreatedDef.Angle);
            AssertVector2Equal(originalDef.Origin, recreatedDef.Origin);
            AssertVector2Equal(originalDef.Delta, recreatedDef.Delta);
            Assert.Equal(originalDef.DashPattern.Count, recreatedDef.DashPattern.Count);
            for (var i = 0; i < originalDef.DashPattern.Count; i++)
            {
                AssertDoubleEqual(originalDef.DashPattern[i], recreatedDef.DashPattern[i]);
            }
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

    public override void Dispose()
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

        base.Dispose();
    }
}
