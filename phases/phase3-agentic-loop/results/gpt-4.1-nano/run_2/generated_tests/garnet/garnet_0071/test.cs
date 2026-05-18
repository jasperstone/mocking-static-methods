using System;
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
        }

        [Fact]
        public async Task LogWarning_IsCalled_WhenGossipAsyncThrows()
        {
            // Arrange
            var node = new GarnetServerNode(_clusterProviderMock.Object, _endPoint, _tlsOptions, _epoch, _loggerMock.Object);
            var configArray = new byte[] { 1, 2, 3 };
            var exception = new Exception("Test exception");
            var mockGarnetClient = new Mock<GarnetClient>();
            mockGarnetClient.Setup(g => g.GossipAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(new byte[] { 4, 5, 6 });
            node.GetType().GetField("gc", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(node, mockGarnetClient.Object);

            // Act
            // Force an exception during GossipAsync to trigger LogWarning
            mockGarnetClient.Setup(g => g.GossipAsync(It.IsAny<byte[]>()))
                .ThrowsAsync(exception);

            // Call the method that contains the line 252
            // We need to simulate the code path that leads to the LogWarning call
            // For that, we need to invoke the internal method or simulate the state
            // Since the method is not directly accessible, we can invoke the private method via reflection
            var methodInfo = typeof(GarnetServerNode).GetMethod("GossipAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await Assert.ThrowsAsync<Exception>(() => (Task)methodInfo.Invoke(node, new object[] { configArray }));

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), "GOSSIP round faulted"),
                Times.Once);
        }
    }
}
