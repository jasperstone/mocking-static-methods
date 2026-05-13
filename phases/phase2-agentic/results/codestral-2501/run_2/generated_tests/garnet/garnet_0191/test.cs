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
    public async Task LogError_WhenSyncFromAofAddressIsLessThanBeginAddress()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockReplicationManager = new Mock<ReplicationManager>();
        var mockServerOptions = new Mock<ServerOptions>();
        var mockAppendOnlyFile = new Mock<AppendOnlyFile>();

        mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
        mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(mockServerOptions.Object);
        mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
        mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);

        var replicaSyncSession = new ReplicaSyncSession(
            mockStoreWrapper.Object,
            mockClusterProvider.Object,
            logger: mockLogger.Object
        );

        var localEntry = new CheckpointEntry();
        var beginAddress = 100;
        var checkpointAofBeginAddress = 200;
        var syncFromAofAddress = 50;

        mockAppendOnlyFile.Setup(aof => aof.BeginAddress).Returns(beginAddress);
        mockReplicationManager.Setup(rm => rm.PrimaryReplId).Returns("primaryReplId");
        mockServerOptions.Setup(so => so.ReplicaSyncTimeout).Returns(TimeSpan.FromSeconds(10));

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
