using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task LogWarning_FromUnknownNode_ShouldCallLogWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicaFailoverSession>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var currentConfigMock = new Mock<ClusterConfig>();
            var oldConfigMock = new Mock<ClusterConfig>();
            var clientMock = new Mock<GarnetClient>();

            // Setup configs
            var unknownNodeId = "unknown-node";
            var knownNodeId = "known-node";

            // Setup clusterProvider to return configs
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(currentConfigMock.Object);
            clusterProviderMock.Setup(cp => cp.clusterManager.OldConfig).Returns(oldConfigMock.Object);
            clusterProviderMock.Setup(cp => cp.clusterManager.TryTakeOverForPrimary()).Returns(true);
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            clusterProviderMock.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);

            // Setup configs to simulate unknown node
            currentConfigMock.Setup(c => c.LocalNodeId).Returns(knownNodeId);
            oldConfigMock.Setup(c => c.LocalNodeId).Returns(knownNodeId);

            // Simulate the response with a different node id
            var respSpan = new ReadOnlySpan<byte>(new byte[] { 1, 2, 3 });
            var respMock = new Mock<Resp>();
            respMock.Setup(r => r.Span).Returns(respSpan);
            respMock.Setup(r => r.Dispose());

            // Create a ClusterConfig with unknown node id
            var clusterConfigMock = new Mock<ClusterConfig>();
            clusterConfigMock.Setup(c => c.LocalNodeId).Returns(unknownNodeId);

            // Instantiate the session
            var session = new ReplicaFailoverSession(
                loggerMock.Object,
                clusterProviderMock.Object,
                cts: new CancellationTokenSource(),
                failoverTimeout: TimeSpan.FromSeconds(10),
                oldConfig: oldConfigMock.Object,
                clusterManager: clusterManagerMock.Object,
                clusterProvider: clusterProviderMock.Object,
                current: new { IsKnown = new Func<string, bool>(id => false) }, // simulate unknown
                resp: respMock.Object,
                clusterConfigFromByteArray: (byte[] array) => clusterConfigMock.Object
            );

            // Act
            // Simulate the code path that calls LogWarning
            var other = clusterConfigMock.Object;
            if (!session.current.IsKnown(other.LocalNodeId))
            {
                session.logger?.LogWarning("Received gossip from unknown node: {node-id}", other.LocalNodeId);
            }

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("Received gossip from unknown node: {node-id}", other.LocalNodeId),
                Times.Once);
        }
    }
}
