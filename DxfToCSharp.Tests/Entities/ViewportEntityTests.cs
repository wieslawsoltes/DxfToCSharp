using netDxf;
using netDxf.Entities;
using DxfToCSharp.Tests.Infrastructure;

namespace DxfToCSharp.Tests.Entities;

public class ViewportEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Viewport_BasicRoundTrip_ShouldPreserveViewportProperties()
    {
        // Arrange
        var originalViewport = new Viewport();
        originalViewport.Center = new Vector3(50, 50, 0);
        originalViewport.Width = 100;
        originalViewport.Height = 80;
        originalViewport.ViewCenter = new Vector2(25, 25);
        originalViewport.ViewHeight = 40;
        originalViewport.LensLength = 50.0;
        originalViewport.TwistAngle = 15.0;
        originalViewport.CircleZoomPercent = 100;
        originalViewport.Status = ViewportStatusFlags.GridMode;

        // Act & Assert
        PerformRoundTripTest(originalViewport, (original, recreated) =>
        {
            // Note: Id property is internal, cannot be tested directly
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Width, recreated.Width);
            AssertDoubleEqual(original.Height, recreated.Height);
            AssertVector2Equal(original.ViewCenter, recreated.ViewCenter);
            AssertDoubleEqual(original.ViewHeight, recreated.ViewHeight);
            AssertDoubleEqual(original.LensLength, recreated.LensLength);
            AssertDoubleEqual(original.TwistAngle, recreated.TwistAngle);
            Assert.Equal(original.CircleZoomPercent, recreated.CircleZoomPercent);
            Assert.Equal(original.Status, recreated.Status);
        });
    }

    [Fact]
    public void Viewport_WithDifferentProperties_ShouldPreserveDifferentProperties()
    {
        // Arrange
        var originalViewport = new Viewport();
        originalViewport.Center = new Vector3(0, 0, 0);
        originalViewport.Width = 200;
        originalViewport.Height = 150;
        originalViewport.ViewCenter = new Vector2(0, 0);
        originalViewport.ViewHeight = 100;
        originalViewport.LensLength = 35.0;
        originalViewport.TwistAngle = 0.0;
        originalViewport.CircleZoomPercent = 150;
        originalViewport.Status = ViewportStatusFlags.GridMode | ViewportStatusFlags.UcsIconVisibility;

        // Act & Assert
        PerformRoundTripTest(originalViewport, (original, recreated) =>
        {
            // Note: Id property is internal, cannot be tested directly
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Width, recreated.Width);
            AssertDoubleEqual(original.Height, recreated.Height);
            AssertVector2Equal(original.ViewCenter, recreated.ViewCenter);
            AssertDoubleEqual(original.ViewHeight, recreated.ViewHeight);
            AssertDoubleEqual(original.LensLength, recreated.LensLength);
            AssertDoubleEqual(original.TwistAngle, recreated.TwistAngle);
            Assert.Equal(original.CircleZoomPercent, recreated.CircleZoomPercent);
            Assert.Equal(original.Status, recreated.Status);
        });
    }

    [Fact]
    public void Viewport_WithViewTarget_ShouldPreserveViewTarget()
    {
        // Arrange
        var originalViewport = new Viewport();
        originalViewport.Center = new Vector3(100, 100, 0);
        originalViewport.Width = 50;
        originalViewport.Height = 50;
        originalViewport.ViewTarget = new Vector3(10, 10, 5);
        originalViewport.ViewDirection = new Vector3(0, 0, 1);
        originalViewport.ViewHeight = 25;

        // Act & Assert
        PerformRoundTripTest(originalViewport, (original, recreated) =>
        {
            // Assert.Equal(original.Id, recreated.Id); // Id is internal property
            AssertVector3Equal(original.Center, recreated.Center);
            AssertDoubleEqual(original.Width, recreated.Width);
            AssertDoubleEqual(original.Height, recreated.Height);
            AssertVector3Equal(original.ViewTarget, recreated.ViewTarget);
            AssertVector3Equal(original.ViewDirection, recreated.ViewDirection);
            AssertDoubleEqual(original.ViewHeight, recreated.ViewHeight);
        });
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}