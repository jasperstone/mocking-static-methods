using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Orleans.Streaming.NATS.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_WithException_CallsLoggerLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new InvalidOperationException("Test exception");
            var message = "Error initializing NATS JetStream Connection Manager";

            // Act
            LoggerExtensions.LogError(loggerMock.Object, exception, message);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
