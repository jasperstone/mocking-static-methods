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
        public void IOCallback_ErrorCode_NotZero_LogsErrorAndReleasesSemaphore()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0);
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
                    It.Is<string>(msg => msg.Contains("error: 123"))
                ),
                Times.Once
            );
            Assert.Equal(1, semaphore.CurrentCount);
        }

        [Fact]
        public void IOCallback_ErrorCode_Zero_DoesNotLogErrorAndReleasesSemaphore()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0);
            uint errorCode = 0;
            uint numBytes = 456;
            object context = semaphore;

            // Act
            loggerMock.Object.IOCallback(errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<object[]>()),
                Times.Never
            );
            Assert.Equal(1, semaphore.CurrentCount);
        }
    }
}
