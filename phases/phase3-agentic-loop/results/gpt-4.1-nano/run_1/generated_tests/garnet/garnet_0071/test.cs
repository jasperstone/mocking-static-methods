using System;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class GarnetServerNodeTests
    {
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<GarnetClient> _garnetClientMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly EndPoint _endPoint;
        private readonly SslClientAuthenticationOptions _tlsOptions;
        private readonly LightEpoch _epoch;

        public GarnetServerNodeTests()
        {
            _clusterProviderMock = new Mock<ClusterProvider>();
            _garnetClientMock = new Mock<GarnetClient>();
            _loggerMock = new Mock<ILogger>();
            _endPoint = new Mock<EndPoint>().Object;
            _tlsOptions = new SslClientAuthenticationOptions();
            _epoch = new LightEpoch();

            // Setup clusterProvider mock to return necessary properties
            var clusterManagerMock = new Mock<ClusterManager>();
            clusterManagerMock.Setup(cm => cm.ctsGossip).Returns(new CancellationTokenSource());
            clusterManagerMock.Setup(cm => cm.gossipDelay).Returns(TimeSpan.FromMilliseconds(100));
            clusterManagerMock.Setup(cm => cm.clusterTimeout).Returns(TimeSpan.FromSeconds(10));
            clusterManagerMock.Setup(cm => cm.CurrentConfig).Returns(new ClusterConfig("node1"));
            clusterManagerMock.Setup(cm => cm.TryMerge(It.IsAny<ClusterConfig>())).Verifiable();

            var clusterManager = clusterManagerMock.Object;
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManager);
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(new StoreWrapper { serverOptions = new ServerOptions() });
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns((ReplicationManager)null);
        }

        [Fact]
        public async Task LogWarning_IsCalled_WhenGossipAsyncThrows()
        {
            // Arrange
            var node = new GarnetServerNode(_clusterProviderMock.Object, _endPoint, _tlsOptions, _epoch, _loggerMock.Object);

            // Setup the gc mock to throw exception
            var mockGarnetClient = new Mock<GarnetClient>();
            mockGarnetClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(() =>
                {
                    throw new Exception("Test exception");
                });

            // Inject the mock into the node
            var gcField = typeof(GarnetServerNode).GetField("gc", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            gcField.SetValue(node, mockGarnetClient.Object);

            // Setup logger to verify LogWarning
            var logger = new Mock<ILogger>();
            var loggerField = typeof(GarnetServerNode).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(node, logger.Object);

            // Act
            // Call the private method via reflection to simulate the code path
            var method = typeof(GarnetServerNode).GetMethod("GossipAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await method.Invoke(node, new object[] { new byte[] { 1, 2, 3 } });

            // Assert
            logger.Verify(
                x => x.LogCritical(It.IsAny<Exception>(), "GOSSIP faulted processing response"),
                Times.Once);
        }
    }
}
