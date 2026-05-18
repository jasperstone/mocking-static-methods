using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ClusterManagerTests
    {
        private class DummyConfig
        {
            public int NumWorkers => 1;
            public string LocalNodeId => "node1";
            public int LocalNodeConfigEpoch => 1;
            public string LocalNodeIp => "127.0.0.1";
            public int LocalNodePort => 6379;
            public string LocalNodeIdShort => "node1";

            public bool TryAddSlots(HashSet<int> slots, out int slot, out DummyConfig newConfig)
            {
                slot = -1;
                newConfig = this;
                return false;
            }

            public bool TryRemoveSlots(HashSet<int> slots, out int slot, out DummyConfig newConfig)
            {
                slot = -1;
                newConfig = this;
                return false;
            }

            public int GetWorkerIdFromNodeId(string nodeId) => 1;
            public string GetNodeIdFromSlot(ushort slot) => "node1";
            public NodeRole GetNodeRoleFromNodeId(string nodeId) => NodeRole.PRIMARY;
            public bool IsLocal(ushort slot) => true;
            public SlotState GetState(ushort slot) => SlotState.STABLE;
            public DummyConfig UpdateSlotState(int slot, int workerId, SlotState state) => this;
            public DummyConfig BumpLocalNodeConfigEpoch() => this;
        }

        private enum NodeRole { PRIMARY, SECONDARY }
        private enum SlotState { STABLE, MIGRATING, OFFLINE }

        [Fact]
        public void LogTrace_IsCalled_When_SetSlotForced()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ClusterManager>>();
            var clusterManager = new ClusterManager(loggerMock.Object)
            {
                currentConfig = new DummyConfig()
            };
            int slot = 1;
            string nodeId = "node1";

            // Act
            var result = clusterManager.TrySetSlot(slot, nodeId, out _);

            // Assert
            Assert.True(result);
            loggerMock.Verify(x => x.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", slot, nodeId), Times.Once);
        }
    }
}
