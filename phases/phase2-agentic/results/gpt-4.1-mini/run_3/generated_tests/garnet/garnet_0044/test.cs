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
        public void IOCallback_ErrorCodeZero_DoesNotLogError_ReleasesSemaphore()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0);

            // Act
            loggerMock.Object.IOCallback(0, 123, semaphore);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Never);

            Assert.Equal(1, semaphore.CurrentCount);
        }

        [Fact]
        public void IOCallback_ErrorCodeNonZero_LogsErrorAndReleasesSemaphore()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0);
            uint errorCode = 42;
            uint numBytes = 100;
            object context = semaphore;

            // We need to mock Utility.GetCallbackErrorMessage to return a known string.
            // Since Utility is not accessible here, we will temporarily replace the method by reflection or 
            // we can just test that LogError is called with the expected format and parameters.

            // Act
            loggerMock.Object.IOCallback(errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}",
                    errorCode,
                    It.IsAny<string>()),
                Times.Once);

            Assert.Equal(1, semaphore.CurrentCount);
        }
    }
}
