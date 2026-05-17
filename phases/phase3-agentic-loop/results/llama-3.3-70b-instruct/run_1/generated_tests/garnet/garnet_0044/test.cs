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
            var semaphoreMock = new Mock<SemaphoreSlim>();
            var errorCode = 1u;
            var numBytes = 0u;
            var context = semaphoreMock.Object;

            // Act
            ClusterUtils.LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), errorCode, It.IsAny<string>()), Times.Once);
            semaphoreMock.Verify(s => s.Release(), Times.Once);
        }

        [Fact]
        public void IOCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphoreMock = new Mock<SemaphoreSlim>();
            var errorCode = 0u;
            var numBytes = 0u;
            var context = semaphoreMock.Object;

            // Act
            ClusterUtils.LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never);
            semaphoreMock.Verify(s => s.Release(), Times.Once);
        }

        [Fact]
        public void IOCallback_ReleasesSemaphore()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var semaphoreMock = new Mock<SemaphoreSlim>();
            var errorCode = 1u;
            var numBytes = 0u;
            var context = semaphoreMock.Object;

            // Act
            ClusterUtils.LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            semaphoreMock.Verify(s => s.Release(), Times.Once);
        }
    }
}
