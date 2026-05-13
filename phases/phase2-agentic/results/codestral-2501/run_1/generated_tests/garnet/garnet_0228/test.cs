using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.cluster.Server.Replication;
using Garnet.common;
using Garnet.server;
using Tsavorite.core;

namespace Garnet.cluster.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_ShouldLogError_WhenPrimaryAddressIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var replicationManagerMock = new Mock<ReplicationManager>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var appendOnlyFileMock = new Mock<AppendOnlyFile>();
            var currentConfigMock = new Mock<ClusterConfig>();
            var sessionMock = new Mock<ClusterSession>();
            var options = new ReplicateSyncOptions();

            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("username");
            clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("password");

            storeWrapperMock.Setup(sw => sw.appendOnlyFile).Returns(appendOnlyFileMock.Object);

            currentConfigMock.Setup(cc => cc.GetLocalNodePrimaryAddress()).Returns((null, -1));

            clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(currentConfigMock.Object);

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object, storeWrapperMock.Object);

            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
