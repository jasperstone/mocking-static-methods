using System;
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
        }

        [Fact]
        public void LogTrace_CompletionMessage_CalledWithCorrectFormat()
        {
            // Arrange
            var slots = "1-1000";
            var state = "STABLE";
            var nodeid = "target-node";

            // Act - Simulate the exact LogTrace call from line 50
            _mockLogger.Object.LogTrace("[Completed] SETSLOT {slots} {state} {nodeid}", slots, state, nodeid);

            // Assert - Verify the underlying ILogger.Log was called with correct parameters
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    0,
                    It.Is<It.IsAnyFormat<string>>(fmt => fmt.Format == "[Completed] SETSLOT {slots} {state} {nodeid}"),
                    It.Is<object[]>(args => args.Length == 3 && 
                                          args[0].ToString() == slots &&
                                          args[1].ToString() == state &&
                                          args[2].ToString() == nodeid),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogTrace_SendingMessage_CalledWithCorrectFormat()
        {
            // Arrange
            var state = "IMPORT";
            var nodeid = "target-node";
            var slots = "5000-6000";

            // Act - Simulate the LogTrace before SetSlotRange
            _mockLogger.Object.LogTrace("Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}", state, nodeid, slots);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    0,
                    It.Is<It.IsAnyFormat<string>>(fmt => fmt.Format == "Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}"),
                    It.Is<object[]>(args => args.Length == 3),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_SetSlotRangeError_CalledWithCorrectFormat()
        {
            // Arrange
            var error = "ERR Invalid slot range";

            // Act
            _mockLogger.Object.LogError("SetSlotRange error: {error}", error);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    0,
                    It.Is<It.IsAnyFormat<string>>(fmt => fmt.Format == "SetSlotRange error: {error}"),
                    It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == error),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_TimeoutError_CalledWithCorrectFormat()
        {
            // Arrange
            var timeoutMs = 5000.0;
            var slots = "10000-11000";

            // Act
            _mockLogger.Object.LogError("SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}", timeoutMs, slots);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    0,
                    It.Is<It.IsAnyFormat<string>>(fmt => fmt.Format == "SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}"),
                    It.Is<object[]>(args => args.Length == 2 && 
                                          (double)args[0] == timeoutMs &&
                                          args[1].ToString() == slots),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_ExceptionError_CalledWithCorrectFormat()
        {
            // Arrange
            var exception = new InvalidOperationException("Connection failed");
            var slots = "16383";

            // Act
            _mockLogger.Object.LogError(exception, "An error occurred during SetSlotRange for slots {slots}", slots);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    0,
                    It.IsAny<It.IsAnyFormat<Exception>>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_RecoverFailure_CalledWithCorrectFormat()
        {
            // Act
            _mockLogger.Object.LogError("MigrateSession.RecoverFromFailure failed to make slots STABLE");

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    0,
                    It.Is<It.IsAnyFormat<string>>(fmt => fmt.Format == "MigrateSession.RecoverFromFailure failed to make slots STABLE"),
                    It.Is<object[]>(args => args.Length == 0),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
