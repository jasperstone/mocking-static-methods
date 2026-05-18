using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ClusterManagerSlotStateTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly ClusterManager _clusterManager;

        public ClusterManagerSlotStateTests()
        {
            _loggerMock = new Mock<ILogger>();
            // Create a dummy ClusterProvider or mock as needed
            var clusterProvider = new Mock<ClusterProvider>().Object;
            _clusterManager = new ClusterManager(clusterProvider, _loggerMock.Object);
        }

        [Fact]
        public void LogTrace_IsCalled_When_SetSlot_Migration()
        {
            // Arrange
            int slot = 1;
            string nodeId = "node123";

            // Setup currentConfig with necessary methods
            var currentConfig = new Mock<ClusterConfig>();
            var newConfig = new Mock<ClusterConfig>();

            // Setup currentConfig.GetWorkerIdFromNodeId to return a valid worker id
            currentConfig.Setup(c => c.GetWorkerIdFromNodeId(It.IsAny<string>())).Returns(1);
            // Setup currentConfig.GetNodeRoleFromNodeId to return PRIMARY
            currentConfig.Setup(c => c.GetNodeRoleFromNodeId(It.IsAny<string>())).Returns(NodeRole.PRIMARY);
            // Setup currentConfig.IsLocal to return true for the slot
            currentConfig.Setup(c => c.IsLocal(It.IsAny<ushort>())).Returns(true);
            // Setup currentConfig.GetState to return SlotState.STABLE
            currentConfig.Setup(c => c.GetState(It.IsAny<ushort>())).Returns(SlotState.STABLE);
            // Setup currentConfig.GetNodeIdFromSlot to return a dummy node id
            currentConfig.Setup(c => c.GetNodeIdFromSlot(It.IsAny<ushort>())).Returns("nodeXYZ");
            // Setup currentConfig.UpdateSlotState to return newConfig
            currentConfig.Setup(c => c.UpdateSlotState(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SlotState>())).Returns(newConfig.Object);

            // Use reflection or internal access to set currentConfig in ClusterManager
            typeof(ClusterManager).GetProperty("currentConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_clusterManager, currentConfig.Object);

            // Act
            var result = _clusterManager.TryPrepareSlotForMigration(slot, nodeId, out var errorMessage);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                x => x.LogTrace("[Processed] SetSlot MIGRATING {slot} TO {nodeId}", slot, nodeId),
                Times.Once);
        }
    }
}
