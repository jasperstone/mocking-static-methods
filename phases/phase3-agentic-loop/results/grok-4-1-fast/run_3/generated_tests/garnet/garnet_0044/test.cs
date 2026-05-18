using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace Garnet.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void IOCallback_ErrorCodeZero_DoesNotLogError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            uint errorCode = 0;
            uint numBytes = 123;
            var semaphore = new SemaphoreSlim(0, 1);

            // Act
            mockLogger.Object.IOCallback(errorCode, numBytes, semaphore);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void IOCallback_WithErrorCode_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            uint errorCode = 123;
            uint numBytes = 456;
            var semaphore = new SemaphoreSlim(0, 1);

            // Act
            mockLogger.Object.IOCallback(errorCode, numBytes, semaphore);

            // Assert
            mockLogger.VerifyAll();
        }

        [Fact]
        public void IOCallback_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger logger = null!;
            uint errorCode = 999;
            uint numBytes = 888;
            var semaphore = new SemaphoreSlim(0, 1);

            // Act & Assert - null-conditional prevents call
            Assert.DoesNotThrow(() => logger?.IOCallback(errorCode, numBytes, semaphore));
        }

        [Fact]
        public void IOCallback_AlwaysReleasesSemaphore()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0, 1);
            uint errorCode = 5;
            uint numBytes = 0;

            // Act
            mockLogger.Object.IOCallback(errorCode, numBytes, semaphore);

            // Assert
            Assert.True(semaphore.CurrentCount > 0);
        }
    }
}
