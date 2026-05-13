using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.UnitTests.Cluster.Server.Gossip
{
    public class GarnetServerNodeTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly TestClusterProvider _clusterProvider;
        private readonly GarnetServerNode _sut;

        public GarnetServerNodeTests()
        {
            _loggerMock = new Mock<ILogger>();
            _clusterProvider = new TestClusterProvider();
            _sut = _clusterProvider.CreateServerNode(_loggerMock.Object);
        }

        [Fact]
        public async Task ProcessGossipTask_WhenTaskFaulted_LogsWarning()
        {
            // Arrange
            var expectedException = new InvalidOperationException("boom");
            var faultedTask = Task.FromException(expectedException);
            _clusterProvider.SetGossipTask(faultedTask);

            // Act
            var result = await _clusterProvider.InvokeProcessGossipTaskAsync();

            // Assert
            Assert.False(result, "Faulted task should yield false");
            VerifyLog(LogLevel.Warning, expectedException, "GOSSIP round faulted");
        }

        private void VerifyLog(LogLevel level, Exception exception, string expectedMessage)
        {
            _loggerMock.Verify(logger => logger.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                        state.ToString() != null &&
                        state.ToString().Contains(expectedMessage)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #region Test Infrastructure

        private sealed class TestClusterProvider : ClusterProvider
        {
            private readonly GarnetServerNodeAccessor _accessor;

            public TestClusterProvider()
            {
                var clusterManager = new TestClusterManager();
                var replicationManager = new TestReplicationManager();
                clusterManager.clusterProvider = this;
                clusterManager.replicationManager = replicationManager;
                this.clusterManager = clusterManager;
                this.replicationManager = replicationManager;
                _accessor = new GarnetServerNodeAccessor(this);
            }

            public GarnetServerNode CreateServerNode(ILogger logger) => _accessor.CreateServerNode(logger);

            public void SetGossipTask(Task task) => _accessor.SetGossipTask(task);

            public Task<bool> InvokeProcessGossipTaskAsync() => _accessor.ProcessGossipTaskAsync();
        }

        private sealed class GarnetServerNodeAccessor
        {
            private readonly TestClusterProvider _clusterProvider;
            private GarnetServerNode _node;

            public GarnetServerNodeAccessor(TestClusterProvider clusterProvider)
            {
                _clusterProvider = clusterProvider;
            }

            public GarnetServerNode CreateServerNode(ILogger logger)
            {
                _node = new GarnetServerNode(
                    _clusterProvider,
                    new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 6379),
                    tlsOptions: null,
                    epoch: new Garnet.common.LightEpoch(),
                    logger);
                return _node;
            }

            public void SetGossipTask(Task task) => typeof(GarnetServerNode)
                .GetField("gossipTask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(_node, task);

            public async Task<bool> ProcessGossipTaskAsync()
            {
                var method = typeof(GarnetServerNode)
                    .GetMethod("ProcessGossipTask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var result = method!.Invoke(_node, Array.Empty<object>());

                if (result is ValueTask<bool> valueTaskBool)
                {
                    return await valueTaskBool.ConfigureAwait(false);
                }

                return (bool)result!;
            }
        }

        #endregion
    }
}
