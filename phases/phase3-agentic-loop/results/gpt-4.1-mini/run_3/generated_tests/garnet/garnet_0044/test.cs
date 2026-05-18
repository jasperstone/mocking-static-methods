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
        public void IOCallback_WithErrorCode_LogsErrorAndReleasesSemaphore()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0, 1);
            uint errorCode = 123;
            uint numBytes = 456;
            object context = semaphore;

            // Act
            loggerMock.Object.IOCallback(errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}",
                    errorCode,
                    It.IsAny<string>()),
                Times.Once);

            // The semaphore should be released once, so Wait should complete immediately.
            var waitSucceeded = semaphore.Wait(0);
            Assert.True(waitSucceeded);
        }

        [Fact]
        public void IOCallback_WithNoErrorCode_DoesNotLogErrorButReleasesSemaphore()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0, 1);
            uint errorCode = 0;
            uint numBytes = 456;
            object context = semaphore;

            // Act
            loggerMock.Object.IOCallback(errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ),
                Times.Never);

            // The semaphore should be released once, so Wait should complete immediately.
            var waitSucceeded = semaphore.Wait(0);
            Assert.True(waitSucceeded);
        }
    }
}
