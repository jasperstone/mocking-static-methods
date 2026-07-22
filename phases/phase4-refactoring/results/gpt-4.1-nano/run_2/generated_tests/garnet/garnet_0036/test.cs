using Xunit;
using Moq;
using System;
using System.Text;
using System.Collections.Generic;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ClusterManagerTests
    {
        [Fact]
        public void TryPrepareSlotForMigration_ShouldLogTrace_WhenSuccessful()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ClusterManager>>();
            var clusterManager = new ClusterManager();

            // Setup currentConfig with necessary methods and properties
            var currentConfigMock = new Mock<IClusterConfig>();
            // Setup methods like GetWorkerIdFromNodeId, IsLocal, GetState, etc.
            // For simplicity, assume currentConfig is set up correctly
            // and assign it to clusterManager (via reflection or property setter)
            // For this example, we will assume such setup is done.

            string nodeId = "node123";
            int slot = 5;
            ReadOnlySpan<byte> errorMessage;

            // Act
            var result = clusterManager.TryPrepareSlotForMigration(slot, nodeId, out errorMessage);

            // Assert
            Assert.True(result);
            mockLogger.Verify(logger => logger.LogTrace("[Processed] SetSlot MIGRATING {slot} TO {nodeId}", slot, nodeId), Times.Once);
        }
    }
}
