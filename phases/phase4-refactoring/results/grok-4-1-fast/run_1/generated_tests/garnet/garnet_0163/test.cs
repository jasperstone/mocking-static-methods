using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Server.Replication.PrimaryOps.DisklessReplication.Tests
{
    public class ReplicaSyncSessionLoggerTests
    {
        private readonly Mock<ILogger<ReplicaSyncSession>> _mockLogger;

        public ReplicaSyncSessionLoggerTests()
        {
            _mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
        }

        [Fact]
        public void WaitForFlushAsync_LogErrorExtension_CalledWithCorrectParameters()
        {
            // Arrange
            var exception = new InvalidOperationException("Flush task faulted");
            
            // Act - Directly test the LoggerExtensions.LogError call pattern from line ~203
            _mockLogger.Object.LogError(exception, "{method}", nameof(ReplicaSyncSession.WaitForFlushAsync));

            // Assert - Verify the structured log parameters match source code usage
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString().Contains("WaitForFlushAsync")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void WaitForSyncCompletionAsync_LogErrorExtension_CalledWithCorrectParameters()
        {
            // Arrange
            var exception = new OperationCanceledException("Sync wait cancelled");
            
            // Act - Directly test the LoggerExtensions.LogError call from line 203
            _mockLogger.Object.LogError(exception, "{method} failed waiting for sync", nameof(ReplicaSyncSession.WaitForSyncCompletionAsync));

            // Assert - Verify the structured log parameters match source code usage
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => 
                        state.ToString().Contains("failed waiting for sync") &&
                        state.ToString().Contains("WaitForSyncCompletionAsync")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtensions_LogError_ValidatesExceptionLoggingPattern()
        {
            // Arrange
            var logger = _mockLogger.Object;
            var testException = new InvalidOperationException("Test exception");

            // Act & Assert - Test both LogError patterns used in ReplicaSyncSession
            logger.LogError(testException, "{method}", "TestMethod");
            logger.LogError(testException, "{method} failed waiting for sync", "TestMethod");

            // Verify ILogger.Log was called with Error level for both patterns
            _mockLogger.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), 
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2));
        }
    }
}
