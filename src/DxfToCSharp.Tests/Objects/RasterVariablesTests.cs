using DxfToCSharp.Core;
using netDxf;
using netDxf.Objects;
using netDxf.Units;

namespace DxfToCSharp.Tests.Objects
{
    public class RasterVariablesTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly DxfCodeGenerator _generator;

        public RasterVariablesTests()
        {
            _tempDirectory = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);
            _generator = new DxfCodeGenerator();
        }

        [Fact]
        public void RasterVariables_GenerationOptions_ShouldBeRespected()
        {
            // Arrange
            var doc = new DxfDocument();
            doc.RasterVariables.DisplayFrame = false; // Set non-default value to ensure generation
            var options = new DxfCodeGenerationOptions
            {
                GenerateRasterVariablesObjects = true
            };

            // Act
            var generatedCode = _generator.Generate(doc, null, null, options);

            // Assert
            Assert.NotNull(generatedCode);
            Assert.Contains("doc.RasterVariables", generatedCode);
        }

        [Fact]
        public void RasterVariables_WithDetailedComments_ShouldIncludeComments()
        {
            // Arrange
            var doc = new DxfDocument();
            var options = new DxfCodeGenerationOptions
            {
                GenerateRasterVariablesObjects = true,
                GenerateDetailedComments = true
            };

            // Act
            var generatedCode = _generator.Generate(doc, null, null, options);

            // Assert
            Assert.NotNull(generatedCode);
            Assert.Contains("RasterVariables uses default values", generatedCode);
            Assert.Contains("//", generatedCode); // Should contain comments
        }

        [Fact]
        public void RasterVariables_WithoutDetailedComments_ShouldNotIncludeDetailedComments()
        {
            // Arrange
            var doc = new DxfDocument();
            doc.RasterVariables.DisplayFrame = false; // Set non-default value to ensure generation
            var options = new DxfCodeGenerationOptions
            {
                GenerateRasterVariablesObjects = true,
                GenerateDetailedComments = false
            };

            // Act
            var generatedCode = _generator.Generate(doc, null, null, options);

            // Assert
            Assert.NotNull(generatedCode);
            Assert.Contains("doc.RasterVariables.DisplayFrame = false;", generatedCode);
            // Should have minimal comments when detailed comments are disabled
            Assert.DoesNotContain("Raster Variables (", generatedCode);
        }

        [Fact]
        public void RasterVariables_WithDefaultValues_ShouldGenerateComment()
        {
            // Arrange
            var doc = new DxfDocument();
            // RasterVariables should have default values: DisplayFrame=true, DisplayQuality=High, Units=Unitless
            var options = new DxfCodeGenerationOptions
            {
                GenerateRasterVariablesObjects = true,
                GenerateDetailedComments = true
            };

            // Act
            var generatedCode = _generator.Generate(doc, null, null, options);

            // Assert
            Assert.NotNull(generatedCode);
            Assert.Contains("RasterVariables uses default values", generatedCode);
        }

        [Fact]
        public void RasterVariables_WithNonDefaultValues_ShouldGeneratePropertyAssignments()
        {
            // Arrange
            var doc = new DxfDocument();
            doc.RasterVariables.DisplayFrame = false;
            doc.RasterVariables.DisplayQuality = ImageDisplayQuality.Draft;
            doc.RasterVariables.Units = ImageUnits.Millimeters;

            var options = new DxfCodeGenerationOptions
            {
                GenerateRasterVariablesObjects = true,
                GenerateDetailedComments = true
            };

            // Act
            var generatedCode = _generator.Generate(doc, null, null, options);

            // Assert
            Assert.NotNull(generatedCode);
            Assert.Contains("doc.RasterVariables.DisplayFrame = false;", generatedCode);
            Assert.Contains("doc.RasterVariables.DisplayQuality = ImageDisplayQuality.Draft;", generatedCode);
            Assert.Contains("doc.RasterVariables.Units = ImageUnits.Millimeters;", generatedCode);
        }

        [Fact]
        public void RasterVariables_GenerationDisabled_ShouldNotGenerateCode()
        {
            // Arrange
            var doc = new DxfDocument();
            doc.RasterVariables.DisplayFrame = false;
            doc.RasterVariables.DisplayQuality = ImageDisplayQuality.Draft;

            var options = new DxfCodeGenerationOptions
            {
                GenerateRasterVariablesObjects = false
            };

            // Act
            var generatedCode = _generator.Generate(doc, null, null, options);

            // Assert
            Assert.NotNull(generatedCode);
            // When RasterVariables generation is disabled, no RasterVariables-related code should be generated
            Assert.DoesNotContain("doc.RasterVariables.DisplayFrame", generatedCode);
            Assert.DoesNotContain("doc.RasterVariables.DisplayQuality", generatedCode);
            Assert.DoesNotContain("doc.RasterVariables.Units", generatedCode);
        }

        [Fact]
        public void RasterVariables_PartialNonDefaultValues_ShouldGenerateOnlyChangedProperties()
        {
            // Arrange
            var doc = new DxfDocument();
            doc.RasterVariables.DisplayFrame = false; // Non-default
                                                      // DisplayQuality and Units remain default

            var options = new DxfCodeGenerationOptions
            {
                GenerateRasterVariablesObjects = true,
                GenerateDetailedComments = false
            };

            // Act
            var generatedCode = _generator.Generate(doc, null, null, options);

            // Assert
            Assert.NotNull(generatedCode);
            Assert.Contains("doc.RasterVariables.DisplayFrame = false;", generatedCode);
            Assert.DoesNotContain("doc.RasterVariables.DisplayQuality", generatedCode);
            Assert.DoesNotContain("doc.RasterVariables.Units", generatedCode);
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
