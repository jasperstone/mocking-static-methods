using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Orleans.Streaming.NATS.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_ExtensionMethod_LogsErrorWithExceptionAndMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var ex = new Exception("Test exception");

            // Act
            loggerMock.Object.LogError(ex, "Error initializing NATS JetStream Connection Manager");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error initializing NATS JetStream Connection Manager")),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
