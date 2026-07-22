using System;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class LoggerExtensionsTests
    {
        private static readonly MethodInfo IOCallbackMethod = typeof(LoggerExtensions)
            .GetMethod("IOCallback", BindingFlags.Public | BindingFlags.Static)!;

        [Fact]
        public void IOCallback_NoError_DoesNotLogError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0, 1);
            uint errorCode = 0;
            uint numBytes = 123;

            // Act
            IOCallbackMethod.Invoke(null, new object[] { mockLogger.Object, errorCode, numBytes, semaphore });

            // Assert
            mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
            Assert.True(semaphore.Wait(0));
        }

        [Fact]
        public void IOCallback_WithError_LogsErrorMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var semaphore = new SemaphoreSlim(0, 1);
            uint errorCode = 123;
            uint numBytes = 456;

            // Act
            IOCallbackMethod.Invoke(null, new object[] { mockLogger.Object, errorCode, numBytes, semaphore });

            // Assert
            mockLogger.Verify(l => l.LogError(
                It.Is<string>(msg => msg.Contains("[ClusterUtils]") && msg.Contains("GetQueuedCompletionStatus error")),
                It.Is<object[]>(args => args.Length == 2 && (uint)args[0] == errorCode)
            ), Times.Once);
            Assert.True(semaphore.Wait(0));
        }

        [Fact]
        public void IOCallback_NullLogger_DoesNotThrow()
        {
            // Arrange
            var semaphore = new SemaphoreSlim(0, 1);
            uint errorCode = 123;
            uint numBytes = 456;

            // Act & Assert
            IOCallbackMethod.Invoke(null, new object[] { null, errorCode, numBytes, semaphore });
            Assert.True(semaphore.Wait(0));
        }

        [Fact]
        public void IOCallback_AlwaysReleasesSemaphore()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>().Object;
            var semaphoreNoError = new SemaphoreSlim(0, 1);
            var semaphoreError = new SemaphoreSlim(0, 1);

            // Act - no error
            IOCallbackMethod.Invoke(null, new object[] { mockLogger, 0u, 0u, semaphoreNoError });
            Assert.True(semaphoreNoError.Wait(0));

            // Act - error
            IOCallbackMethod.Invoke(null, new object[] { mockLogger, 123u, 456u, semaphoreError });
            Assert.True(semaphoreError.Wait(0));
        }
    }
}
