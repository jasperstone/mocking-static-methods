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
                // Setup a simple initial config with one worker and local node id "local"
                currentConfig = new TestConfig("local");
            }

            public new ILogger logger;
            public new TestConfig currentConfig;

            public override void FlushConfig()
            {
                // Do nothing for test
            }
        }

        private class TestConfig
        {
            public string LocalNodeId { get; }
            public string LocalNodeIdShort => LocalNodeId.Substring(0, Math.Min(5, LocalNodeId.Length));
            public int LocalNodeConfigEpoch { get; private set; } = 1;
            public string LocalNodeIp => "127.0.0.1";
            public int LocalNodePort => 1234;
            public int NumWorkers => 1;

            private readonly string localNodeId;

            public TestConfig(string localNodeId)
            {
                this.localNodeId = localNodeId;
                LocalNodeId = localNodeId;
            }

            public int GetWorkerIdFromNodeId(string nodeid)
            {
                if (nodeid == LocalNodeId) return 1;
                if (nodeid == "other") return 2;
                return 0;
            }

            public TestConfig UpdateSlotState(int slot, int workerId, SlotState state)
            {
                return this;
            }

            public TestConfig BumpLocalNodeConfigEpoch()
            {
                LocalNodeConfigEpoch++;
                return this;
            }

            public bool LocalNodeIdEquals(string nodeid)
            {
                return string.Equals(LocalNodeId, nodeid, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void TryPrepareSlotsForMigration_LogsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManager = new TestClusterManager(loggerMock.Object);

            var slots = new HashSet<int> { 1, 2, 3 };
            string nodeid = "other";

            // We need to override currentConfig to simulate the behavior expected in the method
            clusterManager.currentConfig = new TestConfig("local");

            // Act
            ReadOnlySpan<byte> errorMessage;
            bool result = clusterManager.TryPrepareSlotsForMigration(slots, nodeid, out errorMessage);

            // Assert
            Assert.True(result);
            Assert.True(errorMessage.IsEmpty);

            // Verify that LogTrace was called with the expected message and parameters
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("[Processed] SetSlot FORCED TO")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
