using System;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace SemanticKernel.Core.Tests.TemplateEngine.Blocks
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_IsCalled_WithExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // Act
            loggerMock.Object.LogError("The variable name is empty");

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
    }
}
