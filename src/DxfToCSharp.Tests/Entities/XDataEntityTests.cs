using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;

namespace DxfToCSharp.Tests.Entities;

public class XDataEntityTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Line_WithXData_ShouldRoundTrip()
    {
        // Arrange
        var doc = new DxfDocument();
        var line = new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0));

        // Create application registry and XData
        var appReg = doc.ApplicationRegistries.Add(new ApplicationRegistry("MY_APP"));
        var xdata = new XData(appReg);
        xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "hello"));
        xdata.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short)123));
        xdata.XDataRecord.Add(new XDataRecord(XDataCode.Real, 3.14159));
        line.XData.Add(xdata);

        // Act & Assert
        PerformRoundTripTest(line, (original, recreated) =>
        {
            Assert.True(recreated.XData.ContainsAppId("MY_APP"));
            var rx = recreated.XData["MY_APP"];
            Assert.NotNull(rx);
            Assert.Equal(3, rx.XDataRecord.Count);
        });
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}

