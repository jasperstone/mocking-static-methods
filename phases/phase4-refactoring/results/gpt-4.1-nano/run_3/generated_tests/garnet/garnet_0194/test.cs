using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.server;
using Garnet.common;

namespace Garnet.tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsInformationAtLine361()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockCheckpointEntry = new CheckpointEntry();

            // Setup necessary properties and methods
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(() => null);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockClusterProvider.Setup(cp => cp.ClusterUsername).Returns("user");
            mockClusterProvider.Setup(cp => cp.ClusterPassword).Returns("pass");
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(() => null);
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new ClusterConfig());

            // Setup storeWrapper with serverOptions
            var serverOptions = new ServerOptions
            {
                ReplicaSyncTimeout = TimeSpan.FromSeconds(10)
            };
            mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(serverOptions);

            // Instantiate the session
            var session = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                logger: mockLogger.Object);

            // Act
            await session.SendCheckpointAsync();

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("requesting checkpoint")),
                    It.IsAny<object[]>()),
                Times.AtLeastOnce);
        }
    }
}
