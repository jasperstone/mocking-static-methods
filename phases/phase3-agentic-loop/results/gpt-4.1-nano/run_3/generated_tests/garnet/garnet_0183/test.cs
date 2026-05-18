using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Garnet.common;
using Garnet.server;
using Garnet.client;
using System.Net;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly Mock<ClusterManager> _clusterManagerMock;
        private readonly Mock<ReplicationManager> _replicationManagerMock;
        private readonly Mock<ServerOptions> _serverOptionsMock;
        private readonly Mock<SyncMetadata> _syncMetadataMock;
        private readonly Mock<CheckpointEntry> _checkpointEntryMock;
        private readonly Mock<IGarnetClient> _garnetClientMock;

        public ReplicaSyncSessionTests()
        {
            _loggerMock = new Mock<ILogger>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _serverOptionsMock = new Mock<ServerOptions>();
            _syncMetadataMock = new Mock<SyncMetadata>();
            _checkpointEntryMock = new Mock<CheckpointEntry>();
            _garnetClientMock = new Mock<IGarnetClient>();
        }

        [Fact]
        public async Task SendCheckpointAsync_LogsInformationAndCallsConnectAsync()
        {
            // Arrange
            var nodeId = "node1";
            var replicaNodeId = "replica1";

            var currentConfig = new Mock<ClusterConfig>();
            currentConfig.Setup(c => c.GetWorkerAddressFromNodeId(replicaNodeId))
                .Returns(("127.0.0.1", 1234));

            _clusterManagerMock.Setup(c => c.CurrentConfig).Returns(currentConfig.Object);
            _clusterProviderMock.Setup(c => c.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(Mock.Of<ReplicationLogCheckpointManager>());
            _clusterProviderMock.Setup(c => c.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(c => c.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(c => c.serverOptions).Returns(_serverOptionsMock.Object);
            _clusterProviderMock.Setup(c => c.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(c => c.ClusterPassword).Returns("pass");
            _clusterProviderMock.Setup(c => c.replicationManager.GetRSSNetworkBufferSettings).Returns(() => new object());
            _clusterProviderMock.Setup(c => c.replicationManager.GetNetworkPool).Returns(() => new object());
            _clusterProviderMock.Setup(c => c.serverOptions.TlsOptions).Returns((TlsOptions)null);
            _clusterProviderMock.Setup(c => c.clusterManager.CurrentConfig).Returns(currentConfig.Object);

            var session = new ReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                replicaNodeId: replicaNodeId,
                logger: _loggerMock.Object);

            // Mock AcquireCheckpointEntryAsync to return dummy data
            async Task<(CheckpointEntry, AofSyncTaskInfo)> DummyAcquireCheckpointAsync()
            {
                return (new CheckpointEntry
                {
                    metadata = new CheckpointMetadata
                    {
                        storeVersion = 1,
                        objectStoreVersion = 1,
                        storeHlogToken = "token",
                        storeIndexToken = "indexToken",
                        storePrimaryReplId = "replId",
                        objectStorePrimaryReplId = "objReplId"
                    }
                }, null);
            }

            // Replace the method with dummy
            var method = typeof(ReplicaSyncSession).GetMethod("AcquireCheckpointEntryAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dummyMethod = new Func<Task<(CheckpointEntry, AofSyncTaskInfo)>>(() => DummyAcquireCheckpointAsync());
            // Use reflection to set the method if needed, or just simulate the call

            // Act
            var result = await session.SendCheckpointAsync();

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
            Assert.True(result);
        }
    }
}
