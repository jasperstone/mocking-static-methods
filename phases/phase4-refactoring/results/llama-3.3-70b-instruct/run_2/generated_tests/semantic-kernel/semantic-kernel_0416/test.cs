using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.TemplateEngine;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests
{
    public class VarBlockTests
    {
        [Fact]
        public void VarBlock_LogError_WhenContentIsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var varBlock = new VarBlock(string.Empty, loggerFactoryMock.Object);

            // Act
            varBlock.IsValid(out _);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void VarBlock_LogError_WhenContentLengthIsLessThan2()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var varBlock = new VarBlock("a", loggerFactoryMock.Object);

            // Act
            varBlock.IsValid(out _);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void VarBlock_LogError_WhenContentDoesNotStartWithVarPrefix()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var varBlock = new VarBlock("abc", loggerFactoryMock.Object);

            // Act
            varBlock.IsValid(out _);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}
