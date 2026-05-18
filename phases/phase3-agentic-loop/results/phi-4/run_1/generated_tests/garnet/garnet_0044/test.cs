using Moq;
using Microsoft.Extensions.Logging;
using Xunit;

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
            var context = new object();

            // Mock the Log method to intercept LogError calls
            loggerMock
                .Setup(l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((logLevel, eventId, state, exception, formatter) =>
                {
                    var message = formatter(state, exception);
                    Assert.Contains("[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error:", message);
                    Assert.Contains($"errorCode: {errorCode}", message);
                    Assert.Contains("msg: Test error message", message);
                });

            // Act
            loggerMock.Object.IOCallback(errorCode, numBytes, context);
        }

        [Fact]
        public void IOCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var errorCode = 0; // Zero error code
            var numBytes = 0;
            var context = new object();

            // Mock the Log method to intercept LogError calls
            loggerMock
                .Setup(l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((logLevel, eventId, state, exception, formatter) =>
                {
                    Assert.True(false, "LogError should not be called when errorCode is zero.");
                });

            // Act
            loggerMock.Object.IOCallback(errorCode, numBytes, context);
        }
    }
}
