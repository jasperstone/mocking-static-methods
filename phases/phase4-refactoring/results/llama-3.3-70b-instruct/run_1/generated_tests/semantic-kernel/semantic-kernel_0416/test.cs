using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.TemplateEngine;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests
{
    public class VarBlockTests
    {
        [Fact]
        public void VarBlock_LogErrorCalled_WhenContentIsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VarBlock>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var varBlock = new VarBlock(string.Empty, loggerFactoryMock.Object);

            // Act
            varBlock.IsValid(out _);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void VarBlock_LogErrorCalled_WhenContentLengthIsLessThan2()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VarBlock>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var varBlock = new VarBlock("a", loggerFactoryMock.Object);

            // Act
            varBlock.IsValid(out _);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void VarBlock_LogErrorCalled_WhenContentDoesNotStartWithVarPrefix()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VarBlock>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var varBlock = new VarBlock("ab", loggerFactoryMock.Object);

            // Act
            varBlock.IsValid(out _);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}
