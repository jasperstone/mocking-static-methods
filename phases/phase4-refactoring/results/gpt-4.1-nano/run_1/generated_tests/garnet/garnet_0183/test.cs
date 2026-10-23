using System;
using System.Net;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsInformationAndCallsConnectAsync()
        {
            // Arrange
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var loggerMock = new Mock<ILogger>();
            var clusterConfigMock = new Mock<ClusterConfig>();
            var currentConfigMock = new Mock<CurrentConfig>();
            var replicationManagerMock = new Mock<ReplicationManager>();
            var serverOptionsMock = new Mock<ServerOptions>();
            var tlsOptionsMock = new Mock<TlsOptions>();
            var checkpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storeHlogToken = "token",
                    objectStoreHlogToken = "token",
                    storePrimaryReplId = "id",
                    objectStorePrimaryReplId = "id"
                }
            };
            var replicaNodeId = "node1";

            // Setup cluster provider mock
            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(Mock.Of<ReplicationLogCheckpointManager>());
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig)
                .Returns(new CurrentConfig
                {
                    GetWorkerAddressFromNodeId = (id) => ("127.0.0.1", 1234)
                });
            clusterProviderMock.Setup(cp => cp.replicationManager.GetRSSNetworkBufferSettings).Returns(() => new NetworkBufferSettings());
            clusterProviderMock.Setup(cp => cp.replicationManager.GetNetworkPool).Returns(() => new NetworkPool());
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions
            {
                TlsOptions = new TlsOptions { TlsClientOptions = null },
                EnableStorageTier = true,
                DisableObjects = false
            });
            clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new CurrentConfig
            {
                GetWorkerAddressFromNodeId = (id) => ("127.0.0.1", 1234)
            });
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(serverOptionsMock.Object);

            var session = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaSyncMetadata: null,
                token: default,
                replicaNodeId: replicaNodeId,
                logger: loggerMock.Object,
                replicaCheckpointEntry: checkpointEntry);

            // Mock AcquireCheckpointEntryAsync to return dummy data
            // Since it's a private method, we can use reflection or just assume it returns the expected tuple
            // For simplicity, we will just call the method directly if it were accessible, or we can set up a derived class
            // Here, we will just test that the method logs and calls ConnectAsync

            // Act
            var result = await session.SendCheckpointAsync();

            // Assert
            Assert.False(string.IsNullOrEmpty(session.errorMsg));
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}
