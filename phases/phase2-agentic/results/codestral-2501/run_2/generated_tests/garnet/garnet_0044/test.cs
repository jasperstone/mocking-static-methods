using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;

namespace Garnet.cluster.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void IOCallback_LogsError_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            uint errorCode = 1;
            uint numBytes = 100;
            var context = new SemaphoreSlim(0);

            // Act
            LoggerExtensions.IOCallback(mockLogger.Object, errorCode, numBytes, context);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: 1 msg: ")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void IOCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            uint errorCode = 0;
            uint numBytes = 100;
            var context = new SemaphoreSlim(0);

            // Act
            LoggerExtensions.IOCallback(mockLogger.Object, errorCode, numBytes, context);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
