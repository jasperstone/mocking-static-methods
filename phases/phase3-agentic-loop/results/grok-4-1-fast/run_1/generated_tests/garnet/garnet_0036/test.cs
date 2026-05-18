using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class ClusterManagerSlotStateTests
    {
        [Fact]
        public void LogTraceExtension_WhenEnabled_LogsCorrectMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<object>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            var slot = 12345;
            var nodeid = "remote-node-123";

            // Act
            mockLogger.Object.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", slot, nodeid);

            // Assert - verify the Log method call made by LogTrace extension
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogTraceExtension_WhenTraceDisabled_DoesNotLog()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<object>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            mockLogger.Object.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", 12345, "node123");

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void LogTraceExtension_WithNullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger<object> logger = null;

            // Act & Assert - null-conditional prevents exception
            logger?.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", 12345, "node123");
        }

        [Fact]
        public void LogTraceExtension_UsesCorrectLogLevel()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<object>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            mockLogger.Object.LogTrace("test");

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,  // Specifically verify Trace level
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
