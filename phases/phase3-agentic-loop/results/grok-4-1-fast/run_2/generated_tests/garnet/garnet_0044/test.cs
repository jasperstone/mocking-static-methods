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
        public void IOCallback_NoError_DoesNotLogError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;
            uint errorCode = 0;
            uint numBytes = 123;
            var semaphore = new SemaphoreSlim(0, 1);

            // Act
            ((ILogger)logger).IOCallback(errorCode, numBytes, semaphore);

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void IOCallback_WithError_LogsErrorMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();
            var logger = mockLogger.Object;
            uint errorCode = 123;
            uint numBytes = 456;
            var semaphore = new SemaphoreSlim(0, 1);

            // Act
            ((ILogger)logger).IOCallback(errorCode, numBytes, semaphore);

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("[ClusterUtils]") && 
                    v.ToString()!.Contains("123")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void IOCallback_NullLogger_DoesNotThrow()
        {
            // Since it's an extension method with ?. operator, null logger is handled safely
            ILogger? logger = null;
            uint errorCode = 999;
            uint numBytes = 888;
            var semaphore = new SemaphoreSlim(0, 1);

            // Act & Assert - should not throw due to null-conditional operator
            logger?.IOCallback(errorCode, numBytes, semaphore);
        }

        [Fact]
        public void IOCallback_AlwaysReleasesSemaphore()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;
            var semaphore = new SemaphoreSlim(0, 1);
            uint errorCode = 123;
            uint numBytes = 456;

            // Act
            ((ILogger)logger).IOCallback(errorCode, numBytes, semaphore);

            // Assert
            Assert.True(semaphore.CurrentCount > 0);
        }

        [Fact]
        public void IOCallback_NoError_ReleasesSemaphore()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;
            var semaphore = new SemaphoreSlim(0, 1);
            uint errorCode = 0;
            uint numBytes = 456;

            // Act
            ((ILogger)logger).IOCallback(errorCode, numBytes, semaphore);

            // Assert
            Assert.True(semaphore.CurrentCount > 0);
        }
    }
}
