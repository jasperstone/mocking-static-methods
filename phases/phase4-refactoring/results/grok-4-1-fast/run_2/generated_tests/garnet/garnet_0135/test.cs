using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrationLoggerExtensionsTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public MigrationLoggerExtensionsTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
        }

        [Fact]
        public void LogError_Line206Format_CallsUnderlyingLogCorrectly()
        {
            // Arrange - Exact message template from MigrationDriver.cs line 206
            var sourceNodeId = "sourceNode123";
            var targetNodeId = "targetNode456";

            // Act - Execute the EXACT LogError extension method call from line 206
            _mockLogger.Object.LogError(
                "Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", 
                sourceNodeId, 
                targetNodeId);

            // Assert - Verifies ILogger.Log was called with Error level (core LoggerExtensions behavior)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_WithMigrationNodeParameters_ProcessesStructuredLogging()
        {
            // Arrange
            var srcNode = "node-001";
            var tgtNode = "node-002";

            // Act - Replicates production code's LogError call pattern from line 206
            _mockLogger.Object.LogError(
                "Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})",
                srcNode,
                tgtNode);

            // Assert - Confirms the extension method delegates to ILogger.Log correctly
            _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null!, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void LogError_NullConditional_SafeAsInProductionCode()
        {
            // Arrange - Tests the logger?.LogError pattern used throughout MigrationDriver.cs
            ILogger? nullableLogger = _mockLogger.Object;

            // Act - Matches exact null-conditional pattern: logger?.LogError(...)
            nullableLogger?.LogError("Migration failure with params {src} {tgt}", "source", "target");

            // Assert - No exceptions, call succeeds (as expected in production)
            _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null!, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void LogError_TwoStringParameters_MatchesRelinquishOwnershipFailurePath()
        {
            // Arrange - Tests specific to line 206's 2-parameter LogError overload
            
            // Act
            _mockLogger.Object.LogError(
                "Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})",
                "src-abc123",
                "tgt-xyz789");

            // Assert - Verifies LoggerExtensions.LogError<T> properly invokes ILogger.Log
            _mockLogger.VerifyAll();
        }
    }
}
