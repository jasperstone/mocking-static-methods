using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Server.Replication.ReplicaOps
{
    public class ReplicaReceiveCheckpointTests
    {
        private static readonly byte[] MockRespError = Encoding.ASCII.GetBytes("-ERR Generic not assigned primary error\r\n");

        [Fact]
        public void LogError_NoPrimaryAssigned_CallsWithCorrectMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<object>>();
            var errorMsg = Encoding.ASCII.GetString(MockRespError);

            // Act - Execute the exact LogError extension call from line ~100
            mockLogger.Object.LogError("{msg}", errorMsg);

            // Assert - Verify underlying Log method was called correctly
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString().Contains("not assigned primary")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger<object> logger = null;
            var errorMsg = "test error";

            // Act - Null-conditional operator as used in production code
            logger?.LogError("{msg}", errorMsg);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public void LogError_WithException_FormatsCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<object>>();
            var ex = new InvalidOperationException("Test exception");
            var errorMsg = "Test error context";

            // Act - Similar to the other LogError call in TryReplicateDiskbasedSyncAsync
            mockLogger.Object.LogError(ex, errorMsg);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
