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
        public void IOCallback_LogsError_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0);
            var errorCode = 1u;
            var numBytes = 0u;
            var context = semaphore;

            // Act
            Garnet.cluster.ClusterUtils.LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), errorCode, It.IsAny<string>()), Times.Once);
            Assert.True(semaphore.Wait(0));
        }

        [Fact]
        public void IOCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0);
            var errorCode = 0u;
            var numBytes = 0u;
            var context = semaphore;

            // Act
            Garnet.cluster.ClusterUtils.LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never);
            Assert.True(semaphore.Wait(0));
        }
    }
}
