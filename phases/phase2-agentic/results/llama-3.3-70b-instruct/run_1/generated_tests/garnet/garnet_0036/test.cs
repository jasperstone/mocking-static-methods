using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

namespace GarnetTests
{
    public class ClusterManagerTests
    {
        [Fact]
        public void TryPrepareSlotForMigration_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManager = new ClusterManager(new ClusterProvider(), loggerMock.Object);
            var slot = 1;
            var nodeId = "nodeId";

            // Act
            clusterManager.TryPrepareSlotForMigration(slot, nodeId, out _);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), slot, nodeId), Times.Once);
        }

        [Fact]
        public void TryPrepareSlotsForMigration_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManager = new ClusterManager(new ClusterProvider(), loggerMock.Object);
            var slots = new HashSet<int> { 1, 2, 3 };
            var nodeId = "nodeId";

            // Act
            clusterManager.TryPrepareSlotsForMigration(slots, nodeId, out _);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), slots, nodeId), Times.Once);
        }

        [Fact]
        public void TryAddSlots_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManager = new ClusterManager(new ClusterProvider(), loggerMock.Object);
            var slots = new HashSet<int> { 1, 2, 3 };

            // Act
            clusterManager.TryAddSlots(slots, out _);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), slots), Times.Once);
        }

        [Fact]
        public void TryRemoveSlots_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManager = new ClusterManager(new ClusterProvider(), loggerMock.Object);
            var slots = new HashSet<int> { 1, 2, 3 };

            // Act
            clusterManager.TryRemoveSlots(slots, out _);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), slots), Times.Once);
        }
    }
}
