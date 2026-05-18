using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.cluster.Tests
{
    public class ClusterManagerSlotStateTests
    {
        private class DummyConfig
        {
            public string LocalNodeId { get; set; } = "localnode";
            public int LocalNodeConfigEpoch { get; set; } = 1;
            public string LocalNodeIp { get; set; } = "127.0.0.1";
            public int LocalNodePort { get; set; } = 1234;
            public string LocalNodeIdShort => LocalNodeId.Substring(0, Math.Min(8, LocalNodeId.Length));
            public int NumWorkers { get; set; } = 1;

            public int GetWorkerIdFromNodeId(string nodeid)
            {
                if (nodeid == LocalNodeId) return 1;
                if (nodeid == "othernode") return 2;
                return 0;
            }

            public bool Equals(string nodeid, StringComparison comparison) => LocalNodeId.Equals(nodeid, comparison);

            public DummyConfig UpdateSlotState(int slot, int workerId, SlotState state) => this;

            public DummyConfig BumpLocalNodeConfigEpoch()
            {
                LocalNodeConfigEpoch++;
                return this;
            }

            public DummyConfig UpdateMultiSlotState(HashSet<int> slots, int workerId, SlotState state) => this;
        }

        private class TestClusterManager
        {
            public DummyConfig currentConfig;
            public ILogger logger;

            public TestClusterManager(ILogger logger)
            {
                this.logger = logger;
                this.currentConfig = new DummyConfig();
            }

            public void FlushConfig() { }

            public bool TryPrepareSlotsForMigration(HashSet<int> slots, string nodeid, out ReadOnlySpan<byte> errorMessage)
            {
                errorMessage = default;
                while (true)
                {
                    var current = currentConfig;
                    var migratingWorkerId = current.GetWorkerIdFromNodeId(nodeid);

                    if (migratingWorkerId == 0)
                    {
                        errorMessage = Encoding.ASCII.GetBytes($"ERR I don't know about node {nodeid}");
                        return false;
                    }

                    if (current.LocalNodeId.Equals(nodeid, StringComparison.OrdinalIgnoreCase))
                    {
                        errorMessage = Encoding.ASCII.GetBytes($"ERR Cannot migrate to self {nodeid}");
                        return false;
                    }

                    var newConfig = current.UpdateMultiSlotState(slots, migratingWorkerId, SlotState.STABLE);
                    if (current.LocalNodeId.Equals(nodeid, StringComparison.OrdinalIgnoreCase))
                        newConfig = newConfig.BumpLocalNodeConfigEpoch();

                    if (System.Threading.Interlocked.CompareExchange(ref currentConfig, newConfig, current) == current)
                        break;
                }
                FlushConfig();
                logger?.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", 0, nodeid);
                return true;
            }
        }

        [Fact]
        public void TryPrepareSlotsForMigration_LogsTrace()
        {
            var loggerMock = new Mock<ILogger>();
            var clusterManager = new TestClusterManager(loggerMock.Object);

            var slots = new HashSet<int> { 1, 2, 3 };
            string nodeid = "othernode";

            ReadOnlySpan<byte> errorMessage;
            bool result = clusterManager.TryPrepareSlotsForMigration(slots, nodeid, out errorMessage);

            Assert.True(result);
            Assert.True(errorMessage.IsEmpty);

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
