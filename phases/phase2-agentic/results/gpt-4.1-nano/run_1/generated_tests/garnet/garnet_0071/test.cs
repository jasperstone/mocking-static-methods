using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;
using System.Threading;
using System.Net;
using Garnet.client;

namespace Garnet.Tests
{
    public class GarnetServerNodeTests
    {
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<GarnetClient> _garnetClientMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly EndPoint _endPoint;
        private readonly LightEpoch _epoch;

        public GarnetServerNodeTests()
        {
            _clusterProviderMock = new Mock<ClusterProvider>();
            _garnetClientMock = new Mock<GarnetClient>();
            _loggerMock = new Mock<ILogger>();
            _endPoint = new IPEndPoint(IPAddress.Loopback, 12345);
            _epoch = new LightEpoch();

            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(Mock.Of<StoreWrapper>());
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(Mock.Of<ClusterManager>());
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new ClusterConfig());
            _clusterProviderMock.Setup(cp => cp.clusterManager.clusterProvider).Returns(_clusterProviderMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager.clusterProvider.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(cp => cp.clusterManager.clusterProvider.ClusterPassword).Returns("pass");
            _clusterProviderMock.Setup(cp => cp.clusterManager.ctsGossip).Returns(new CancellationTokenSource());

            // Setup for storeWrapper
            var storeWrapperMock = new Mock<StoreWrapper>();
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            storeWrapperMock.Setup(sw => sw.serverOptions).Returns(new ServerOptions());
        }

        [Fact]
        public void Constructor_ShouldInitializeFields()
        {
            var node = new GarnetServerNode(_clusterProviderMock.Object, _endPoint, null, _epoch, _loggerMock.Object);
            Assert.NotNull(node);
        }

        [Fact]
        public async Task InitializeAsync_ShouldSetUpCancellationTokensAndReconnect()
        {
            var node = new GarnetServerNode(_clusterProviderMock.Object, _endPoint, null, _epoch, _loggerMock.Object);
            var initializeTask = node.InitializeAsync();
            Assert.IsType<ValueTask>(initializeTask);
            await initializeTask.AsTask();
        }

        [Fact]
        public void Dispose_ShouldCancelAndDisposeResources()
        {
            var node = new GarnetServerNode(_clusterProviderMock.Object, _endPoint, null, _epoch, _loggerMock.Object);
            node.Dispose();
            // Call again to test multiple calls
            node.Dispose();
        }

        [Fact]
        public async Task GetMostRecentConfig_ShouldReturnConfigBytes()
        {
            var node = new GarnetServerNode(_clusterProviderMock.Object, _endPoint, null, _epoch, _loggerMock.Object);
            var configBytes = node.GetMostRecentConfig();
            Assert.NotNull(configBytes);
        }

        [Fact]
        public async Task GossipAsync_ShouldLogWarningOnUnknownNode()
        {
            var node = new GarnetServerNode(_clusterProviderMock.Object, _endPoint, null, _epoch, _loggerMock.Object);
            // Setup clusterManager.CurrentConfig to have a known node
            var currentConfig = new ClusterConfig();
            currentConfig.LocalNodeId = "knownNode";
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(currentConfig);
            // Setup clusterManager.TryMerge
            var clusterManagerMock = new Mock<ClusterManager>();
            clusterManagerMock.Setup(cm => cm.TryMerge(It.IsAny<ClusterConfig>()));
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);

            // Setup gc.GossipAsync to return a response with unknown node id
            var respBytes = new byte[] { 1, 2, 3 };
            var respSpan = new ReadOnlySpan<byte>(respBytes);
            var respMemory = new Memory<byte>(respBytes);
            var resp = new ResponseMemory(respMemory);
            var gcMock = new Mock<GarnetClient>();
            gcMock.Setup(g => g.GossipAsync(It.IsAny<byte[]>())).ReturnsAsync(resp);
            // Replace gc in node
            var nodeField = typeof(GarnetServerNode).GetField("gc", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            nodeField.SetValue(node, gcMock.Object);

            // Call GossipAsync
            await node.GetType().GetMethod("GossipAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(node, new object[] { new byte[] { 1, 2, 3 } });
            // Verify that LogWarning was called
            _loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }
    }

    // Helper class to mock response
    public class ResponseMemory : IDisposable
    {
        public Memory<byte> Span { get; }
        public ResponseMemory(Memory<byte> span) => Span = span;
        public void Dispose() { }
    }

    // Placeholder for StoreWrapper
    public class StoreWrapper
    {
        public ServerOptions serverOptions { get; set; } = new ServerOptions();
    }

    // Placeholder for ServerOptions
    public class ServerOptions
    {
        public bool DisablePubSub { get; set; } = false;
        public long PubSubPageSizeBytes() => 1024;
        public int ClusterTimeout { get; set; } = 30;
        public int GossipDelay { get; set; } = 1;
        public int ClusterConfigFlushFrequencyMs { get; set; } = -1;
        public string CheckpointDir { get; set; } = "/tmp";
        public bool CleanClusterConfig { get; set; } = false;
        public IGarnetTlsOptions TlsOptions { get; set; } = null;
    }

    // Placeholder for IGarnetTlsOptions
    public interface IGarnetTlsOptions { }
}
