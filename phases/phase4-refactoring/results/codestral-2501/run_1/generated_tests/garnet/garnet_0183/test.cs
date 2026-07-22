using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Garnet.common;
using Garnet.server;
using Garnet.client;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task SendCheckpointAsync_LogsCheckpointSearchCompleted()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockReplicationManager = new Mock<ReplicationManager>();
        var mockServerOptions = new Mock<ServerOptions>();
        var mockClusterManager = new Mock<ClusterManager>();
        var mockCurrentConfig = new Mock<CurrentConfig>();

        mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(new ReplicationLogCheckpointManager());
        mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Object)).Returns(new ReplicationLogCheckpointManager());
        mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
        mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
        mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);
        mockClusterProvider.Setup(cp => cp.ClusterUsername).Returns("username");
        mockClusterProvider.Setup(cp => cp.ClusterPassword).Returns("password");

        mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(mockCurrentConfig.Object);
        mockCurrentConfig.Setup(cc => cc.GetWorkerAddressFromNodeId(It.IsAny<string>())).Returns((IPAddress.Parse("127.0.0.1"), 1234));

        var replicaSyncSession = new ReplicaSyncSession(
            mockStoreWrapper.Object,
            mockClusterProvider.Object,
            replicaNodeId: "replicaNodeId",
            replicaCheckpointEntry: new CheckpointEntry(),
            logger: mockLogger.Object
        );

        // Act
        await replicaSyncSession.SendCheckpointAsync();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Checkpoint search completed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
