using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.common;
using System.Threading.Tasks;
using System.Threading;
using System;
using Garnet.client;
using System.Net;
using Garnet.server;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsInformation_WhenSendingCheckpointMetadata()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockReplicationLogCheckpointManager = new Mock<ReplicationLogCheckpointManager>();
            var mockGarnetClientSession = new Mock<GarnetClientSession>(new IPEndPoint(IPAddress.Loopback, 0), null, null, null, null, null, null);

            mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(new ServerOptions { ReplicaSyncTimeout = TimeSpan.FromSeconds(10) });
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Main)).Returns(mockReplicationLogCheckpointManager.Object);
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(StoreType.Object)).Returns(mockReplicationLogCheckpointManager.Object);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(new Mock<ClusterManager>().Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions { SegmentSizeBits = () => 10 });
            mockClusterProvider.Setup(cp => cp.ClusterUsername).Returns("username");
            mockClusterProvider.Setup(cp => cp.ClusterPassword).Returns("password");

            var replicaSyncSession = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                logger: mockLogger.Object);

            var fileToken = Guid.NewGuid();
            var fileType = CheckpointFileType.STORE_SNAPSHOT;

            mockGarnetClientSession.Setup(gcs => gcs.ExecuteSendCkptMetadata(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()))
                .ReturnsAsync("OK");

            // Act
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    "<Begin sending checkpoint metadata {fileToken} {fileType}",
                    fileToken,
                    fileType),
                Times.Once);

            mockLogger.Verify(
                logger => logger.LogInformation(
                    "<Complete sending checkpoint metadata {fileToken} {fileType}",
                    fileToken,
                    fileType),
                Times.Once);
        }
    }
}
