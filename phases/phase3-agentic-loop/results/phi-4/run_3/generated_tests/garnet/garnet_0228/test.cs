using System;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public void LogError_ShouldBeCalled_WhenNoPrimaryAssigned()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var replicationManagerMock = new Mock<ReplicationManager>();

            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(new ServerOptions { EnableFastCommit = false });
            clusterProviderMock.SetupGet(c => c.ClusterUsername).Returns("username");
            clusterProviderMock.SetupGet(c => c.ClusterPassword).Returns("password");

            clusterManagerMock.Setup(c => c.CurrentConfig).Returns(new ClusterConfig
            {
                LocalNodeId = 1,
                GetLocalNodePrimaryAddress = () => (null, -1)
            });

            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object, storeWrapperMock.Object);

            // Act
            var result = replicationManager.ReplicaSyncAttachTaskAsync(false, false).Result;

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == Encoding.ASCII.GetString(CmdStrings.RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
