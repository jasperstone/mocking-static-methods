using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarning_WhenReceivedGossipFromUnknownNode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var clientMock = new Mock<GarnetClient>();
            var oldConfig = new ClusterConfig { LocalNodePrimaryId = "primaryId" };
            var newConfig = new ClusterConfig();
            var replicaId = "unknownReplicaId";
            var configByteArray = new byte[0];
            var failoverTimeout = TimeSpan.FromSeconds(5);
            var cts = new CancellationTokenSource();

            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(new Mock<ClusterManager>().Object);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(new Mock<ServerOptions>().Object);
            clusterProviderMock.SetupGet(c => c.ClusterUsername).Returns("username");
            clusterProviderMock.SetupGet(c => c.ClusterPassword).Returns("password");

            var session = new ReplicaFailoverSession(clusterProviderMock.Object, loggerMock.Object, oldConfig, newConfig, failoverTimeout, cts.Token);

            // Act
            await session.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(It.IsAny<string>(), It.Is<object[]>(args => args[0].ToString() == "unknownReplicaId")),
                Times.Once);
        }
    }
}
