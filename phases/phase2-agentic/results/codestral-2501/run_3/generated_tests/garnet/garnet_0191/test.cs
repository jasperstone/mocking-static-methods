using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.server;
using Garnet.common;
using System.Threading.Tasks;
using System.Threading;
using System;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task SendCheckpointAsync_ShouldLogError_WhenSyncFromAofAddressIsLessThanBeginAddress()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockReplicationManager = new Mock<ReplicationManager>();
        var mockServerOptions = new Mock<ServerOptions>();
        var mockAppendOnlyFile = new Mock<AppendOnlyFile>();

        mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(mockServerOptions.Object);
        mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
        mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
        mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);

        var replicaSyncSession = new ReplicaSyncSession(
            mockStoreWrapper.Object,
            mockClusterProvider.Object,
            logger: mockLogger.Object);

        var localEntry = new CheckpointEntry();
        var beginAddress = 100;
        var checkpointAofBeginAddress = 200;
        var syncFromAofAddress = 50;

        mockReplicationManager.Setup(rm => rm.PrimaryReplId).Returns("primaryReplId");
        mockAppendOnlyFile.Setup(aof => aof.BeginAddress).Returns(beginAddress);
        mockServerOptions.Setup(so => so.ReplicaSyncTimeout).Returns(TimeSpan.FromSeconds(10));
        mockServerOptions.Setup(so => so.UseAofNullDevice).Returns(false);
        mockServerOptions.Setup(so => so.FastAofTruncate).Returns(false);
        mockServerOptions.Setup(so => so.OnDemandCheckpoint).Returns(true);

        var gcsMock = new Mock<GarnetClientSession>();
        gcsMock.Setup(gcs => gcs.ExecuteBeginReplicaRecover(
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<long>(),
            It.IsAny<long>()))
            .ReturnsAsync(syncFromAofAddress.ToString());

        // Act
        await Assert.ThrowsAsync<Exception>(() => replicaSyncSession.SendCheckpointAsync());

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("syncFromAofAddress: 50 < beginAofAddress: 100")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
