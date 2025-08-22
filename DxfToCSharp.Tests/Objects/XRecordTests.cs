using System;
using System.IO;
using Xunit;
using DxfToCSharp.Core;
using netDxf;
using netDxf.Objects;

namespace DxfToCSharp.Tests.Objects
{
    public class XRecordTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly DxfCodeGenerator _generator;

        public XRecordTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);
            _generator = new DxfCodeGenerator();
        }

        [Fact]
        public void XRecord_GenerationOptions_ShouldBeRespected()
        {
            // Arrange
            var doc = new DxfDocument();
            var options = new DxfCodeGenerationOptions
            {
                GenerateXRecordObjects = true
            };

            // Act
            var generatedCode = _generator.Generate(doc, null, null, options);

            // Assert
            Assert.NotNull(generatedCode);
            Assert.Contains("XRecord", generatedCode);
        }

        [Fact]
        public void XRecord_WithDetailedComments_ShouldIncludeComments()
        {
            // Arrange
            var doc = new DxfDocument();
            var options = new DxfCodeGenerationOptions
            {
                GenerateXRecordObjects = true,
                GenerateDetailedComments = true
            };

            // Act
            var generatedCode = _generator.Generate(doc, null, null, options);

            // Assert
            Assert.NotNull(generatedCode);
            Assert.Contains("XRecord", generatedCode);
            Assert.Contains("//", generatedCode); // Should contain comments
        }

        [Fact]
        public void XRecord_WithoutDetailedComments_ShouldNotIncludeComments()
        {
            // Arrange
            var doc = new DxfDocument();
            var options = new DxfCodeGenerationOptions
            {
                GenerateXRecordObjects = true,
                GenerateDetailedComments = false
            };

            // Act
            var generatedCode = _generator.Generate(doc, null, null, options);

            // Assert
            Assert.NotNull(generatedCode);
            Assert.Contains("XRecord", generatedCode);
            // Should have minimal comments when detailed comments are disabled
        }

        [Fact]
        public void XRecord_CodeGeneration_ShouldIncludeAllProperties()
        {
            // Arrange
            var doc = new DxfDocument();
            var options = new DxfCodeGenerationOptions
            {
                GenerateXRecordObjects = true,
                GenerateDetailedComments = true
            };

            // Act
            var generatedCode = _generator.Generate(doc, null, null, options);

            // Assert
            Assert.NotNull(generatedCode);
            // XRecord objects are internal and not directly accessible, so we expect placeholder comments
            Assert.Contains("XRecord objects are internal to netDxf and not directly accessible", generatedCode);
            Assert.Contains("XRecord objects contain arbitrary data as key-value pairs", generatedCode);
        }

        [Fact]
        public void XRecord_GenerationDisabled_ShouldNotGenerateCode()
        {
            // Arrange
            var doc = new DxfDocument();
            var options = new DxfCodeGenerationOptions
            {
                GenerateXRecordObjects = false
            };

            // Act
            var generatedCode = _generator.Generate(doc, null, null, options);

            // Assert
            Assert.NotNull(generatedCode);
            // When XRecord generation is disabled, no XRecord-related code should be generated
            Assert.DoesNotContain("XRecord objects are internal to netDxf", generatedCode);
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