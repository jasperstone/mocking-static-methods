using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading;

namespace Garnet.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void IOCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var errorCode = 1u;
            var numBytes = 10u;
            var context = new SemaphoreSlim(0);
            var errorMessage = "Error occurred";

            // Mock Utility.GetCallbackErrorMessage to return a specific message
            // Since Utility is not accessible here, we can assume it's static and can be mocked if needed.
            // For simplicity, we will just test that LogError is called with the correct parameters.

            // Act
            mockLogger.Object.IOCallback(errorCode, numBytes, context);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    "[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}",
                    errorCode,
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
