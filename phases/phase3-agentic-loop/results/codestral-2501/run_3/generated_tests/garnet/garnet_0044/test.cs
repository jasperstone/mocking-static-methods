using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void IOCallback_LogsError_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var errorCode = 1u;
            var numBytes = 10u;
            var context = new SemaphoreSlim(0);
            var mockLogger = new Mock<ILogger>();

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
            var errorCode = 0u;
            var numBytes = 10u;
            var context = new SemaphoreSlim(0);
            var mockLogger = new Mock<ILogger>();

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

        [Fact]
        public void IOCallback_ReleasesSemaphore()
        {
            // Arrange
            var errorCode = 1u;
            var numBytes = 10u;
            var context = new SemaphoreSlim(0);
            var mockLogger = new Mock<ILogger>();

            // Act
            LoggerExtensions.IOCallback(mockLogger.Object, errorCode, numBytes, context);

            // Assert
            Assert.True(context.Wait(0));
        }
    }
}
