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

            // Semaphore should be released once, so Wait should not block
            Assert.True(semaphore.Wait(0));
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

            // Setup Utility.GetCallbackErrorMessage to return a fixed string
            // Since Utility is not accessible, we simulate by intercepting the LogError call

            string capturedMessage = null;
            object[] capturedArgs = null;

            loggerMock.Setup(l => l.LogError(
                It.IsAny<string>(),
                It.IsAny<object[]>()
            )).Callback<string, object[]>((msg, args) =>
            {
                capturedMessage = msg;
                capturedArgs = args;
            });

            // Act
            loggerMock.Object.IOCallback(errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}",
                    errorCode, It.IsAny<object>()),
                Times.Once);

            Assert.NotNull(capturedMessage);
            Assert.Equal("[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}", capturedMessage);
            Assert.NotNull(capturedArgs);
            Assert.Equal(errorCode, capturedArgs[0]);
            Assert.NotNull(capturedArgs[1]);

            // Semaphore should be released once, so Wait should not block
            Assert.True(semaphore.Wait(0));
        }
    }
}
