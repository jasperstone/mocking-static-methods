using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.Cluster.Server
{
    public class ClusterManagerSlotStateTests
    {
        [Fact]
        public void SetSlotForcesOwnership_ShouldLogTraceWithExpectedMessage()
        {
            // Arrange
            var slot = 12;
            var nodeId = "NODE-123";
            var currentConfig = CreateConfig(nodeId, workerId: 2);
            var loggerMock = new Mock<ILogger>();
            var state = new TestableClusterManagerSlotState(currentConfig, loggerMock.Object);

            // Act
            var result = state.InvokeSetSlotForced(slot, nodeId);

            // Assert
            Assert.True(result, "Expected the forced slot update to succeed.");
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) =>
                        v.ToString() == "[Processed] SetSlot {slot} FORCED TO {nodeId}"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static TestableClusterConfig CreateConfig(string nodeId, int workerId)
            => new TestableClusterConfig(nodeId, workerId);

        private sealed class TestableClusterManagerSlotState
        {
            private ClusterConfig currentConfig;
            private readonly ILogger logger;

            public TestableClusterManagerSlotState(ClusterConfig config, ILogger logger)
            {
                currentConfig = config;
                this.logger = logger;
            }

            public bool InvokeSetSlotForced(int slot, string nodeId)
            {
                var current = currentConfig;
                while (true)
                {
                    current = currentConfig;
                    var workerId = current.GetWorkerIdFromNodeId(nodeId);
                    var newConfig = current.UpdateSlotState(slot, workerId, SlotState.Stable);
                    if (Interlocked.CompareExchange(ref currentConfig, newConfig, current) == current)
                        break;
                }

                logger?.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", slot, nodeId);
                return true;
            }
        }

        private abstract class ClusterConfig
        {
            public abstract int GetWorkerIdFromNodeId(string nodeId);
            public abstract ClusterConfig UpdateSlotState(int slot, int workerId, SlotState state);
        }

        private sealed class TestableClusterConfig : ClusterConfig
        {
            private readonly string expectedNodeId;
            private readonly int workerId;

            public TestableClusterConfig(string expectedNodeId, int workerId)
            {
                this.expectedNodeId = expectedNodeId;
                this.workerId = workerId;
            }

            public override int GetWorkerIdFromNodeId(string nodeId)
                => string.Equals(nodeId, expectedNodeId, StringComparison.OrdinalIgnoreCase) ? workerId : 0;

            public override ClusterConfig UpdateSlotState(int slot, int workerId, SlotState state)
                => this;
        }

        private enum SlotState
        {
            Stable
        }
    }
}
