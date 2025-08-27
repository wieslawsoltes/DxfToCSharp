using DxfToCSharp.Core;
using netDxf;

namespace DxfToCSharp.Tests.Objects
{
    public class DictionaryObjectTests : IDisposable
    {
        private readonly string _tempDirectory;

        public DictionaryObjectTests()
        {
            _tempDirectory = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);
        }

        [Fact]
        public void DictionaryObject_GenerationOptions_ShouldBeRespected()
        {
            // Arrange
            var doc = new DxfDocument();
            var options = new DxfCodeGenerationOptions
            {
                GenerateDictionaryObjects = true,
                GenerateDetailedComments = false
            };
            var generator = new DxfCodeGenerator();

            // Act
            var generatedCode = generator.Generate(doc, null, null, options);

            // Assert
            Assert.Contains("// Dictionary objects are internal to netDxf and not directly accessible", generatedCode);
        }

        [Fact]
        public void DictionaryObject_WithDetailedComments_ShouldIncludeComments()
        {
            // Arrange
            var doc = new DxfDocument();
            var options = new DxfCodeGenerationOptions
            {
                GenerateDictionaryObjects = true,
                GenerateDetailedComments = true
            };
            var generator = new DxfCodeGenerator();

            // Act
            var generatedCode = generator.Generate(doc, null, null, options);

            // Assert
            Assert.Contains("// Dictionary objects are internal to netDxf and not directly accessible", generatedCode);
            Assert.Contains("// Dictionary objects store key-value pairs for named objects", generatedCode);
            Assert.Contains("// They are used internally for organizing objects like layer states", generatedCode);
            Assert.Contains("// Example structure:", generatedCode);
            Assert.Contains("//   Handle: [object handle]", generatedCode);
            Assert.Contains("//   Entries: Dictionary<string, string> of handle/name pairs", generatedCode);
            Assert.Contains("//   IsHardOwner: [boolean indicating ownership type]", generatedCode);
            Assert.Contains("//   Cloning: [DictionaryCloningFlags]", generatedCode);
        }

        [Fact]
        public void DictionaryObject_WithoutDetailedComments_ShouldNotIncludeComments()
        {
            // Arrange
            var doc = new DxfDocument();
            var options = new DxfCodeGenerationOptions
            {
                GenerateDictionaryObjects = true,
                GenerateDetailedComments = false
            };
            var generator = new DxfCodeGenerator();

            // Act
            var generatedCode = generator.Generate(doc, null, null, options);

            // Assert
            Assert.Contains("// Dictionary objects are internal to netDxf and not directly accessible", generatedCode);
            Assert.DoesNotContain("// Dictionary objects store key-value pairs for named objects", generatedCode);
            Assert.DoesNotContain("// They are used internally for organizing objects like layer states", generatedCode);
            Assert.DoesNotContain("// Example structure:", generatedCode);
        }

        [Fact]
        public void DictionaryObject_CodeGeneration_ShouldIncludeAllProperties()
        {
            // Arrange
            var doc = new DxfDocument();
            var options = new DxfCodeGenerationOptions
            {
                GenerateDictionaryObjects = true,
                GenerateDetailedComments = true
            };
            var generator = new DxfCodeGenerator();

            // Act
            var generatedCode = generator.Generate(doc, null, null, options);

            // Assert
            // DictionaryObject is internal and not directly accessible, so we check for placeholder comments
            Assert.Contains("// Dictionary objects are internal to netDxf and not directly accessible", generatedCode);
            Assert.Contains("//   Entries: Dictionary<string, string> of handle/name pairs", generatedCode);
            Assert.Contains("//   IsHardOwner: [boolean indicating ownership type]", generatedCode);
            Assert.Contains("//   Cloning: [DictionaryCloningFlags]", generatedCode);
        }

        [Fact]
        public void DictionaryObject_GenerationDisabled_ShouldNotGenerateCode()
        {
            // Arrange
            var doc = new DxfDocument();
            var options = new DxfCodeGenerationOptions
            {
                GenerateDictionaryObjects = false,
                GenerateDetailedComments = true
            };
            var generator = new DxfCodeGenerator();

            // Act
            var generatedCode = generator.Generate(doc, null, null, options);

            // Assert
            Assert.DoesNotContain("// Dictionary objects are internal to netDxf and not directly accessible", generatedCode);
            Assert.DoesNotContain("// Dictionary objects store key-value pairs for named objects", generatedCode);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
    }
}
