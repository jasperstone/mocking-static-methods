using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;

namespace ClusterUtilsTests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void IOCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var context = new SemaphoreSlim(0);
            uint errorCode = 123;
            uint numBytes = 456;

            // Act
            LoggerExtensions.IOCallback(mockLogger.Object, errorCode, numBytes, context);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    "[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}", 
                    errorCode, 
                    It.Is<string>(msg => msg.Contains("error code: 123"))
                ),
                Times.Once
            );
            Assert.Equal(1, context.CurrentCount);
        }

        [Fact]
        public void IOCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var context = new SemaphoreSlim(0);
            uint errorCode = 0;
            uint numBytes = 456;

            // Act
            LoggerExtensions.IOCallback(mockLogger.Object, errorCode, numBytes, context);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Never
            );
            Assert.Equal(1, context.CurrentCount);
        }
    }
}
