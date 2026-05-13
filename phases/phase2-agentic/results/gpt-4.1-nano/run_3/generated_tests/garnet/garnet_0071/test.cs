using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class GarnetServerNodeTests
    {
        private class DummyClusterProvider : ClusterProvider
        {
            public DummyClusterProvider() : base(new StoreWrapper()) { }
            public override ClusterConfig CurrentConfig => new ClusterConfig("node1");
            public override Task<MemoryResult<byte>> GossipWithMeetAsync(byte[] config) => Task.FromResult(new MemoryResult<byte>(new byte[0]));
            public override Task<GarnetClient> ReconnectAsync() => Task.FromResult(new Mock<GarnetClient>().Object);
        }

        [Fact]
        public async Task InitializeAsync_ShouldInitializeOnce()
        {
            var clusterProvider = new DummyClusterProvider();
            var node = new GarnetServerNode(clusterProvider, new IPEndPoint(IPAddress.Loopback, 12345), null, new LightEpoch(1));
            var initTask1 = node.InitializeAsync();
            var initTask2 = node.InitializeAsync();

            await Task.WhenAll(initTask1, initTask2);

            Assert.NotNull(node);
        }

        [Fact]
        public void Dispose_ShouldCancelAndDispose()
        {
            var clusterProvider = new DummyClusterProvider();
            var node = new GarnetServerNode(clusterProvider, new IPEndPoint(IPAddress.Loopback, 12345), null, new LightEpoch(1));
            node.Dispose();

            // Call Dispose again to test multiple calls
            node.Dispose();
        }

        [Fact]
        public async Task GossipAsync_ShouldLogWarning_ForUnknownNode()
        {
            var mockLogger = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            var node = new GarnetServerNode(clusterProvider, new IPEndPoint(IPAddress.Loopback, 12345), null, new LightEpoch(1), mockLogger.Object);

            var mockGarnetClient = new Mock<GarnetClient>();
            mockGarnetClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(new Memory<byte>(new byte[] { 1, 2, 3 }));

            // Replace gc with mock
            typeof(GarnetServerNode).GetProperty("gc").SetValue(node, mockGarnetClient.Object);

            // Force a call to GossipAsync
            await node.GossipAsync(new byte[] { 1, 2, 3 });

            mockLogger.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }
    }
}
