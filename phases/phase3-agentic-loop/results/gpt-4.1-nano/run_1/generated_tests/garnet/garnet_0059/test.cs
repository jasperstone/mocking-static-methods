using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.client;

namespace Garnet.Tests
{
    public class FailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsCriticalOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var clientMock = new Mock<GarnetClient>();

            var failoverSession = new FailoverSession
            {
                logger = loggerMock.Object,
                clusterProvider = clusterProviderMock.Object,
                cts = new CancellationTokenSource(),
                failoverTimeout = TimeSpan.FromSeconds(1),
                oldConfig = new ClusterConfig { LocalNodePrimaryId = "primary", LocalNodeId = "node1" },
                // Additional setup as needed
            };

            // Setup clusterProvider to return current config
            var currentConfig = new ClusterConfig { LocalNodeId = "node1" };
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(currentConfig);

            // Setup primaryClient
            failoverSession.primaryClient = clientMock.Object;

            // Setup client.GossipAsync to throw
            clientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).ThrowsAsync(new Exception("Gossip error"));

            // Act
            await failoverSession.BroadcastConfigAndRequestAttachAsync("replicaId", new byte[] { 1, 2, 3 });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Gossip error")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
