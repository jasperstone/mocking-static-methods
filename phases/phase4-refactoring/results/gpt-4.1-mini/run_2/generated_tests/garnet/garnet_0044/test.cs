using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void IOCallback_LogsErrorAndReleasesSemaphore_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0, 1);
            uint errorCode = 123;
            uint numBytes = 456;
            object context = semaphore;

            // Act
            LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}",
                    errorCode,
                    It.IsAny<string>()),
                Times.Once);

            Assert.True(semaphore.CurrentCount > 0);
        }

        [Fact]
        public void IOCallback_DoesNotLogErrorButReleasesSemaphore_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0, 1);
            uint errorCode = 0;
            uint numBytes = 456;
            object context = semaphore;

            // Act
            LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Never);

            Assert.True(semaphore.CurrentCount > 0);
        }

        [Fact]
        public void IOCallback_DoesNotThrow_WhenLoggerIsNull()
        {
            // Arrange
            ILogger logger = null;
            var semaphore = new SemaphoreSlim(0, 1);
            uint errorCode = 123;
            uint numBytes = 456;
            object context = semaphore;

            // Act & Assert
            var ex = Record.Exception(() => LoggerExtensions.IOCallback(logger, errorCode, numBytes, context));
            Assert.Null(ex);
            Assert.True(semaphore.CurrentCount > 0);
        }
    }
}
