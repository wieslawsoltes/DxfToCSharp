using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Tables;

namespace DxfToCSharp.Tests.Tables;

public class ApplicationRegistryTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void ApplicationRegistry_BasicProperties_ShouldRoundTrip()
    {
        // Arrange
        var originalAppReg = new ApplicationRegistry("TestApp");

        // Act & Assert
        PerformRoundTripTest(originalAppReg, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
            Assert.False(loaded.IsReserved);
        });
    }

    [Fact]
    public void ApplicationRegistry_DefaultApplication_ShouldRoundTrip()
    {
        // Arrange
        var originalAppReg = ApplicationRegistry.Default;

        // Act & Assert
        PerformRoundTripTest(originalAppReg, (_, loaded) =>
        {
            Assert.Equal(ApplicationRegistry.DefaultName, loaded.Name);
            Assert.True(loaded.IsReserved);
        });
    }

    [Fact]
    public void ApplicationRegistry_CustomName_ShouldRoundTrip()
    {
        // Arrange
        var originalAppReg = new ApplicationRegistry("MyCustomApp");

        // Act & Assert
        PerformRoundTripTest(originalAppReg, (_, loaded) =>
        {
            Assert.Equal("MyCustomApp", loaded.Name);
            Assert.False(loaded.IsReserved);
        });
    }

    [Fact]
    public void ApplicationRegistry_WithXData_ShouldRoundTrip()
    {
        // Arrange
        var appReg = new ApplicationRegistry("TestApp");
        var originalAppReg = new ApplicationRegistry("AppWithXData");

        var xdata = new XData(appReg);
        xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "Test XData"));
        originalAppReg.XData.Add(xdata);

        // Act & Assert
        PerformRoundTripTest(originalAppReg, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
            Assert.False(loaded.IsReserved);
            Assert.Single(loaded.XData);
            Assert.True(loaded.XData.ContainsAppId(appReg.Name));
        });
    }

    [Fact]
    public void ApplicationRegistry_LongName_ShouldRoundTrip()
    {
        // Arrange
        var longName = "VeryLongApplicationRegistryNameThatExceedsNormalLimits";
        var originalAppReg = new ApplicationRegistry(longName);

        // Act & Assert
        PerformRoundTripTest(originalAppReg, (_, loaded) =>
        {
            Assert.Equal(longName, loaded.Name);
            Assert.False(loaded.IsReserved);
        });
    }

    private void PerformRoundTripTest(ApplicationRegistry originalAppReg, Action<ApplicationRegistry, ApplicationRegistry> assertions)
    {
        // Create a DXF document and add the application registry
        var originalDoc = new DxfDocument();
        originalDoc.ApplicationRegistries.Add(originalAppReg);

        // Save to memory stream
        using var stream = new MemoryStream();
        originalDoc.Save(stream);
        stream.Position = 0;

        // Load from memory stream
        var loadedDoc = DxfDocument.Load(stream);
        var loadedAppReg = loadedDoc.ApplicationRegistries[originalAppReg.Name];

        // Verify basic properties
        Assert.NotNull(loadedAppReg);

        // Run custom assertions
        assertions(originalAppReg, loadedAppReg);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
