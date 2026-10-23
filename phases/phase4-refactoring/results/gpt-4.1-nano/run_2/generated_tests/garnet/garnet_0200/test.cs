using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.server;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionLoggingTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsInformationAndErrors()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var configMock = new Mock<ClusterConfig>();
            var currentConfig = new Mock<ClusterConfig>();
            var networkBufferSettings = new Mock<INetworkBufferSettings>();
            var networkPool = new Mock<INetworkPool>();
            var serverOptions = new Mock<IServerOptions>();
            var tlsOptions = new Mock<ITlsClientOptions>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var checkpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1
                }
            };

            // Setup mocks
            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(new Mock<IReplicationLogCheckpointManager>().Object);
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(currentConfig.Object);
            currentConfig.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 1234));
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(serverOptions.Object);
            serverOptions.Setup(so => so.TlsOptions).Returns(new TlsOptions { TlsClientOptions = tlsOptions.Object });
            clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(new Mock<IReplicationLogCheckpointManager>().Object);
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(currentConfig.Object);
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new Mock<IServerOptions>().Object);
            var session = new ReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProviderMock.Object,
                replicaCheckpointEntry: checkpointEntry,
                logger: loggerMock.Object);

            // Act
            await session.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("requesting checkpoint")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);

            // Simulate an error log
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to validate metadata")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
