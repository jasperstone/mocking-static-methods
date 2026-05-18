using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.common;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogError_WhenSyncFromAofAddressLessThanBeginAddress()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockAppendOnlyFile = new Mock<AppendOnlyFile>();

            mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions { ReplicaSyncTimeout = TimeSpan.FromSeconds(10) });

            var replicaSyncSession = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                logger: mockLogger.Object);

            var localEntry = new CheckpointEntry();
            var beginAddress = 100L;
            var checkpointAofBeginAddress = 200L;
            var syncFromAofAddress = 50L;

            mockAppendOnlyFile.Setup(aof => aof.BeginAddress).Returns(beginAddress);
            mockReplicationManager.Setup(rm => rm.PrimaryReplId).Returns("primaryReplId");
            mockReplicationManager.Setup(rm => rm.ExecuteBeginReplicaRecover(
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<long>(),
                It.IsAny<long>()))
                .Returns(Task.FromResult(syncFromAofAddress.ToString()));

            // Act
            await replicaSyncSession.SendCheckpointAsync();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("syncFromAofAddress: 50 < beginAofAddress: 100")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
