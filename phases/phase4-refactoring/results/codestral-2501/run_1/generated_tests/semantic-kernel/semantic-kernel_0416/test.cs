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
        public void IsValid_Returns_False_When_Variable_Name_Is_Empty()
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
        }

        [Fact]
        public void IsValid_Returns_False_When_Variable_Does_Not_Start_With_VarPrefix()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<VarBlock>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var varBlock = new VarBlock("a", loggerFactoryMock.Object);

            // Act
            var isValid = varBlock.IsValid(out var errorMsg);

            // Assert
            Assert.False(isValid);
            Assert.Equal($"A variable must start with the symbol {Symbols.VarPrefix}", errorMsg);
        }

        [Fact]
        public void IsValid_Returns_False_When_Variable_Name_Contains_Invalid_Characters()
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
        }
    }
}
