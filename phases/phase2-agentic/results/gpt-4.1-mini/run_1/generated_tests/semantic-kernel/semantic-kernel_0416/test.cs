using System;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.SemanticKernel.TemplateEngine.Blocks;
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
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

            // Act
            var block = new VarBlock("a", loggerFactoryMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "The variable name is empty"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_SetsName_WhenContentLengthAtLeast2()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(Mock.Of<ILogger>());

            // Act
            var block = new VarBlock("$name", loggerFactoryMock.Object);

            // Assert
            Assert.Equal("name", block.Name);
        }

        [Fact]
        public void Constructor_SetsName_EmptyContent_SetsNameEmpty()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(Mock.Of<ILogger>());

            // Act
            var block = new VarBlock(null, loggerFactoryMock.Object);

            // Assert
            Assert.Equal(string.Empty, block.Name);
        }
    }
}
