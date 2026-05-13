using System;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Server.Replication.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public void ReplicaSyncAttachTaskAsync_LogsError_WhenNoPrimaryAssigned()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var currentConfigMock = new Mock<ClusterConfig>();

            clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(currentConfigMock.Object);
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(new ReplicationManager());
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions { EnableFastCommit = false });
            clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);

            currentConfigMock.Setup(cc => cc.GetLocalNodePrimaryAddress())
                .Returns((string)null, -1);

            var replicationManager = new ReplicationManager
            {
                clusterProvider = clusterProviderMock.Object,
                logger = loggerMock.Object
            };

            // Act
            var task = replicationManager.ReplicaSyncAttachTaskAsync(false, false);
            task.Wait();

            // Assert
            loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.Is<object[]>(args => args.Length == 1 && args[0] is string errorMsg && errorMsg == Encoding.ASCII.GetString(CmdStrings.RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR))),
                Times.Once);
        }
    }
}
