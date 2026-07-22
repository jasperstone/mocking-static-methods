using System;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.tests
{
    public class LoggerExtensionsTests
    {
        private static readonly MethodInfo IoCallbackMethod;

        static LoggerExtensionsTests()
        {
            var assembly = typeof(Garnet.cluster.ClusterUtils).Assembly;
            var loggerExtensionsType = assembly.GetType("Garnet.cluster.LoggerExtensions");
            IoCallbackMethod = loggerExtensionsType.GetMethod("IOCallback", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(IoCallbackMethod);
        }

        [Fact]
        public void IOCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            uint errorCode = 123;
            uint numBytes = 456;
            var context = new SemaphoreSlim(0);

            // Act
            IoCallbackMethod.Invoke(null, new object[] { loggerMock.Object, errorCode, numBytes, context });

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}",
                    errorCode,
                    It.IsAny<string>()),
                Times.Once);

            Assert.Equal(1, context.CurrentCount);
        }

        [Fact]
        public void IOCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            uint errorCode = 0;
            uint numBytes = 456;
            var context = new SemaphoreSlim(0);

            // Act
            IoCallbackMethod.Invoke(null, new object[] { loggerMock.Object, errorCode, numBytes, context });

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ),
                Times.Never);

            Assert.Equal(1, context.CurrentCount);
        }
    }
}
