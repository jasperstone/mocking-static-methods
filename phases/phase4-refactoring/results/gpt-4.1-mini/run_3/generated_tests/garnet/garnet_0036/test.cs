using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Reflection;

namespace Garnet.cluster.Tests
{
    public class ClusterManagerSlotStateTests
    {
        [Fact]
        public void TryPrepareSlotsForMigration_LogsTrace_WhenCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManagerType = Type.GetType("Garnet.cluster.ClusterManager, garnet");
            Assert.NotNull(clusterManagerType);

            // Create instance of ClusterManager via reflection
            var instance = Activator.CreateInstance(clusterManagerType, new object[] { null, loggerMock.Object });
            Assert.NotNull(instance);

            // Prepare slots and nodeId
            var slots = new HashSet<int> { 1, 2, 3 };
            string nodeId = "node1";

            // Setup currentConfig field to a fake config that returns valid values
            var currentConfigField = clusterManagerType.GetField("currentConfig", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(currentConfigField);

            var fakeConfig = new FakeClusterConfig(nodeId);
            currentConfigField.SetValue(instance, fakeConfig);

            // Get method TryPrepareSlotsForMigration
            var method = clusterManagerType.GetMethod("TryPrepareSlotsForMigration", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(method);

            // Prepare out parameter for errorMessage
            object errorMessage = default(ReadOnlySpan<byte>);
            var parameters = new object[] { slots, nodeId, errorMessage };

            // Act
            var result = (bool)method.Invoke(instance, parameters);
            errorMessage = parameters[2];

            // Assert
            Assert.True(result);
            Assert.True(((ReadOnlySpan<byte>)errorMessage).IsEmpty);

            // Verify logger.LogTrace was called with expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("[Processed] SetSlot")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class FakeClusterConfig
        {
            private readonly string localNodeId;

            public FakeClusterConfig(string localNodeId)
            {
                this.localNodeId = localNodeId;
            }

            public string LocalNodeId => localNodeId;

            public int GetWorkerIdFromNodeId(string nodeId)
            {
                return nodeId == localNodeId ? 1 : 2;
            }

            public string LocalNodeIp => "127.0.0.1";
            public int LocalNodePort => 1234;
            public string LocalNodeIdShort => localNodeId.Substring(0, Math.Min(4, localNodeId.Length));
            public int LocalNodeConfigEpoch => 1;

            public int NumWorkers => 1;

            public int GetNodeRoleFromNodeId(string nodeId)
            {
                // Return primary role for test
                return 1; // Assuming 1 means PRIMARY
            }

            public bool IsLocal(ushort slot)
            {
                return true;
            }

            public SlotState GetState(ushort slot)
            {
                return SlotState.STABLE;
            }

            public string GetNodeIdFromSlot(ushort slot)
            {
                return localNodeId;
            }

            public FakeClusterConfig UpdateSlotState(int slot, int workerId, SlotState state)
            {
                return this;
            }

            public FakeClusterConfig BumpLocalNodeConfigEpoch()
            {
                return this;
            }
        }

        private enum SlotState
        {
            STABLE,
            MIGRATING,
            OFFLINE
        }
    }
}
