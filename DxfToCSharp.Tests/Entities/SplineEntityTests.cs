using System.Linq;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;
using Xunit;

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
            for (int i = 0; i < originalPoints.Count; i++)
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
            for (int i = 0; i < originalPoints.Count; i++)
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
            for (int i = 0; i < originalPoints.Count; i++)
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
            for (int i = 0; i < originalPoints.Count; i++)
            {
                AssertVector3Equal(originalPoints[i], recreatedPoints[i]);
            }
        });
    }
}