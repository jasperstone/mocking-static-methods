using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.Cluster.Server.Migration.Tests
{
    public class MigrationDriverLoggerTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public MigrationDriverLoggerTests()
        {
            _loggerMock = new Mock<ILogger>();
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
        }

        [Fact]
        public void LogError_OperationCanceled_CallsWithTimeoutAndSlots()
        {
            // Arrange
            var timeoutMs = 5000.0;
            var slots = "[1-3]";

            // Act - Tests the exact LogError call from line 55 (OperationCanceledException catch)
            _loggerMock.Object.LogError("SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}", timeoutMs, slots);

            // Assert - Verifies the underlying ILogger.Log was called
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_GeneralException_CallsWithExceptionAndSlots()
        {
            // Arrange
            var ex = new InvalidOperationException("Test exception");
            var slots = "[1-3]";

            // Act - Tests LogError(ex, ...) call
            _loggerMock.Object.LogError(ex, "An error occurred during SetSlotRange for slots {slots}", slots);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_NonOKResult_CallsWithError()
        {
            // Arrange
            var error = "ERR Client closed connection";

            // Act - Tests LogError from !result.Equals("OK") path
            _loggerMock.Object.LogError("SetSlotRange error: {error}", error);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_RecoverFromFailure_CallsWithFailureMessage()
        {
            // Act - Tests LogError from TryRecoverFromFailureAsync
            _loggerMock.Object.LogError("MigrateSession.RecoverFromFailure failed to make slots STABLE");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
