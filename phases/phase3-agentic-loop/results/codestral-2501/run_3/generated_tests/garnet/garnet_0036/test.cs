using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ClusterManagerWrapper
    {
        private readonly ClusterManager _clusterManager;

        public ClusterManagerWrapper(ILogger<ClusterManager> logger)
        {
            _clusterManager = new ClusterManager(logger);
        }

        public bool TryPrepareSlotForMigration(int slot, string nodeid, out ReadOnlySpan<byte> errorMessage)
        {
            return _clusterManager.TryPrepareSlotForMigration(slot, nodeid, out errorMessage);
        }

        public bool TryPrepareSlotsForMigration(HashSet<int> slots, string nodeid, out ReadOnlySpan<byte> errorMessage)
        {
            return _clusterManager.TryPrepareSlotsForMigration(slots, nodeid, out errorMessage);
        }
    }

    public class ClusterManagerTests
    {
        [Fact]
        public void TryPrepareSlotForMigration_LogsTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ClusterManager>>();
            var clusterManagerWrapper = new ClusterManagerWrapper(mockLogger.Object);

            int slot = 1;
            string nodeid = "node1";

            // Act
            var result = clusterManagerWrapper.TryPrepareSlotForMigration(slot, nodeid, out var errorMessage);

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
            var clusterManagerWrapper = new ClusterManagerWrapper(mockLogger.Object);

            var slots = new HashSet<int> { 1, 2, 3 };
            string nodeid = "node1";

            // Act
            var result = clusterManagerWrapper.TryPrepareSlotsForMigration(slots, nodeid, out var errorMessage);

            // Assert
            mockLogger.Verify(
                x => x.LogTrace(
                    "[Processed] SetSlot {slot} FORCED TO {nodeId}",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
