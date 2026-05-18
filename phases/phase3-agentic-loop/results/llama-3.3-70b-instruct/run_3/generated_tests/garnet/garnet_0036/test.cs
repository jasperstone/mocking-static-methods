using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using Garnet.cluster;

namespace Garnet.cluster
{
    public class ClusterManagerTests
    {
        [Fact]
        public void TryPrepareSlotForMigration_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProvider = new ClusterProvider(storeWrapperMock.Object);
            var clusterManager = new ClusterManager(clusterProvider, loggerMock.Object);

            // Act
            var result = clusterManager.TryPrepareSlotForMigration(1, "nodeId", out _);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void TryPrepareSlotsForMigration_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProvider = new ClusterProvider(storeWrapperMock.Object);
            var clusterManager = new ClusterManager(clusterProvider, loggerMock.Object);

            // Act
            var result = clusterManager.TryPrepareSlotsForMigration(new HashSet<int> { 1, 2 }, "nodeId", out _);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void TryAddSlots_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProvider = new ClusterProvider(storeWrapperMock.Object);
            var clusterManager = new ClusterManager(clusterProvider, loggerMock.Object);

            // Act
            var result = clusterManager.TryAddSlots(new HashSet<int> { 1, 2 }, out _);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void TryRemoveSlots_LogTrace_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var clusterProvider = new ClusterProvider(storeWrapperMock.Object);
            var clusterManager = new ClusterManager(clusterProvider, loggerMock.Object);

            // Act
            var result = clusterManager.TryRemoveSlots(new HashSet<int> { 1, 2 }, out _);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
