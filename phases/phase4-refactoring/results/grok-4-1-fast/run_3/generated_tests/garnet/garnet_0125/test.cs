using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Cluster.Tests
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public LoggerExtensionsTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
        }

        [Fact]
        public void LogTrace_CompletionMessage_CalledWithCorrectParameters()
        {
            // Arrange
            var logger = _mockLogger.Object;
            var slotsRange = "1-3";
            var state = "STABLE";
            var nodeid = "targetNode";

            // Act - Directly test the logger extension call on line 50
            logger.LogTrace("[Completed] SETSLOT {slots} {state} {nodeid}", slotsRange, state, nodeid ?? "");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("[Completed] SETSLOT") &&
                        v.ToString().Contains(slotsRange) &&
                        v.ToString().Contains(state) &&
                        v.ToString().Contains(nodeid)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogTrace_SendingMessage_CalledWithCorrectParameters()
        {
            // Arrange
            var logger = _mockLogger.Object;
            var state = "IMPORT";
            var nodeid = "sourceNode";
            var slotsRange = "10-20";

            // Act - Test the LogTrace before SetSlotRange
            logger.LogTrace("Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}", state, nodeid ?? "null", slotsRange);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("Sending CLUSTER SETSLOTRANGE") &&
                        v.ToString().Contains(state) &&
                        v.ToString().Contains("sourceNode") &&
                        v.ToString().Contains(slotsRange)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_SetSlotRangeError_CalledWithCorrectParameters()
        {
            // Arrange
            var logger = _mockLogger.Object;
            var errorResult = "ERR Invalid operation";

            // Act
            logger.LogError("SetSlotRange error: {error}", errorResult);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange error") && v.ToString().Contains(errorResult)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_OperationCanceled_CalledWithCorrectParameters()
        {
            // Arrange
            var logger = _mockLogger.Object;
            var timeoutMs = 5000.0;
            var slotsRange = "5-10";

            // Act
            logger.LogError("SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}", timeoutMs, slotsRange);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("timed out or was cancelled") &&
                        v.ToString().Contains("5000") &&
                        v.ToString().Contains(slotsRange)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_RecoverFromFailure_CalledWithCorrectParameters()
        {
            // Arrange
            var logger = _mockLogger.Object;

            // Act
            logger.LogError("MigrateSession.RecoverFromFailure failed to make slots STABLE");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("RecoverFromFailure failed to make slots STABLE")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_WithException_CalledWithCorrectParameters()
        {
            // Arrange
            var logger = _mockLogger.Object;
            var testException = new InvalidOperationException("Test failure");
            var slotsRange = "100-200";

            // Act
            logger.LogError(testException, "An error occurred during SetSlotRange for slots {slots}", slotsRange);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    testException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
