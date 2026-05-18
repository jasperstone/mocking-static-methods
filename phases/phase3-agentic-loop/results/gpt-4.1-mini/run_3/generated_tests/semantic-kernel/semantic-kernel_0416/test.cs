using System;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.TemplateEngine;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine
{
    // Derived class to expose internal members for testing
    internal class TestableVarBlock : VarBlock
    {
        public TestableVarBlock(string? content, ILoggerFactory? loggerFactory = null)
            : base(content, loggerFactory)
        {
        }

        public new bool IsValid(out string errorMsg)
        {
            return base.IsValid(out errorMsg);
        }
    }

    public class VarBlockTests
    {
        [Fact]
        public void Constructor_LogsError_WhenContentLengthLessThan2()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(VarBlock))).Returns(loggerMock.Object);

            // Act
            var block = new TestableVarBlock("a", loggerFactoryMock.Object);

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
        public void Constructor_SetsName_WhenContentLengthAtLeast2()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(VarBlock))).Returns(Mock.Of<ILogger>());

            // Act
            var block = new TestableVarBlock("$name", loggerFactoryMock.Object);

            // Assert
            Assert.Equal("name", block.Name);
        }

        [Fact]
        public void IsValid_LogsErrorAndReturnsFalse_WhenContentIsNullOrEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(VarBlock))).Returns(loggerMock.Object);

            var block = new TestableVarBlock(null, loggerFactoryMock.Object);

            // Act
            var result = block.IsValid(out string errorMsg);

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
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(VarBlock))).Returns(loggerMock.Object);

            var block = new TestableVarBlock("xName", loggerFactoryMock.Object);

            // Act
            var result = block.IsValid(out string errorMsg);

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
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(VarBlock))).Returns(loggerMock.Object);

            var block = new TestableVarBlock("$", loggerFactoryMock.Object);

            // Act
            var result = block.IsValid(out string errorMsg);

            // Assert
            Assert.False(result);
            Assert.Equal("The variable name is empty", errorMsg);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "The variable name is empty"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
