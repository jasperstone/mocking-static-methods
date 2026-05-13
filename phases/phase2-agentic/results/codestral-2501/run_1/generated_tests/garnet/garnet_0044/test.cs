using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ClusterUtilsTests
    {
        [Fact]
        public void IOCallback_LogsError_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            uint errorCode = 1;
            uint numBytes = 10;
            var context = new SemaphoreSlim(0);

            // Act
            LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("OverlappedStream GetQueuedCompletionStatus error")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void IOCallback_ReleasesSemaphore_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            uint errorCode = 0;
            uint numBytes = 10;
            var context = new SemaphoreSlim(0);

            // Act
            LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            Assert.True(context.CurrentCount == 1);
        }

        [Fact]
        public void IOCallback_ReleasesSemaphore_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            uint errorCode = 1;
            uint numBytes = 10;
            var context = new SemaphoreSlim(0);

            // Act
            LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            Assert.True(context.CurrentCount == 1);
        }
    }
}
