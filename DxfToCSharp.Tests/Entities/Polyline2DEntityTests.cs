using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class Polyline2DEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Polyline2D_BasicOpenPolyline_ShouldPreserveVertices()
    {
        // Arrange
        var vertices = new List<Polyline2DVertex>
        {
            new Polyline2DVertex(0, 0),
            new Polyline2DVertex(10, 0),
            new Polyline2DVertex(10, 10),
            new Polyline2DVertex(0, 10)
        };
        var originalPolyline = new Polyline2D(vertices, false);

        // Act & Assert
        PerformRoundTripTest(originalPolyline, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.IsClosed, recreated.IsClosed);
            
            for (int i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(
                    new Vector3(original.Vertexes[i].Position.X, original.Vertexes[i].Position.Y, 0),
                    new Vector3(recreated.Vertexes[i].Position.X, recreated.Vertexes[i].Position.Y, 0));
                AssertDoubleEqual(original.Vertexes[i].Bulge, recreated.Vertexes[i].Bulge);
            }
        });
    }

    [Fact]
    public void Polyline2D_BasicClosedPolyline_ShouldPreserveClosedState()
    {
        // Arrange
        var vertices = new List<Polyline2DVertex>
        {
            new Polyline2DVertex(0, 0),
            new Polyline2DVertex(20, 0),
            new Polyline2DVertex(20, 20),
            new Polyline2DVertex(0, 20)
        };
        var originalPolyline = new Polyline2D(vertices, true);

        // Act & Assert
        PerformRoundTripTest(originalPolyline, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.IsClosed, recreated.IsClosed);
            
            for (int i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(
                    new Vector3(original.Vertexes[i].Position.X, original.Vertexes[i].Position.Y, 0),
                    new Vector3(recreated.Vertexes[i].Position.X, recreated.Vertexes[i].Position.Y, 0));
            }
        });
    }

    [Fact]
    public void Polyline2D_WithBulgeValues_ShouldPreserveBulges()
    {
        // Arrange
        var vertices = new List<Polyline2DVertex>
        {
            new Polyline2DVertex(0, 0) { Bulge = 0.5 },
            new Polyline2DVertex(10, 0) { Bulge = -0.3 },
            new Polyline2DVertex(10, 10) { Bulge = 1.0 },
            new Polyline2DVertex(0, 10) { Bulge = 0.0 }
        };
        var originalPolyline = new Polyline2D(vertices, false);

        // Act & Assert
        PerformRoundTripTest(originalPolyline, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            
            for (int i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(
                    new Vector3(original.Vertexes[i].Position.X, original.Vertexes[i].Position.Y, 0),
                    new Vector3(recreated.Vertexes[i].Position.X, recreated.Vertexes[i].Position.Y, 0));
                AssertDoubleEqual(original.Vertexes[i].Bulge, recreated.Vertexes[i].Bulge);
            }
        });
    }

    [Fact]
    public void Polyline2D_WithThickness_ShouldPreserveThickness()
    {
        // Arrange
        var vertices = new List<Polyline2DVertex>
        {
            new Polyline2DVertex(0, 0),
            new Polyline2DVertex(50, 0),
            new Polyline2DVertex(50, 50)
        };
        var originalPolyline = new Polyline2D(vertices, false)
        {
            Thickness = 5.5
        };

        // Act & Assert
        PerformRoundTripTest(originalPolyline, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            AssertDoubleEqual(original.Thickness, recreated.Thickness);
            
            for (int i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(
                    new Vector3(original.Vertexes[i].Position.X, original.Vertexes[i].Position.Y, 0),
                    new Vector3(recreated.Vertexes[i].Position.X, recreated.Vertexes[i].Position.Y, 0));
            }
        });
    }

    [Fact]
    public void Polyline2D_WithCustomLayer_ShouldPreserveLayerProperties()
    {
        // Arrange
        var customLayer = new Layer("PolylineLayer")
        {
            Color = new AciColor(4), // Cyan
            Lineweight = Lineweight.Default
        };
        
        var vertices = new List<Polyline2DVertex>
        {
            new Polyline2DVertex(0, 0),
            new Polyline2DVertex(30, 0),
            new Polyline2DVertex(30, 30),
            new Polyline2DVertex(0, 30)
        };
        var originalPolyline = new Polyline2D(vertices, true)
        {
            Layer = customLayer
        };

        // Act & Assert
        PerformRoundTripTest(originalPolyline, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.Layer.Name, recreated.Layer.Name);
            
            for (int i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(
                    new Vector3(original.Vertexes[i].Position.X, original.Vertexes[i].Position.Y, 0),
                    new Vector3(recreated.Vertexes[i].Position.X, recreated.Vertexes[i].Position.Y, 0));
            }
        });
    }

    [Fact]
    public void Polyline2D_WithNegativeCoordinates_ShouldPreserveNegativeValues()
    {
        // Arrange
        var vertices = new List<Polyline2DVertex>
        {
            new Polyline2DVertex(-10, -10),
            new Polyline2DVertex(-5, -15),
            new Polyline2DVertex(0, -10),
            new Polyline2DVertex(-5, -5)
        };
        var originalPolyline = new Polyline2D(vertices, true);

        // Act & Assert
        PerformRoundTripTest(originalPolyline, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            
            for (int i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(
                    new Vector3(original.Vertexes[i].Position.X, original.Vertexes[i].Position.Y, 0),
                    new Vector3(recreated.Vertexes[i].Position.X, recreated.Vertexes[i].Position.Y, 0));
            }
        });
    }

    [Fact]
    public void Polyline2D_WithPreciseCoordinates_ShouldPreservePrecision()
    {
        // Arrange
        var vertices = new List<Polyline2DVertex>
        {
            new Polyline2DVertex(0.123456789, 0.987654321),
            new Polyline2DVertex(10.111111111, 0.222222222),
            new Polyline2DVertex(10.333333333, 10.444444444)
        };
        var originalPolyline = new Polyline2D(vertices, false);

        // Act & Assert
        PerformRoundTripTest(originalPolyline, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            
            for (int i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(
                    new Vector3(original.Vertexes[i].Position.X, original.Vertexes[i].Position.Y, 0),
                    new Vector3(recreated.Vertexes[i].Position.X, recreated.Vertexes[i].Position.Y, 0),
                    1e-12);
            }
        });
    }

    [Fact]
    public void Polyline2D_WithCustomColor_ShouldPreserveColor()
    {
        // Arrange
        var vertices = new List<Polyline2DVertex>
        {
            new Polyline2DVertex(0, 0),
            new Polyline2DVertex(15, 0),
            new Polyline2DVertex(15, 15)
        };
        var originalPolyline = new Polyline2D(vertices, false)
        {
            Color = new AciColor(5) // Blue
        };

        // Act & Assert
        PerformRoundTripTest(originalPolyline, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.Color.Index, recreated.Color.Index);
            
            for (int i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(
                    new Vector3(original.Vertexes[i].Position.X, original.Vertexes[i].Position.Y, 0),
                    new Vector3(recreated.Vertexes[i].Position.X, recreated.Vertexes[i].Position.Y, 0));
            }
        });
    }

    [Fact]
    public void Polyline2D_ComplexShape_ShouldPreserveComplexGeometry()
    {
        // Arrange - Create a complex shape with multiple vertices and bulges
        var vertices = new List<Polyline2DVertex>
        {
            new Polyline2DVertex(0, 0) { Bulge = 0.0 },
            new Polyline2DVertex(10, 0) { Bulge = 0.414213562 }, // 90 degree arc
            new Polyline2DVertex(20, 10) { Bulge = 0.0 },
            new Polyline2DVertex(15, 20) { Bulge = -0.5 },
            new Polyline2DVertex(5, 25) { Bulge = 1.0 }, // 180 degree arc
            new Polyline2DVertex(-5, 15) { Bulge = 0.0 },
            new Polyline2DVertex(-2, 5) { Bulge = 0.2 }
        };
        var originalPolyline = new Polyline2D(vertices, true);

        // Act & Assert
        PerformRoundTripTest(originalPolyline, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.IsClosed, recreated.IsClosed);
            
            for (int i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(
                    new Vector3(original.Vertexes[i].Position.X, original.Vertexes[i].Position.Y, 0),
                    new Vector3(recreated.Vertexes[i].Position.X, recreated.Vertexes[i].Position.Y, 0));
                AssertDoubleEqual(original.Vertexes[i].Bulge, recreated.Vertexes[i].Bulge, 1e-12);
            }
        });
    }

    [Fact]
    public void Polyline2D_BasicRoundTrip_ShouldPreserveVertices()
    {
        // Arrange
        var vertices = new List<Polyline2DVertex>
        {
            new Polyline2DVertex(0, 0),
            new Polyline2DVertex(10, 0),
            new Polyline2DVertex(10, 10),
            new Polyline2DVertex(0, 10)
        };
        
        var originalPolyline2D = new Polyline2D(vertices, false);

        // Act & Assert
        PerformRoundTripTest(originalPolyline2D, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.IsClosed, recreated.IsClosed);
            
            for (int i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(
                    new Vector3(original.Vertexes[i].Position.X, original.Vertexes[i].Position.Y, 0),
                    new Vector3(recreated.Vertexes[i].Position.X, recreated.Vertexes[i].Position.Y, 0));
                AssertDoubleEqual(original.Vertexes[i].Bulge, recreated.Vertexes[i].Bulge);
            }
        });
    }

    [Fact]
    public void Polyline2D_ClosedWithBulges_ShouldPreserveBulgeValues()
    {
        // Arrange
        var vertices = new List<Polyline2DVertex>
        {
            new Polyline2DVertex(0, 0, 0.5),
            new Polyline2DVertex(20, 0, -0.3),
            new Polyline2DVertex(20, 20, 0.8),
            new Polyline2DVertex(0, 20, -0.2)
        };

        var originalPolyline2D = new Polyline2D(vertices, true);

        // Act & Assert
        PerformRoundTripTest(originalPolyline2D, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.IsClosed, recreated.IsClosed);
            
            for (int i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(
                    new Vector3(original.Vertexes[i].Position.X, original.Vertexes[i].Position.Y, 0),
                    new Vector3(recreated.Vertexes[i].Position.X, recreated.Vertexes[i].Position.Y, 0));
                AssertDoubleEqual(original.Vertexes[i].Bulge, recreated.Vertexes[i].Bulge);
            }
        });
    }

    [Fact]
    public void Polyline2D_WithElevation_ShouldPreserveElevation()
    {
        // Arrange
        var vertices = new List<Polyline2DVertex>
        {
            new Polyline2DVertex(0, 0),
            new Polyline2DVertex(10, 10),
            new Polyline2DVertex(20, 0)
        };

        var originalPolyline2D = new Polyline2D(vertices, false)
        {
            Elevation = 15.5
        };

        // Act & Assert
        PerformRoundTripTest(originalPolyline2D, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            Assert.Equal(original.IsClosed, recreated.IsClosed);
            AssertDoubleEqual(original.Elevation, recreated.Elevation);
        });
    }



    [Fact]
    public void Polyline2D_NegativeCoordinates_ShouldPreserveNegativeCoordinates()
    {
        // Arrange
        var vertices = new List<Polyline2DVertex>
        {
            new Polyline2DVertex(-10.5, -20.3, 0.7),
            new Polyline2DVertex(-5.2, -15.8, -0.4),
            new Polyline2DVertex(-2.1, -25.9, 0.2)
        };

        var originalPolyline2D = new Polyline2D(vertices, false);

        // Act & Assert
        PerformRoundTripTest(originalPolyline2D, (original, recreated) =>
        {
            Assert.Equal(original.Vertexes.Count, recreated.Vertexes.Count);
            
            for (int i = 0; i < original.Vertexes.Count; i++)
            {
                AssertVector3Equal(
                    new Vector3(original.Vertexes[i].Position.X, original.Vertexes[i].Position.Y, 0),
                    new Vector3(recreated.Vertexes[i].Position.X, recreated.Vertexes[i].Position.Y, 0));
                AssertDoubleEqual(original.Vertexes[i].Bulge, recreated.Vertexes[i].Bulge);
            }
        });
    }
}