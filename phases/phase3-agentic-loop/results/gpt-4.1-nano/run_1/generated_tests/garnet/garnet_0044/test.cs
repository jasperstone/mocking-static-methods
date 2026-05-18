using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;
using System;

namespace Garnet.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void IOCallback_ErrorCodeNotZero_LogsErrorAndReleasesSemaphore()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0);
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
            Assert.Equal(1, semaphore.CurrentCount);
        }

        [Fact]
        public void IOCallback_ErrorCodeZero_DoesNotLogErrorAndReleasesSemaphore()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0);
            uint errorCode = 0;
            uint numBytes = 456;
            object context = semaphore;

            // Act
            LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Never);
            Assert.Equal(1, semaphore.CurrentCount);
        }
    }
}
