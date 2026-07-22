using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    internal class LoggerExtensionsTests
    {
        [Fact]
        public void IOCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            uint errorCode = 123;
            uint numBytes = 456;
            var semaphore = new SemaphoreSlim(0);

            // Act
            loggerMock.Object.IOCallback(errorCode, numBytes, semaphore);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}",
                    errorCode,
                    It.IsAny<string>()),
                Times.Once);

            // Also verify that the semaphore was released
            Assert.Equal(1, semaphore.CurrentCount);
        }

        [Fact]
        public void IOCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            uint errorCode = 0;
            uint numBytes = 456;
            var semaphore = new SemaphoreSlim(0);

            // Act
            loggerMock.Object.IOCallback(errorCode, numBytes, semaphore);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ),
                Times.Never);

            // Also verify that the semaphore was released
            Assert.Equal(1, semaphore.CurrentCount);
        }
    }
}
