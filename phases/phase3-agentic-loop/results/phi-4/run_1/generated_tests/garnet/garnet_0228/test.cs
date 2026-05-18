using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public async Task LogErrorIsCalled_WhenAddressIsNullOrPortIsInvalid()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var replicaReceiveCheckpoint = new ReplicaReceiveCheckpoint(
                clusterProviderMock.Object,
                storeWrapperMock.Object,
                loggerMock.Object);

            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig)
                .Returns(new ClusterConfig
                {
                    GetLocalNodePrimaryAddress = () => (null, -1)
                });

            // Act
            var result = await replicaReceiveCheckpoint.ReplicaSyncAttachTaskAsync(false, false);

            // Assert
            loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains(nameof(ReplicaReceiveCheckpoint.TryReplicateDiskbasedSyncAsync)))),
                Times.Once);

            Assert.Equal(Encoding.ASCII.GetString(CmdStrings.RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR), result);
        }
    }

    // Mock classes for testing
    public class ClusterProvider
    {
        public ClusterManager clusterManager { get; set; }
        public ReplicationManager replicationManager { get; set; }
        public ServerOptions serverOptions { get; set; }
    }

    public class ClusterManager
    {
        public ClusterConfig CurrentConfig { get; set; }
    }

    public class ClusterConfig
    {
        public Func<(string address, int port)> GetLocalNodePrimaryAddress { get; set; }
    }

    public class ReplicationManager
    {
        public int ReplicationOffset { get; set; }
    }

    public class ServerOptions
    {
        public bool EnableFastCommit { get; set; }
    }

    public class StoreWrapper
    {
        public AppendOnlyFile appendOnlyFile { get; set; }
    }

    public class AppendOnlyFile
    {
        public Task CommitAsync() => Task.CompletedTask;
        public Task WaitForCommitAsync() => Task.CompletedTask;
    }

    public class ReplicaReceiveCheckpoint
    {
        private readonly ClusterProvider clusterProvider;
        private readonly StoreWrapper storeWrapper;
        private readonly ILogger logger;

        public ReplicaReceiveCheckpoint(ClusterProvider clusterProvider, StoreWrapper storeWrapper, ILogger logger)
        {
            this.clusterProvider = clusterProvider;
            this.storeWrapper = storeWrapper;
            this.logger = logger;
        }

        public async Task<string> ReplicaSyncAttachTaskAsync(bool downgradeLock, bool forceAsync)
        {
            if (forceAsync)
            {
                await Task.Yield();
            }

            var current = clusterProvider.clusterManager.CurrentConfig;
            var (address, port) = current.GetLocalNodePrimaryAddress();

            if (address == null || port == -1)
            {
                var errorMsg = Encoding.ASCII.GetString(CmdStrings.RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR);
                logger.LogError(new Exception(), $"{nameof(TryReplicateDiskbasedSyncAsync)}");
                return errorMsg;
            }

            // Simulate other operations
            return string.Empty;
        }

        internal void TryReplicateDiskbasedSyncAsync()
        {
            // Simulated method for logging
        }
    }

    public static class CmdStrings
    {
        public static readonly byte[] RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR = Encoding.ASCII.GetBytes("Error: Not assigned primary");
    }
}
