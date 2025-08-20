using netDxf;
using netDxf.Tables;
using Xunit.Abstractions;

namespace DxfToCSharp.Tests.Tables;

public class UCSDebugTest
{
    private readonly ITestOutputHelper _output;
    private readonly string _tempDirectory;

    public UCSDebugTest(ITestOutputHelper output)
    {
        _output = output;
        _tempDirectory = Path.GetTempPath();
    }

    [Fact]
    public void Debug_UCS_SaveAndLoad()
    {
        _output.WriteLine("Testing UCS name validation...");
            
        // Test if WorldAlignedUCS is a valid name
        string testName = "WorldAlignedUCS";
        bool isValid = TableObject.IsValidName(testName);
        _output.WriteLine($"Is '{testName}' a valid name? {isValid}");
            
        // Try to create a UCS with this name
        UCS ucs = new UCS(testName);
        _output.WriteLine($"Successfully created UCS with name: {ucs.Name}");
            
        // Create a document and add the UCS
        DxfDocument doc = new DxfDocument();
        doc.UCSs.Add(ucs);
        _output.WriteLine($"Successfully added UCS to document. Count: {doc.UCSs.Count}");
            
        // Save and load
        var dxfPath = Path.Combine(_tempDirectory, "debug_ucs.dxf");
        doc.Save(dxfPath);
        _output.WriteLine($"Saved document to: {dxfPath}");
            
        var loadedDoc = DxfDocument.Load(dxfPath);
        _output.WriteLine($"Loaded document. UCS count: {loadedDoc.UCSs.Count}");
            
        foreach (var loadedUcs in loadedDoc.UCSs.Items)
        {
            _output.WriteLine($"Found UCS: '{loadedUcs.Name}'");
        }
            
        // Check if the UCS exists in the loaded document
        bool found = loadedDoc.UCSs.Names.Contains(testName);
        _output.WriteLine($"UCS '{testName}' found in loaded document: {found}");
            
        Assert.True(found, $"UCS '{testName}' should be found in the loaded document");
    }
}