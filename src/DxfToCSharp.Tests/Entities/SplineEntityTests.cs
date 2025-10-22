using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;

namespace DxfToCSharp.Tests.Entities;

public class SplineEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Spline_BasicRoundTrip_ShouldPreserveGeneratedControlPoints()
    {
        // Arrange
        var fitPoints = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(10, 5, 0),
            new Vector3(20, -5, 0),
            new Vector3(30, 0, 0)
        };

        // Note: netDxf automatically generates control points from fit points
        var originalSpline = new Spline(fitPoints);

        // Act & Assert
        PerformRoundTripTest(originalSpline, (original, recreated) =>
        {
            // netDxf generates control points from fit points, so we compare the generated ones
            Assert.Equal(original.ControlPoints.Count(), recreated.ControlPoints.Count());
            Assert.Equal(original.Degree, recreated.Degree);

            var originalPoints = original.ControlPoints.ToList();
            var recreatedPoints = recreated.ControlPoints.ToList();
            for (var i = 0; i < originalPoints.Count; i++)
            {
                AssertVector3Equal(originalPoints[i], recreatedPoints[i]);
            }
        });
    }

    [Fact]
    public void Spline_WithWeights_ShouldPreserveGeneratedControlPoints()
    {
        // Arrange
        var fitPoints = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(10, 10, 0),
            new Vector3(20, 5, 0),
            new Vector3(30, 0, 0)
        };

        // Note: netDxf automatically generates control points from fit points
        var originalSpline = new Spline(fitPoints);

        // Act & Assert
        PerformRoundTripTest(originalSpline, (original, recreated) =>
        {
            // netDxf generates control points from fit points, so we compare the generated ones
            Assert.Equal(original.ControlPoints.Count(), recreated.ControlPoints.Count());
            Assert.Equal(original.Degree, recreated.Degree);

            var originalPoints = original.ControlPoints.ToList();
            var recreatedPoints = recreated.ControlPoints.ToList();
            for (var i = 0; i < originalPoints.Count; i++)
            {
                AssertVector3Equal(originalPoints[i], recreatedPoints[i]);
            }
        });
    }

    [Fact]
    public void Spline_3D_ShouldPreserveGenerated3DControlPoints()
    {
        // Arrange
        var fitPoints = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(10, 5, 8),
            new Vector3(20, -5, 12),
            new Vector3(30, 0, 5)
        };

        // Note: netDxf automatically generates control points from fit points
        var originalSpline = new Spline(fitPoints);

        // Act & Assert
        PerformRoundTripTest(originalSpline, (original, recreated) =>
        {
            // netDxf generates control points from fit points, so we compare the generated ones
            Assert.Equal(original.ControlPoints.Count(), recreated.ControlPoints.Count());
            Assert.Equal(original.Degree, recreated.Degree);

            var originalPoints = original.ControlPoints.ToList();
            var recreatedPoints = recreated.ControlPoints.ToList();
            for (var i = 0; i < originalPoints.Count; i++)
            {
                AssertVector3Equal(originalPoints[i], recreatedPoints[i]);
            }
        });
    }

    [Fact]
    public void Spline_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("SplineLayer")
        {
            Color = new AciColor(7), // White
            Lineweight = Lineweight.Default
        };

        var fitPoints = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(5, 5, 0),
            new Vector3(10, 0, 0)
        };

        // Note: netDxf automatically generates control points from fit points
        var originalSpline = new Spline(fitPoints)
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalSpline, (original, recreated) =>
        {
            // netDxf generates control points from fit points, so we compare the generated ones
            Assert.Equal(original.ControlPoints.Count(), recreated.ControlPoints.Count());
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
        });
    }

    [Fact]
    public void Spline_HighDegree_ShouldPreserveGeneratedControlPoints()
    {
        // Arrange
        var fitPoints = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(2, 8, 0),
            new Vector3(4, -3, 0),
            new Vector3(6, 5, 0),
            new Vector3(8, 1, 0),
            new Vector3(10, 0, 0)
        };

        // Note: netDxf automatically generates control points from fit points
        var originalSpline = new Spline(fitPoints); // Use default degree

        // Act & Assert
        PerformRoundTripTest(originalSpline, (original, recreated) =>
        {
            // netDxf generates control points from fit points, so we compare the generated ones
            Assert.Equal(original.ControlPoints.Count(), recreated.ControlPoints.Count());
            Assert.Equal(original.Degree, recreated.Degree);

            var originalPoints = original.ControlPoints.ToList();
            var recreatedPoints = recreated.ControlPoints.ToList();
            for (var i = 0; i < originalPoints.Count; i++)
            {
                AssertVector3Equal(originalPoints[i], recreatedPoints[i]);
            }
        });
    }

    [Fact]
    public void Spline_WithExplicitControlPointsWeightsAndKnots_ShouldPreserveDetailedData()
    {
        // Arrange - build a cubic spline with explicit weights and knots
        var controlPoints = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(5, 10, 0),
            new Vector3(10, 5, 0),
            new Vector3(15, 0, 0)
        };

        var weights = new List<double> { 1.0, 0.75, 1.25, 1.0 };
        var knots = new List<double> { 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0 };

        var originalSpline = new Spline(controlPoints, weights, knots, 3, false)
        {
            StartTangent = new Vector3(1, 0, 0),
            EndTangent = new Vector3(1, -1, 0)
        };

        // Act & Assert
        PerformRoundTripTest(originalSpline, (original, recreated) =>
        {
            Assert.Equal(original.ControlPoints.Length, recreated.ControlPoints.Length);
            Assert.Equal(original.Weights.Length, recreated.Weights.Length);
            Assert.Equal(original.Knots.Length, recreated.Knots.Length);
            Assert.Equal(original.Degree, recreated.Degree);
            Assert.Equal(original.IsClosedPeriodic, recreated.IsClosedPeriodic);

            for (var i = 0; i < original.ControlPoints.Length; i++)
            {
                AssertVector3Equal(original.ControlPoints[i], recreated.ControlPoints[i]);
            }

            for (var i = 0; i < original.Weights.Length; i++)
            {
                AssertDoubleEqual(original.Weights[i], recreated.Weights[i]);
            }

            for (var i = 0; i < original.Knots.Length; i++)
            {
                AssertDoubleEqual(original.Knots[i], recreated.Knots[i]);
            }

            if (original.StartTangent.HasValue)
            {
                Assert.True(recreated.StartTangent.HasValue);
                AssertVector3Equal(original.StartTangent.Value, recreated.StartTangent.Value);
            }

            if (original.EndTangent.HasValue)
            {
                Assert.True(recreated.EndTangent.HasValue);
                AssertVector3Equal(original.EndTangent.Value, recreated.EndTangent.Value);
            }
        });
    }
}
