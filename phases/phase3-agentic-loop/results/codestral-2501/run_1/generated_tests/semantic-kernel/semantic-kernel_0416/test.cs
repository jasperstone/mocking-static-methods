using Xunit;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.Extensions.Logging;
using Moq;

namespace SemanticKernel.Core.Tests.TemplateEngine.Blocks
{
    public class VarBlockTests
    {
        [Fact]
        public void Constructor_EmptyVariableName_LogsError()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<VarBlock>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            // Act
            var varBlock = new VarBlock("", loggerFactoryMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The variable name is empty")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void IsValid_EmptyContent_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<VarBlock>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var varBlock = new VarBlock("", loggerFactoryMock.Object);

            // Act
            var isValid = varBlock.IsValid(out var errorMsg);

            // Assert
            Assert.False(isValid);
            Assert.Equal($"A variable must start with the symbol {Symbols.VarPrefix} and have a name", errorMsg);
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"A variable must start with the symbol {Symbols.VarPrefix} and have a name")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void IsValid_ContentDoesNotStartWithVarPrefix_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<VarBlock>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var varBlock = new VarBlock("invalid", loggerFactoryMock.Object);

            // Act
            var isValid = varBlock.IsValid(out var errorMsg);

            // Assert
            Assert.False(isValid);
            Assert.Equal($"A variable must start with the symbol {Symbols.VarPrefix}", errorMsg);
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"A variable must start with the symbol {Symbols.VarPrefix}")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void IsValid_ContentLengthLessThanTwo_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<VarBlock>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var varBlock = new VarBlock($"{Symbols.VarPrefix}", loggerFactoryMock.Object);

            // Act
            var isValid = varBlock.IsValid(out var errorMsg);

            // Assert
            Assert.False(isValid);
            Assert.Equal("The variable name is empty", errorMsg);
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The variable name is empty")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void IsValid_InvalidVariableName_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<VarBlock>>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var varBlock = new VarBlock($"{Symbols.VarPrefix}invalid-name!", loggerFactoryMock.Object);

            // Act
            var isValid = varBlock.IsValid(out var errorMsg);

            // Assert
            Assert.False(isValid);
            Assert.Equal($"The variable name 'invalid-name!' contains invalid characters. Only alphanumeric chars and underscore are allowed.", errorMsg);
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"The variable name 'invalid-name!' contains invalid characters. Only alphanumeric chars and underscore are allowed.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
