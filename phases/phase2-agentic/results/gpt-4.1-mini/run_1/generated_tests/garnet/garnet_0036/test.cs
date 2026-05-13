using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class ClusterManagerTests
    {
        private class TestClusterManager : ClusterManager
        {
            public TestClusterManager(ILogger logger)
            {
                this.logger = logger;
                // Setup a simple currentConfig mock with minimal implementation for testing
                currentConfig = new TestConfig();
            }

            public new ILogger logger;

            public new TestConfig currentConfig;

            public bool FlushConfigCalled { get; private set; }

            protected override void FlushConfig()
            {
                FlushConfigCalled = true;
            }

            private class TestConfig : IClusterConfig
            {
                public int LocalNodeConfigEpoch { get; set; } = 1;
                public string LocalNodeIp { get; set; } = "127.0.0.1";
                public int LocalNodePort { get; set; } = 1234;
                public string LocalNodeId { get; set; } = "localnode";
                public string LocalNodeIdShort => "local";

                public int NumWorkers => 1;

                public bool TryAddSlots(HashSet<int> slots, out int slot, out IClusterConfig newConfig)
                {
                    slot = -1;
                    newConfig = this;
                    return true;
                }

                public bool TryRemoveSlots(HashSet<int> slots, out int slot, out IClusterConfig newConfig)
                {
                    slot = -1;
                    newConfig = this;
                    return true;
                }

                public int GetWorkerIdFromNodeId(string nodeid)
                {
                    if (nodeid == "unknown") return 0;
                    if (nodeid == "localnode") return 1;
                    return 2;
                }

                public string GetNodeIdFromSlot(ushort slot) => "nodeid";

                public NodeRole GetNodeRoleFromNodeId(string nodeid)
                {
                    if (nodeid == "primary") return NodeRole.PRIMARY;
                    return NodeRole.REPLICA;
                }

                public bool IsLocal(ushort slot) => true;

                public SlotState GetState(ushort slot) => SlotState.STABLE;

                public IClusterConfig UpdateSlotState(int slot, int workerId, SlotState state) => this;

                public IClusterConfig UpdateMultiSlotState(HashSet<int> slots, int workerId, SlotState state) => this;

                public IClusterConfig BumpLocalNodeConfigEpoch() => this;
            }
        }

        [Fact]
        public void TryPrepareSlotsForMigration_LogsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManager = new TestClusterManager(loggerMock.Object);

            var slots = new HashSet<int> { 1, 2, 3 };
            string nodeid = "nodeid";
            ReadOnlySpan<byte> errorMessage;

            // Act
            var result = clusterManager.TryPrepareSlotsForMigration(slots, nodeid, out errorMessage);

            // Assert
            Assert.True(result);
            Assert.True(clusterManager.FlushConfigCalled);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("[Processed] SetSlot")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
