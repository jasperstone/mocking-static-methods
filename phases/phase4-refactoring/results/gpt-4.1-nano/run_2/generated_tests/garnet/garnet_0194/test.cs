using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionLoggingTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsInformationAndErrors()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockTlsOptions = new Mock<TlsOptions>();
            var mockCurrentConfig = new Mock<ClusterConfig>();

            // Setup minimal necessary properties and methods
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.ClusterUsername).Returns("user");
            mockClusterProvider.Setup(cp => cp.ClusterPassword).Returns("pass");
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new ServerOptions
            {
                TlsOptions = new TlsOptions { TlsClientOptions = null },
                ReplicaSyncTimeout = TimeSpan.FromSeconds(10)
            });
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(new Mock<ReplicationLogCheckpointManager>().Object);
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new ClusterConfig());

            // Setup store wrapper
            mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(new ServerOptions
            {
                ReplicaSyncTimeout = TimeSpan.FromSeconds(10)
            });

            // Instantiate session
            var session = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                logger: mockLogger.Object);

            // Act
            await session.SendCheckpointAsync();

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.AtLeastOnce);
            mockLogger.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<object>()),
                Times.Never);
        }
    }
}
