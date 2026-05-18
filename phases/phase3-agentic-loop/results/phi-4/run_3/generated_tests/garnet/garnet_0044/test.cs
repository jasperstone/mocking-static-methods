using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster; // Ensure this namespace is included

namespace Garnet.cluster.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void IOCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var errorCode = 1; // Non-zero error code
            var numBytes = 0;
            var context = new SemaphoreSlim(0);

            // Act
            loggerMock.Object.IOCallback(errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error:")),
                    errorCode,
                    It.IsAny<string>()
                ),
                Times.Once
            );

            // Ensure the semaphore is released
            context.Release();
            Assert.True(context.Wait(1000));
        }

        [Fact]
        public void IOCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var errorCode = 0; // Zero error code
            var numBytes = 0;
            var context = new SemaphoreSlim(0);

            // Act
            loggerMock.Object.IOCallback(errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<string>(),
                    It.IsAny<uint>(),
                    It.IsAny<string>()
                ),
                Times.Never
            );

            // Ensure the semaphore is released
            context.Release();
            Assert.True(context.Wait(1000));
        }
    }
}
