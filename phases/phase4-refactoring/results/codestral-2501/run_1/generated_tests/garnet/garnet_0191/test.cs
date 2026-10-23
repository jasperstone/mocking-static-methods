using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_ShouldLogError_WhenSyncFromAofAddressIsLessThanBeginAddress()
        {
            // Arrange
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockLogger = new Mock<ILogger>();
            var mockGcs = new Mock<GarnetClientSession>(new IPEndPoint(IPAddress.Loopback, 0), null, null, null, null, null, mockLogger.Object);
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockAppendOnlyFile = new Mock<AppendOnlyFile>();

            mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(mockServerOptions.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockReplicationManager.Setup(rm => rm.PrimaryReplId).Returns("primaryReplId");
            mockServerOptions.Setup(so => so.ReplicaSyncTimeout).Returns(TimeSpan.FromSeconds(10));
            mockAppendOnlyFile.Setup(aof => aof.BeginAddress).Returns(100);

            var replicaSyncSession = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                logger: mockLogger.Object);

            var localEntry = new CheckpointEntry();
            var beginAddress = 50L;
            var checkpointAofBeginAddress = 50L;
            var replayAOF = false;
            var skipLocalMainStoreCheckpoint = false;
            var skipLocalObjectStoreCheckpoint = false;
            var replicaNodeId = "replicaNodeId";
            var syncFromAofAddress = 40L;

            mockGcs.Setup(gcs => gcs.ExecuteBeginReplicaRecover(
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<long>(),
                It.IsAny<long>()))
                .ReturnsAsync(syncFromAofAddress.ToString());

            // Act
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
