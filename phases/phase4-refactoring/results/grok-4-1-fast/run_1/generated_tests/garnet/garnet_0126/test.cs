using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public LoggerExtensionsTests()
        {
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public void LogError_WithException_FormatsMessageCorrectly()
        {
            // Arrange
            var logger = _mockLogger.Object;
            var exception = new InvalidOperationException("Test exception");
            var slots = "1-3";

            // Act - Matches logger?.LogError(ex, "An error occurred during SetSlotRange for slots {slots}", ClusterManager.GetRange([.. _sslots]));
            logger.LogError(exception, "An error occurred during SetSlotRange for slots {slots}", slots);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    exception,
                    It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_WithTimeoutAndSlots_FormatsMessageCorrectly()
        {
            // Arrange
            var logger = _mockLogger.Object;
            var timeoutMs = 1000.0;
            var slots = "1-3";

            // Act - Matches line 55: logger?.LogError("SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}", _timeout.TotalMilliseconds, ClusterManager.GetRange([.. _sslots]));
            logger.LogError("SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}", timeoutMs, slots);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_WithErrorResult_FormatsMessageCorrectly()
        {
            // Arrange
            var logger = _mockLogger.Object;
            var error = "ERR Invalid operation";

            // Act - Matches line 55: logger?.LogError("SetSlotRange error: {error}", result);
            logger.LogError("SetSlotRange error: {error}", error);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_RecoverFromFailure_FormatsMessageCorrectly()
        {
            // Arrange
            var logger = _mockLogger.Object;

            // Act - Matches TryRecoverFromFailureAsync: logger?.LogError("MigrateSession.RecoverFromFailure failed to make slots STABLE");
            logger.LogError("MigrateSession.RecoverFromFailure failed to make slots STABLE");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }
    }
}
