using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using garnet.cluster;

namespace Garnet.Tests
{
    public class GarnetServerNodeTests
    {
        private class DummyClusterProvider : ClusterProvider
        {
            public override ClusterManager clusterManager { get; }
            public override GarnetClient Client => throw new NotImplementedException();

            public DummyClusterProvider()
            {
                var cm = new Mock<ClusterManager>();
                cm.SetupGet(c => c.CurrentConfig).Returns(new ClusterConfig());
                cm.SetupGet(c => c.gossipStats).Returns(new GossipStats());
                cm.SetupGet(c => c.clusterTimeout).Returns(1000);
                cm.SetupGet(c => c.ctsGossip).Returns(new CancellationTokenSource());
                clusterManager = cm.Object;
            }
        }

        [Fact]
        public async Task LogWarning_IsCalled_When_TaskFaults()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var clusterProvider = new DummyClusterProvider();
            var node = new GarnetServerNode(clusterProvider, new System.Net.IPEndPoint(0, 0), null, null, mockLogger.Object);

            // Use reflection to set the private gossipTask field to a faulted task
            var gossipTaskField = typeof(GarnetServerNode).GetField("gossipTask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var faultedTask = Task.FromException(new InvalidOperationException("fail"));
            gossipTaskField.SetValue(node, faultedTask);

            // Act
            var method = typeof(GarnetServerNode).GetMethod("GossipAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method.Invoke(node, new object[] { new byte[] { 1, 2, 3 } });

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), "GOSSIP round faulted"),
                Times.AtLeastOnce);
        }
    }
}
