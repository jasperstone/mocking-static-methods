using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogTrace_LogsTraceMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            string message = "Test message";
            object[] args = new object[] { "arg1", "arg2" };

            // Act
            LoggerExtensions.LogTrace(mockLogger.Object, message, args);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
