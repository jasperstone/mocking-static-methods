using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicaSyncSession>>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterConfigMock = new Mock<ClusterConfig>();
            var currentConfigMock = new Mock<ClusterConfig>();
            var replicationManagerMock = new Mock<ReplicationManager>();
            var serverOptionsMock = new Mock<ServerOptions>();
            var tlsOptionsMock = new Mock<TlsOptions>();
            var networkBufferSettings = new object();
            var networkPool = new object();

            // Setup clusterProvider to return mocks
            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(new Mock<ReplicationLogCheckpointManager>().Object);
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(new Mock<ClusterManager>().Object);
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new Mock<ClusterConfig>().Object);
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions
            {
                TlsOptions = new TlsOptions { TlsClientOptions = null },
                EnableStorageTier = false,
                DisableObjects = false
            });
            clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            clusterProviderMock.Setup(cp => cp.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 1234));

            // Setup replicationManager to return dummy network settings
            replicationManagerMock.Setup(rm => rm.GetRSSNetworkBufferSettings).Returns(new object());
            replicationManagerMock.Setup(rm => rm.GetNetworkPool).Returns(new object());

            // Setup clusterConfig to return current config
            currentConfigMock.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 1234));
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(currentConfigMock.Object);

            // Create a dummy CheckpointEntry
            var checkpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storePrimaryReplId = "id",
                    objectStorePrimaryReplId = "id"
                }
            };

            // Instantiate ReplicaSyncSession
            var session = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaCheckpointEntry: checkpointEntry,
                logger: loggerMock.Object);

            // Mock AcquireCheckpointEntryAsync to return dummy data
            var dummyLocalEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storePrimaryReplId = "id",
                    objectStorePrimaryReplId = "id"
                }
            };
            // Use reflection to invoke SendCheckpointAsync
            var method = typeof(ReplicaSyncSession).GetMethod("SendCheckpointAsync");
            var task = (Task<bool>)method.Invoke(session, null);

            // Act
            await session.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Replica replicaId")),
                    It.IsAny<object[]>()),
                Times.AtLeastOnce);

            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object>()),
                Times.Never);
        }
    }
}
