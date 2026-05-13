using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using garnet.cluster;

namespace Garnet.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void IOCallback_ErrorCodeNonZero_LogsErrorAndReleasesSemaphore()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0);
            uint errorCode = 123;
            uint numBytes = 456;
            string expectedMessagePart = "[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: 123 msg: ";

            // Act
            loggerMock.Object.IOCallback(errorCode, numBytes, semaphore);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.Is<string>(msg => msg.StartsWith(expectedMessagePart)), 
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

            // Act
            loggerMock.Object.IOCallback(errorCode, numBytes, semaphore);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<string>()),
                Times.Never);
            Assert.Equal(1, semaphore.CurrentCount);
        }
    }
}
