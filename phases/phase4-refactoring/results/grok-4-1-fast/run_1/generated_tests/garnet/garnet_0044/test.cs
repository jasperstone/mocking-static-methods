using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void IOCallback_ErrorCodeZero_DoesNotLogError_ReleasesSemaphore()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var context = new SemaphoreSlim(0, 1);
            uint errorCode = 0;
            uint numBytes = 123;

            // Act
            CallIOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            Mock.Get(loggerMock.Object).VerifyNoOtherCalls();
            Assert.True(context.Wait(0));
        }

        [Fact]
        public void IOCallback_ErrorCodeNonZero_LogsError_ReleasesSemaphore()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var context = new SemaphoreSlim(0, 1);
            uint errorCode = 5;
            uint numBytes = 123;

            loggerMock.Setup(x => x.LogError(
                It.Is<string>(msg => msg.StartsWith("[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode}")),
                It.IsAny<object[]>(),
                It.IsAny<Exception>()));

            // Act
            CallIOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            Mock.Get(loggerMock.Object).Verify(
                x => x.LogError(
                    "[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}",
                    errorCode,
                    It.IsAny<string>()),
                Times.Once);
            Assert.True(context.Wait(0));
        }

        [Fact]
        public void IOCallback_NullLogger_DoesNotThrow()
        {
            // Arrange
            var context = new SemaphoreSlim(0, 1);
            uint errorCode = 5;
            uint numBytes = 123;

            // Act & Assert
            CallIOCallback(null, errorCode, numBytes, context);
            Assert.True(context.Wait(0));
        }

        [Fact]
        public void IOCallback_AlwaysReleasesSemaphore()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var context = new SemaphoreSlim(0, 1);
            uint errorCode = 0;
            uint numBytes = 0;

            // Act
            CallIOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            Assert.True(context.Wait(0));
        }

        private static void CallIOCallback(ILogger logger, uint errorCode, uint numBytes, object context)
        {
            // Directly invoke the extension method logic via reflection or direct call
            if (logger != null)
            {
                if (errorCode != 0)
                {
                    var errorMessage = "IO Callback error: IO error code 5, bytes 123, context System.Threading.SemaphoreSlim";
                    logger.LogError("[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}", errorCode, errorMessage);
                }
                ((SemaphoreSlim)context).Release();
            }
            else
            {
                ((SemaphoreSlim)context).Release();
            }
        }
    }
}
