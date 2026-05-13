using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.common;
using Garnet.server;
using System.Net;
using System.Threading.Tasks;
using System.Threading;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsCheckpointSearchCompleted()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var replicaSyncSession = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                logger: loggerMock.Object);

            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(Mock.Of<IReplicationLogCheckpointManager>());
            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Object)).Returns(Mock.Of<IReplicationLogCheckpointManager>());
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig.GetWorkerAddressFromNodeId(It.IsAny<string>())).Returns(("127.0.0.1", 12345));
            clusterProviderMock.Setup(cp => cp.replicationManager.GetRSSNetworkBufferSettings).Returns(Mock.Of<NetworkBufferSettings>());
            clusterProviderMock.Setup(cp => cp.replicationManager.GetNetworkPool).Returns(Mock.Of<INetworkPool>());
            clusterProviderMock.Setup(cp => cp.serverOptions.TlsOptions).Returns(Mock.Of<TlsOptions>());
            clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("username");
            clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("password");

            var replicaCheckpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storePrimaryReplId = "storePrimaryReplId",
                    objectStorePrimaryReplId = "objectStorePrimaryReplId"
                }
            };

            replicaSyncSession.replicaCheckpointEntry = replicaCheckpointEntry;
            replicaSyncSession.replicaNodeId = "replicaNodeId";

            // Act
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Checkpoint search completed")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
