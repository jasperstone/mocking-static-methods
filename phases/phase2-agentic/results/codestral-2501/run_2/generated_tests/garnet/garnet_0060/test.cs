using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.client;
using Garnet.common;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarning_WhenReplicaOfRespIsNotOK()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            var mockClient = new Mock<GarnetClient>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockOldConfig = new Mock<ClusterConfig>();
            var mockNewConfig = new Mock<ClusterConfig>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockTlsOptions = new Mock<TlsOptions>();
            var mockTlsClientOptions = new Mock<TlsClientOptions>();
            var mockCancellationTokenSource = new Mock<CancellationTokenSource>();

            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.Setup(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.Setup(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockServerOptions.Setup(so => so.TlsOptions).Returns(mockTlsOptions.Object);
            mockTlsOptions.Setup(to => to.TlsClientOptions).Returns(mockTlsClientOptions.Object);

            var failoverSession = new FailoverSession(
                mockClusterProvider.Object,
                mockOldConfig.Object,
                mockNewConfig.Object,
                mockLogger.Object,
                mockCancellationTokenSource.Object.Token,
                TimeSpan.FromSeconds(30)
            );

            var replicaId = "replica1";
            var configByteArray = new byte[] { 1, 2, 3, 4 };

            mockClient.Setup(c => c.ReplicaOf(It.IsAny<string>(), It.IsAny<int>()))
                      .ReturnsAsync("NOT_OK");

            // Act
            await failoverSession.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IssueAttachReplicas Error: replica1 NOT_OK")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
