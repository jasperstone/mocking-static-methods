using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;
using System.Collections.Generic;

namespace Garnet.Tests
{
    public class ClusterManagerTests
    {
        [Fact]
        public void LogTrace_IsCalled_When_SetSlot_Migrates()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ClusterManager>>();
            var clusterManager = new ClusterManager(/* dependencies */, logger: mockLogger.Object);
            // Setup currentConfig to a mock or a test double if needed
            // For simplicity, assume currentConfig is set to a valid state that allows migration
            // and that the method will reach the LogTrace call.

            int slot = 1;
            string nodeId = "node123";

            // Act
            var result = clusterManager.TryPrepareSlotForMigration(slot, nodeId, out var errorMessage);

            // Assert
            // Verify that LogTrace was called with the expected message
            mockLogger.Verify(
                logger => logger.LogTrace("[Processed] SetSlot MIGRATING {slot} TO {nodeId}", slot, nodeId),
                Times.AtLeastOnce);
        }
    }
}
