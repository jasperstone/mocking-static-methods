using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Garnet.cluster;
using Garnet.common;
using Garnet.server;
using Tsavorite.core;

namespace Garnet.Tests
{
    public class ClusterManagerSlotStateTests
    {
        [Fact]
        public void TryPrepareSlotForMigration_LogsTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ClusterManager>>();
            var clusterManager = new ClusterManager(mockLogger.Object);

            var slot = 1;
            var nodeid = "node1";
            var errorMessage = default(ReadOnlySpan<byte>);

            // Act
            var result = clusterManager.TryPrepareSlotForMigration(slot, nodeid, out errorMessage);

            // Assert
            mockLogger.Verify(
                x => x.LogTrace(
                    "[Processed] SetSlot MIGRATING {slot} TO {nodeId}",
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void TryPrepareSlotsForMigration_LogsTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ClusterManager>>();
            var clusterManager = new ClusterManager(mockLogger.Object);

            var slots = new HashSet<int> { 1, 2, 3 };
            var nodeid = "node1";
            var errorMessage = default(ReadOnlySpan<byte>);

            // Act
            var result = clusterManager.TryPrepareSlotsForMigration(slots, nodeid, out errorMessage);

            // Assert
            mockLogger.Verify(
                x => x.LogTrace(
                    "[Processed] SetSlot {slot} FORCED TO {nodeId}",
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void TryPrepareSlotsForOwnershipChange_LogsDebug()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ClusterManager>>();
            var clusterManager = new ClusterManager(mockLogger.Object);

            var slots = new HashSet<int> { 1, 2, 3 };
            var nodeid = "node1";
            var errorMessage = default(ReadOnlySpan<byte>);

            // Act
            var result = clusterManager.TryPrepareSlotsForOwnershipChange(slots, nodeid, out errorMessage);

            // Assert
            mockLogger.Verify(
                x => x.LogDebug(
                    "Bumped Epoch ({LocalNodeConfigEpoch}) [{LocalIp}:{LocalPort},{LocalNodeId}]",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
