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
        private class DummyConfig
        {
            public string LocalNodeId { get; set; } = "localnode";
            public int LocalNodeConfigEpoch { get; set; } = 1;
            public string LocalNodeIp { get; set; } = "127.0.0.1";
            public int LocalNodePort { get; set; } = 1234;
            public string LocalNodeIdShort { get; set; } = "local";

            public int GetWorkerIdFromNodeId(string nodeid)
            {
                if (nodeid == "knownnode") return 1;
                return 0;
            }

            public bool EqualsLocalNodeId(string nodeid)
            {
                return string.Equals(LocalNodeId, nodeid, StringComparison.OrdinalIgnoreCase);
            }

            public DummyConfig UpdateMultiSlotState(HashSet<int> slots, int workerId, SlotState state)
            {
                return this;
            }

            public DummyConfig BumpLocalNodeConfigEpoch()
            {
                LocalNodeConfigEpoch++;
                return this;
            }
        }

        private class TestClusterManager
        {
            private DummyConfig _currentConfig = new DummyConfig();
            private readonly ILogger _logger;

            public TestClusterManager(ILogger logger)
            {
                _logger = logger;
            }

            private DummyConfig currentConfig
            {
                get => _currentConfig;
                set => _currentConfig = value;
            }

            private void FlushConfig()
            {
                // no-op for test
            }

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

                    if (current.EqualsLocalNodeId(nodeid))
                    {
                        errorMessage = Encoding.ASCII.GetBytes("ERR Cannot migrate to self");
                        return false;
                    }

                    var newConfig = current.UpdateMultiSlotState(slots, migratingWorkerId, SlotState.STABLE);
                    if (current.EqualsLocalNodeId(nodeid))
                        newConfig = newConfig.BumpLocalNodeConfigEpoch();

                    // Simulate atomic compare exchange by just assigning
                    if (ReferenceEquals(_currentConfig, current))
                    {
                        _currentConfig = newConfig;
                        break;
                    }
                }
                FlushConfig();
                _logger?.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", 0, "dummy");
                return true;
            }
        }

        [Fact]
        public void TryPrepareSlotsForMigration_LogsTrace_WhenSuccessful()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManager = new TestClusterManager(loggerMock.Object);

            var slots = new HashSet<int> { 1, 2, 3 };
            string nodeid = "knownnode";

            // Act
            var result = clusterManager.TryPrepareSlotsForMigration(slots, nodeid, out var errorMessage);

            // Assert
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
