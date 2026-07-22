using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;
using System.Threading;

namespace Garnet.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void IOCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var context = new SemaphoreSlim(0);
            uint errorCode = 123;
            uint numBytes = 456;
            string expectedMessagePart = "[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error";

            // Act
            LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.Is<string>(s => s.Contains(expectedMessagePart)), errorCode, It.IsAny<string>()),
                Times.Once);
            Assert.Equal(1, context.CurrentCount);
        }

        [Fact]
        public void IOCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var context = new SemaphoreSlim(0);
            uint errorCode = 0;
            uint numBytes = 456;

            // Act
            LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<string>()),
                Times.Never);
            Assert.Equal(1, context.CurrentCount);
        }
    }
}
