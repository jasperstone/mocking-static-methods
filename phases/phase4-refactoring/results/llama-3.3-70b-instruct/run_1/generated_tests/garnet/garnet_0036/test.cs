using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using Garnet.common;
using Garnet.server;
using Microsoft.Extensions.Logging;

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
            clusterManager.TryPrepareSlotForMigration(1, "nodeid", out _);

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
            clusterManager.TryPrepareSlotsForMigration(new HashSet<int> { 1 }, "nodeid", out _);

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
            clusterManager.TryAddSlots(new HashSet<int> { 1 }, out _);

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
            clusterManager.TryRemoveSlots(new HashSet<int> { 1 }, out _);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
