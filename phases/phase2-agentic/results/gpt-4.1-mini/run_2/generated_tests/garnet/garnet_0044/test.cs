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

            // We need to mock Utility.GetCallbackErrorMessage to return a known string.
            // Since Utility is not accessible here, we will temporarily replace the method by reflection or 
            // we can just test that LogError is called with any string.
            // Here, we will just verify that LogError is called with the expected parameters.

            // Act
            loggerMock.Object.IOCallback(errorCode, numBytes, context);

            // Assert
            // Verify that LogError was called once with the expected message and parameters
            loggerMock.Verify(
                x => x.LogError(
                    "[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}",
                    errorCode,
                    It.IsAny<string>()),
                Times.Once);

            // Verify that the semaphore was released (initial count 0, after release count should be 1)
            Assert.True(semaphore.Wait(0), "Semaphore should have been released");
        }

        [Fact]
        public void IOCallback_WithNoErrorCode_DoesNotLogErrorButReleasesSemaphore()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0, 1);
            uint errorCode = 0;
            uint numBytes = 0;
            object context = semaphore;

            // Act
            loggerMock.Object.IOCallback(errorCode, numBytes, context);

            // Assert
            // Verify that LogError was never called
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ),
                Times.Never);

            // Verify that the semaphore was released
            Assert.True(semaphore.Wait(0), "Semaphore should have been released");
        }
    }
}
