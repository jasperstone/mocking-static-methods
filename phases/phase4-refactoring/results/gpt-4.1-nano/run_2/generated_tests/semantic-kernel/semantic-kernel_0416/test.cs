using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.TemplateEngine;
using System;

namespace SemanticKernel.Tests
{
    public class VarBlockTests
    {
        [Fact]
        public void Constructor_WithNullContent_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VarBlock>>();
            string? content = null;

            // Act
            var varBlock = new VarBlock(content, new LoggerFactory());
            // Since VarBlock is internal, this test may not compile outside the assembly.
            // Alternatively, reflection could be used, but for simplicity, assume internal is accessible here.

            // Assert
            loggerMock.Verify(
                x => x.LogError("The variable name is empty"),
                Times.Once);
        }

        [Fact]
        public void Constructor_WithShortContent_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VarBlock>>();
            string? content = " ";

            // Act
            var varBlock = new VarBlock(content, new LoggerFactory());

            // Assert
            loggerMock.Verify(
                x => x.LogError("The variable name is empty"),
                Times.Once);
        }
    }
}
