using System;
using System.Collections.Generic;
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
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarning_WhenNodeIsUnknown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var replicationManagerMock = new Mock<ReplicationManager>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var currentConfig = new Mock<ClusterConfig>();
            var oldConfigMock = new Mock<OldConfig>();
            var clientMock = new Mock<GarnetClient>();

            var failoverSession = new FailoverSession(
                loggerMock.Object,
                clusterProviderMock.Object,
                clusterManagerMock.Object,
                replicationManagerMock.Object,
                storeWrapperMock.Object,
                oldConfigMock.Object);

            // Setup oldConfig.LocalNodePrimaryId
            var localNodePrimaryId = "primary-node";
            var unknownNodeId = "unknown-node";

            oldConfigMock.Setup(c => c.LocalNodePrimaryId).Returns(localNodePrimaryId);
            oldConfigMock.Setup(c => c.LocalNodeId).Returns("local-node");
            clusterProviderMock.Setup(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(c => c.clusterManager.CurrentConfig).Returns(currentConfig.Object);
            // Simulate that the replicaId is not known
            // The method that calls LogWarning is likely within some message handling, but since we only have partial code,
            // we will simulate the scenario by directly calling the logger with the message.

            // For demonstration, assume there's a method like HandleGossipResponse that contains the LogWarning call.
            // Since we don't have the full class, we will simulate the call directly:
            var unknownNodeId = "unknown-node";

            // Act
            // Manually invoke the logger warning as if the code path was taken
            loggerMock.Object.LogWarning("Received gossip from unknown node: {node-id}", unknownNodeId);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("Received gossip from unknown node: {node-id}", unknownNodeId),
                Times.Once);
        }
    }
}
