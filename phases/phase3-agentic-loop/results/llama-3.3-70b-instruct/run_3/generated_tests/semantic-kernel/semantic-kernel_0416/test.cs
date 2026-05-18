using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine.Tests
{
    public class VarBlockTests
    {
        [Fact]
        public void Constructor_InvalidContent_LogsError()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var content = string.Empty;

            // Act
            var varBlock = new Microsoft.SemanticKernel.TemplateEngine.VarBlock(content, loggerFactory.Object);

            // Assert
            logger.Verify(l => l.LogError("The variable name is empty"), Times.Once);
        }

        [Fact]
        public void IsValid_InvalidContent_LogsError()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var content = string.Empty;
            var varBlock = new Microsoft.SemanticKernel.TemplateEngine.VarBlock(content, loggerFactory.Object);

            // Act
            var isValid = varBlock.IsValid(out var errorMsg);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(errorMsg);
            logger.Verify(l => l.LogError(errorMsg), Times.Once);
        }

        [Fact]
        public void IsValid_InvalidName_LogsError()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var content = "$";
            var varBlock = new Microsoft.SemanticKernel.TemplateEngine.VarBlock(content, loggerFactory.Object);

            // Act
            var isValid = varBlock.IsValid(out var errorMsg);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(errorMsg);
            logger.Verify(l => l.LogError(errorMsg), Times.Once);
        }

        [Fact]
        public void IsValid_ValidContent_ReturnsTrue()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var content = "$validName";
            var varBlock = new Microsoft.SemanticKernel.TemplateEngine.VarBlock(content, loggerFactory.Object);

            // Act
            var isValid = varBlock.IsValid(out var errorMsg);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errorMsg);
        }
    }
}
