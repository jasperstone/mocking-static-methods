using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.common;
using Garnet.server;
using System.Threading.Tasks;
using System.Threading;
using System;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task LogError_When_SyncFromAofAddress_Less_Than_BeginAofAddress()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockReplicationManager = new Mock<ReplicationManager>();
        var mockAppendOnlyFile = new Mock<AppendOnlyFile>();

        mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
        mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
        mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions { UseAofNullDevice = false, FastAofTruncate = false, OnDemandCheckpoint = true });

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

        // Act
        await replicaSyncSession.SendCheckpointAsync();

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("syncFromAofAddress: 50 < beginAofAddress: 100")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
