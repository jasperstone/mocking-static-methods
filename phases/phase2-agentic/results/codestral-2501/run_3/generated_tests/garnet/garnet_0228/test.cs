using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text;
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
            var serverOptionsMock = new Mock<ServerOptions>();
            var currentConfigMock = new Mock<ClusterConfig>();

            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(serverOptionsMock.Object);
            clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            storeWrapperMock.Setup(sw => sw.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(currentConfigMock.Object);
            currentConfigMock.Setup(cc => cc.GetLocalNodePrimaryAddress()).Returns((null, -1));

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);

            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(null, new ReplicateSyncOptions());

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);

            Assert.False(result.Success);
            Assert.Equal(Encoding.ASCII.GetString(CmdStrings.RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR), Encoding.ASCII.GetString(result.ErrorMessage.ToArray()));
        }
    }
}
