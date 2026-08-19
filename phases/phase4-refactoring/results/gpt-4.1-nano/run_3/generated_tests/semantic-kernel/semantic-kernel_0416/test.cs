using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.SemanticKernel.TemplateEngine;
using System;

namespace SemanticKernel.Tests
{
    public class VarBlockTests
    {
        [Fact]
        public void Constructor_NullContent_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VarBlock>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<VarBlock>()).Returns(loggerMock.Object);

            // Act
            var varBlock = new VarBlock(null, loggerFactoryMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The variable name is empty")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
