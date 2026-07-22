using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void IOCallback_ErrorCodeZero_DoesNotLogError()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0, 1);
            uint errorCode = 0;
            uint numBytes = 123;

            // Act
            CallIOCallback(logger.Object, errorCode, numBytes, semaphore);

            // Assert
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);

            Assert.Equal(1, semaphore.CurrentCount);
        }

        [Fact]
        public void IOCallback_NonZeroErrorCode_LogsError()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0, 1);
            uint errorCode = 5;
            uint numBytes = 123;

            // Act
            CallIOCallback(logger.Object, errorCode, numBytes, semaphore);

            // Assert
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.Is<EventId>(e => e.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[ClusterUtils]") && 
                                                 v.ToString()!.Contains("5")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Equal(1, semaphore.CurrentCount);
        }

        [Fact]
        public void IOCallback_NullLogger_ReleasesSemaphore()
        {
            // Arrange
            var semaphore = new SemaphoreSlim(0, 1);
            uint errorCode = 5;
            uint numBytes = 123;

            // Act
            CallIOCallback(null, errorCode, numBytes, semaphore);

            // Assert
            Assert.Equal(1, semaphore.CurrentCount);
        }

        [Fact]
        public void IOCallback_AlwaysReleasesSemaphore()
        {
            // Arrange
            var logger = new Mock<ILogger>().Object;
            var semaphore = new SemaphoreSlim(0, 1);

            // Act
            CallIOCallback(logger, 0, 0, semaphore);
            CallIOCallback(logger, 999, 999, semaphore);

            // Assert
            Assert.Equal(2, semaphore.CurrentCount);
        }

        private static void CallIOCallback(ILogger? logger, uint errorCode, uint numBytes, object context)
        {
            if (errorCode != 0 && logger != null)
            {
                var errorMessage = $"IOCallback called with errorCode={errorCode}, numBytes={numBytes}, context={context}";
                logger.LogError("[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}", errorCode, errorMessage);
            }
            ((SemaphoreSlim)context).Release();
        }
    }
}
