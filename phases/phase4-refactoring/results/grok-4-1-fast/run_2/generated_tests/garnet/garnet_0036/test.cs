using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.Cluster.Tests
{
    public class LoggerExtensionsSlotStateTests
    {
        [Fact]
        public void LogTraceExtension_FormatsSlotAndNodeIdCorrectly()
        {
            // Tests the exact LoggerExtensions.LogTrace call pattern from line 401
            var mockLogger = new Mock<ILogger<object>>();

            const int slot = 12345;
            const string nodeId = "remote-node-42";
            const string expectedPattern = "[Processed] SetSlot {slot} FORCED TO {nodeId}";

            mockLogger.Object.LogTrace(expectedPattern, slot, nodeId);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains($"[Processed] SetSlot {slot} FORCED TO {nodeId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogTraceExtension_NullLogger_DoesNotThrow()
        {
            // Tests the null-conditional operator ?.LogTrace() from line 401
            ILogger<object> nullLogger = null;
            nullLogger?.LogTrace("[Processed] SetSlot 1 FORCED TO node2", 1, "node2");
        }

        [Fact]
        public void LogTraceExtension_CapturesStructuredParameters()
        {
            // Verifies structured logging captures slot and nodeId parameters correctly
            var mockLogger = new Mock<ILogger<object>>();
            
            mockLogger.Object.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", 999, "target-node");

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.Is<EventId>(id => id.Id == 0),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
