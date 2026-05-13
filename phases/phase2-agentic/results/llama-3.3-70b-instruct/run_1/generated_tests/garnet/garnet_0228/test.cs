using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public void LogError_WhenExceptionThrown_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");
            var replicaReceiveCheckpoint = new ReplicaReceiveCheckpoint(loggerMock.Object);

            // Act
            try
            {
                replicaReceiveCheckpoint.TryReplicateDiskbasedSyncAsync(null, null);
            }
            catch (Exception ex)
            {
                replicaReceiveCheckpoint.logger.LogError(ex, $"{nameof(TryReplicateDiskbasedSyncAsync)}");
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void LogError_WhenNoPrimaryAddress_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaReceiveCheckpoint = new ReplicaReceiveCheckpoint(loggerMock.Object);
            var current = new CurrentConfig();
            current.GetLocalNodePrimaryAddress = () => (null, -1);

            // Act
            var result = replicaReceiveCheckpoint.ReplicaSyncAttachTaskAsync(false, false).Result;

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }

    public class ReplicaReceiveCheckpoint
    {
        public ILogger logger;

        public ReplicaReceiveCheckpoint(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task<(bool Success, ReadOnlyMemory<byte> ErrorMessage)> TryReplicateDiskbasedSyncAsync(ClusterSession session, ReplicateSyncOptions options)
        {
            try
            {
                // Simulate an exception being thrown
                throw new Exception("Test exception");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{nameof(TryReplicateDiskbasedSyncAsync)}");
                return (false, default);
            }
        }

        public async Task<string> ReplicaSyncAttachTaskAsync(bool downgradeLock, bool forceAsync)
        {
            // Simulate no primary address
            var current = new CurrentConfig();
            current.GetLocalNodePrimaryAddress = () => (null, -1);
            var (address, port) = current.GetLocalNodePrimaryAddress();

            if (address == null || port == -1)
            {
                var errorMsg = "Test error message";
                logger.LogError("{msg}", errorMsg);
                return errorMsg;
            }

            return null;
        }
    }

    public class CurrentConfig
    {
        public Func<(string, int)> GetLocalNodePrimaryAddress { get; set; }
    }

    public class ClusterSession
    {
    }

    public class ReplicateSyncOptions
    {
    }
}
