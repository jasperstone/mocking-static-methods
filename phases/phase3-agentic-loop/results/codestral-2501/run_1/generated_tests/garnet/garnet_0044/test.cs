using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class ClusterUtilsTests
    {
        [Fact]
        public void IOCallback_LogsError_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            uint errorCode = 1;
            uint numBytes = 10;
            var context = new SemaphoreSlim(0);

            // Act
            LoggerExtensions.IOCallback(mockLogger.Object, errorCode, numBytes, context);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
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
            uint numBytes = 10;
            var context = new SemaphoreSlim(0);

            // Act
            LoggerExtensions.IOCallback(mockLogger.Object, errorCode, numBytes, context);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }
    }
}
