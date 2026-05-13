using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.server;
using Garnet.common;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Garnet.Tests
{
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

            mockServerOptions.Setup(so => so.ReplicaSyncTimeout).Returns(TimeSpan.FromSeconds(10));
            mockServerOptions.Setup(so => so.UseAofNullDevice).Returns(false);
            mockServerOptions.Setup(so => so.FastAofTruncate).Returns(false);
            mockServerOptions.Setup(so => so.OnDemandCheckpoint).Returns(true);

            mockAppendOnlyFile.Setup(aof => aof.BeginAddress).Returns(100);

            var replicaSyncSession = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                logger: mockLogger.Object);

            // Act
            await Assert.ThrowsAsync<Exception>(() => replicaSyncSession.SendCheckpointAsync());

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
