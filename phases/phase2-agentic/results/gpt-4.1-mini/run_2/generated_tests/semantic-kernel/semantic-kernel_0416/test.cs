using System;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.TemplateEngine;
using Moq;
using Xunit;

namespace SemanticKernel.Core.TemplateEngine.Blocks.Tests
{
    public class VarBlockTests
    {
        [Fact]
        public void Constructor_LogsError_WhenContentLengthLessThan2()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

            // Act
            var varBlock = new VarBlock("a", loggerFactoryMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "The variable name is empty"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void IsValid_LogsErrorAndReturnsFalse_WhenContentIsNullOrEmpty()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

            var varBlock = new VarBlock(null, loggerFactoryMock.Object);

            // Act
            var result = varBlock.IsValid(out string errorMsg);

            // Assert
            Assert.False(result);
            Assert.Contains("A variable must start with the symbol", errorMsg);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == errorMsg),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void IsValid_LogsErrorAndReturnsFalse_WhenContentDoesNotStartWithVarPrefix()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

            var varBlock = new VarBlock("xName", loggerFactoryMock.Object);

            // Act
            var result = varBlock.IsValid(out string errorMsg);

            // Assert
            Assert.False(result);
            Assert.Contains("A variable must start with the symbol", errorMsg);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == errorMsg),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void IsValid_LogsErrorAndReturnsFalse_WhenContentLengthLessThan2()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

            var varBlock = new VarBlock("$", loggerFactoryMock.Object);

            // Act
            var result = varBlock.IsValid(out string errorMsg);

            // Assert
            Assert.False(result);
            Assert.Equal("The variable name is empty", errorMsg);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == errorMsg),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
