using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.common;
using Garnet.server;
using System.Threading.Tasks;
using System.Threading;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task AcquireCheckpointEntryAsync_LogsInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);

            var replicaSyncSession = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                logger: mockLogger.Object);

            // Act
            await replicaSyncSession.AcquireCheckpointEntryAsync();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AcquireCheckpointEntry iteration")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.AtLeastOnce);
        }
    }
}
