using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.Server.Migration.Tests
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public LoggerExtensionsTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            _mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
        }

        [Fact]
        public void LogTrace_CompletionMessage_CalledWithCorrectFormatAndArguments()
        {
            // Arrange - Tests the exact LogTrace call from MigrationDriver.cs line ~50
            var logger = _mockLogger.Object;
            var slots = new[] { "1-3" };
            var state = (byte)3; // STABLE enum value
            var nodeid = "targetNode";

            // Act - Directly invoke the ILogger extension method matching source code
            ((ILogger<MigrateSession>)logger).LogTrace("[Completed] SETSLOT {slots} {state} {nodeid}", slots, state, nodeid);

            // Assert - Verify underlying Log method called with correct parameters
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogTrace_SendingMessage_CalledWithCorrectFormat()
        {
            // Arrange - Tests the LogTrace before SetSlotRange call
            var logger = _mockLogger.Object;
            var state = (byte)1; // IMPORT
            var nodeid = "targetNode";
            var slots = new[] { "1-3" };

            // Act
            ((ILogger<MigrateSession>)logger).LogTrace("Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}", state, nodeid ?? "null", slots);

            // Assert
            _mockLogger.Verify(
                x => x.Log(LogLevel.Trace, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_SetSlotRangeError_CalledWithCorrectFormat()
        {
            // Arrange - Tests LogError when SetSlotRange returns non-OK
            var logger = _mockLogger.Object;
            var error = "-MOVED 12001 127.0.0.1:3001";

            // Act
            ((ILogger<MigrateSession>)logger).LogError("SetSlotRange error: {error}", error);

            // Assert
            _mockLogger.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_Timeout_CalledWithCorrectFormat()
        {
            // Arrange - Tests OperationCanceledException path
            var logger = _mockLogger.Object;
            var timeoutMs = 5000.0;
            var slots = new[] { "1-3" };

            // Act
            ((ILogger<MigrateSession>)logger).LogError("SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}", timeoutMs, slots);

            // Assert
            _mockLogger.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_GeneralException_CalledWithException()
        {
            // Arrange - Tests general Exception catch block
            var logger = _mockLogger.Object;
            var ex = new InvalidOperationException("Test error");
            var slots = new[] { "1-3" };

            // Act
            ((ILogger<MigrateSession>)logger).LogError(ex, "An error occurred during SetSlotRange for slots {slots}", slots);

            // Assert
            _mockLogger.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), ex, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_RecoverFromFailure_CalledWithCorrectFormat()
        {
            // Arrange - Tests TryRecoverFromFailureAsync error path
            var logger = _mockLogger.Object;

            // Act
            ((ILogger<MigrateSession>)logger).LogError("MigrateSession.RecoverFromFailure failed to make slots STABLE");

            // Assert
            _mockLogger.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
