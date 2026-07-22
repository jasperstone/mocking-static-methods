using Xunit;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.Extensions.Logging;
using Moq;

namespace SemanticKernel.Core.Tests.TemplateEngine.Blocks
{
    public class VarBlockTests
    {
        [Fact]
        public void LogError_Called_When_Variable_Name_Is_Empty()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<VarBlock>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            // Act
            var varBlock = new VarBlock("v", loggerFactoryMock.Object);

            // Assert
            loggerMock.Verify(x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>()), Times.Once);
        }

        [Fact]
        public void IsValid_Returns_False_And_Logs_Error_When_Content_Is_Null()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<VarBlock>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var varBlock = new VarBlock(null, loggerFactoryMock.Object);

            // Act
            var isValid = varBlock.IsValid(out var errorMsg);

            // Assert
            Assert.False(isValid);
            Assert.Equal($"A variable must start with the symbol {Symbols.VarPrefix} and have a name", errorMsg);
            loggerMock.Verify(x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>()), Times.Once);
        }

        [Fact]
        public void IsValid_Returns_False_And_Logs_Error_When_Content_Does_Not_Start_With_VarPrefix()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<VarBlock>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var varBlock = new VarBlock("x", loggerFactoryMock.Object);

            // Act
            var isValid = varBlock.IsValid(out var errorMsg);

            // Assert
            Assert.False(isValid);
            Assert.Equal($"A variable must start with the symbol {Symbols.VarPrefix}", errorMsg);
            loggerMock.Verify(x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>()), Times.Once);
        }

        [Fact]
        public void IsValid_Returns_False_And_Logs_Error_When_Variable_Name_Is_Empty()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<VarBlock>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var varBlock = new VarBlock("v", loggerFactoryMock.Object);

            // Act
            var isValid = varBlock.IsValid(out var errorMsg);

            // Assert
            Assert.False(isValid);
            Assert.Equal("The variable name is empty", errorMsg);
            loggerMock.Verify(x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>()), Times.Once);
        }

        [Fact]
        public void IsValid_Returns_False_And_Logs_Error_When_Variable_Name_Contains_Invalid_Characters()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<VarBlock>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var varBlock = new VarBlock("v$", loggerFactoryMock.Object);

            // Act
            var isValid = varBlock.IsValid(out var errorMsg);

            // Assert
            Assert.False(isValid);
            Assert.Equal("The variable name '$' contains invalid characters. Only alphanumeric chars and underscore are allowed.", errorMsg);
            loggerMock.Verify(x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>()), Times.Once);
        }

        [Fact]
        public void Render_Throws_KernelException_When_Variable_Name_Is_Empty()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<VarBlock>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var varBlock = new VarBlock("v", loggerFactoryMock.Object);

            // Act & Assert
            Assert.Throws<KernelException>(() => varBlock.Render(null));
            loggerMock.Verify(x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>()), Times.Once);
        }

        [Fact]
        public void Render_Returns_Null_When_Variable_Not_Found()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<VarBlock>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var varBlock = new VarBlock("vx", loggerFactoryMock.Object);
            var arguments = new KernelArguments();

            // Act
            var result = varBlock.Render(arguments);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(x => x.LogWarning(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>()), Times.Once);
        }
    }
}
