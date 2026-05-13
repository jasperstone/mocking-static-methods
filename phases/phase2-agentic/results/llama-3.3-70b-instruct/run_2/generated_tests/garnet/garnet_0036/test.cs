using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster
{
    public class ClusterManagerTests
    {
        [Fact]
        public void TryPrepareSlotForMigration_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManager = new ClusterManager(new ClusterProvider(), loggerMock.Object);

            // Act
            clusterManager.TryPrepareSlotForMigration(1, "nodeId", out _);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void TryPrepareSlotsForMigration_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManager = new ClusterManager(new ClusterProvider(), loggerMock.Object);

            // Act
            clusterManager.TryPrepareSlotsForMigration(new HashSet<int> { 1, 2 }, "nodeId", out _);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void TryAddSlots_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManager = new ClusterManager(new ClusterProvider(), loggerMock.Object);

            // Act
            clusterManager.TryAddSlots(new HashSet<int> { 1, 2 }, out _);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void TryRemoveSlots_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterManager = new ClusterManager(new ClusterProvider(), loggerMock.Object);

            // Act
            clusterManager.TryRemoveSlots(new HashSet<int> { 1, 2 }, out _);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
