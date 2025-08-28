using DxfToCSharp.Tests.Infrastructure;
using netDxf;
using netDxf.Entities;
using netDxf.Objects;

namespace DxfToCSharp.Tests.Objects;

public class GroupTests : RoundTripTestBase, IDisposable
{
    [Fact]
    public void Group_BasicProperties_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var group = new Group("TestGroup");

        // Add some entities to the group
        var line1 = new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0));
        var line2 = new Line(new Vector3(10, 0, 0), new Vector3(20, 10, 0));
        var circle = new Circle(new Vector3(5, 5, 0), 2.5);

        originalDoc.Entities.Add(line1);
        originalDoc.Entities.Add(line2);
        originalDoc.Entities.Add(circle);

        group.Entities.Add(line1);
        group.Entities.Add(line2);
        group.Entities.Add(circle);

        originalDoc.Groups.Add(group);

        // Act & Assert
        PerformObjectRoundTripTest(originalDoc, group, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
            Assert.Equal(original.Entities.Count, loaded.Entities.Count);
            Assert.Equal(original.Description, loaded.Description);
            Assert.Equal(original.IsSelectable, loaded.IsSelectable);

            // Verify that the group contains the expected entity types
            Assert.Contains(loaded.Entities, e => e is Line);
            Assert.Contains(loaded.Entities, e => e is Circle);
        });
    }

    [Fact]
    public void Group_EmptyGroup_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var group = new Group("EmptyGroup");
        originalDoc.Groups.Add(group);

        // Add at least one entity to make the document valid
        originalDoc.Entities.Add(new Line(Vector3.Zero, new Vector3(1, 1, 0)));

        // Act & Assert
        PerformObjectRoundTripTest(originalDoc, group, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
            Assert.Empty(loaded.Entities);
        });
    }

    [Fact]
    public void Group_WithSpecialCharacters_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var group = new Group("Test_Group-123");
        originalDoc.Groups.Add(group);

        // Add at least one entity to make the document valid
        originalDoc.Entities.Add(new Line(Vector3.Zero, new Vector3(1, 1, 0)));

        // Act & Assert
        PerformObjectRoundTripTest(originalDoc, group, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
            Assert.Equal(original.Description, loaded.Description);
            Assert.Equal(original.IsSelectable, loaded.IsSelectable);
        });
    }

    [Fact]
    public void Group_WithDescriptionAndSelectability_ShouldRoundTrip()
    {
        // Arrange
        var originalDoc = new DxfDocument();
        var group = new Group("GroupWithProperties")
        {
            Description = "Test group description",
            IsSelectable = false
        };

        // Add some entities to the group
        var line = new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0));
        originalDoc.Entities.Add(line);
        group.Entities.Add(line);

        originalDoc.Groups.Add(group);

        // Act & Assert
        PerformObjectRoundTripTest(originalDoc, group, (original, loaded) =>
        {
            Assert.Equal(original.Name, loaded.Name);
            Assert.Equal(original.Description, loaded.Description);
            Assert.Equal(original.IsSelectable, loaded.IsSelectable);
            Assert.Equal(original.Entities.Count, loaded.Entities.Count);
        });
    }

    private void PerformObjectRoundTripTest<T>(DxfDocument originalDoc, T originalObject, Action<T, T> validator) where T : class
    {
        // Step 1: Save original document to DXF file
        var originalDxfPath = Path.Join(_tempDirectory, "original.dxf");
        originalDoc.Save(originalDxfPath);

        // Step 2: Load DXF file
        var loadedDoc = DxfDocument.Load(originalDxfPath);
        Assert.NotNull(loadedDoc);

        // Step 3: Find the corresponding object in the loaded document
        T? loadedObject = null;
        if (typeof(T) == typeof(Group))
        {
            var originalGroup = originalObject as Group;
            loadedObject = loadedDoc.Groups.FirstOrDefault(g => g.Name == originalGroup?.Name) as T;
        }

        Assert.NotNull(loadedObject);

        // Step 4: Generate C# code from the loaded document
        var generatedCode = _generator.Generate(loadedDoc, originalDxfPath);
        Assert.NotNull(generatedCode);
        Assert.NotEmpty(generatedCode);

        // Step 5: Compile and execute the generated code
        var recreatedDoc = CompileAndExecuteCode(generatedCode);
        Assert.NotNull(recreatedDoc);

        // Step 6: Find the corresponding object in the recreated document
        T? recreatedObject = null;
        if (typeof(T) == typeof(Group))
        {
            var originalGroup = originalObject as Group;
            recreatedObject = recreatedDoc.Groups.FirstOrDefault(g => g.Name == originalGroup?.Name) as T;
        }

        Assert.NotNull(recreatedObject);

        // Step 7: Validate the recreated object matches the original
        validator(originalObject, recreatedObject);

        // Step 8: Save recreated document and verify it can be loaded
        var recreatedDxfPath = Path.Join(_tempDirectory, "recreated.dxf");
        recreatedDoc.Save(recreatedDxfPath);
        var finalDoc = DxfDocument.Load(recreatedDxfPath);
        Assert.NotNull(finalDoc);
    }

    public override void Dispose()
    {
        // Cleanup if needed
        base.Dispose();
    }
}
