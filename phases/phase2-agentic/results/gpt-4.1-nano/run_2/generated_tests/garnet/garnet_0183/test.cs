using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;
using Garnet.server;
using Garnet.common;
using Garnet.client;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        private Mock<ClusterProvider> _clusterProviderMock;
        private Mock<ILogger> _loggerMock;
        private Mock<StoreWrapper> _storeWrapperMock;
        private Mock<ClusterManager> _clusterManagerMock;
        private Mock<ReplicationManager> _replicationManagerMock;
        private Mock<ServerOptions> _serverOptionsMock;
        private Mock<ITlsOptions> _tlsOptionsMock;
        private Mock<SyncMetadata> _replicaCheckpointEntryMock;
        private Mock<CheckpointEntry> _checkpointEntryMock;
        private Mock<ClusterConfig> _clusterConfigMock;
        private Mock<INetworkBufferSettings> _networkBufferSettingsMock;
        private Mock<INetworkPool> _networkPoolMock;

        public ReplicaSyncSessionTests()
        {
            _clusterProviderMock = new Mock<ClusterProvider>();
            _loggerMock = new Mock<ILogger>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _serverOptionsMock = new Mock<ServerOptions>();
            _tlsOptionsMock = new Mock<ITlsOptions>();
            _replicaCheckpointEntryMock = new Mock<SyncMetadata>();
            _checkpointEntryMock = new Mock<CheckpointEntry>();
            _clusterConfigMock = new Mock<ClusterConfig>();
            _networkBufferSettingsMock = new Mock<INetworkBufferSettings>();
            _networkPoolMock = new Mock<INetworkPool>();
        }

        [Fact]
        public async Task SendCheckpointAsync_ShouldLogInformationAndReturnTrue_WhenSuccessful()
        {
            // Arrange
            var replicaNodeId = "node1";
            var replicaAssignedPrimaryId = "primary1";

            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockTlsOptions = new Mock<ITlsOptions>();
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>();

            // Setup cluster provider to return mock objects
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(Mock.Of<ReplicationLogCheckpointManager>());
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(Mock.Of<ClusterManager>());
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig).Returns(Mock.Of<ClusterConfig>());
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(Mock.Of<ReplicationManager>());
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions { TlsOptions = new TlsOptions { TlsClientOptions = null }, EnableStorageTier = false, DisableObjects = false });
            mockClusterProvider.Setup(cp => cp.ClusterUsername).Returns("user");
            mockClusterProvider.Setup(cp => cp.ClusterPassword).Returns("pass");
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>())).Returns(Mock.Of<ReplicationLogCheckpointManager>());

            // Setup cluster config to return address and port
            var mockCurrentConfig = new Mock<ClusterConfig>();
            mockCurrentConfig.Setup(c => c.GetWorkerAddressFromNodeId(replicaNodeId))
                .Returns(("127.0.0.1", 1234));
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig).Returns(mockCurrentConfig.Object);

            // Setup the cluster provider to return the mock objects
            var session = new ReplicaSyncSession(
                storeWrapper: mockStoreWrapper.Object,
                clusterProvider: mockClusterProvider.Object,
                replicaNodeId: replicaNodeId,
                replicaAssignedPrimaryId: replicaAssignedPrimaryId,
                replicaCheckpointEntry: new CheckpointEntry
                {
                    metadata = new CheckpointMetadata
                    {
                        storeVersion = 1,
                        objectStoreVersion = 1
                    }
                },
                logger: mockLogger.Object);

            // Act
            var result = await session.SendCheckpointAsync();

            // Assert
            Assert.True(result);
            mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
            mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
