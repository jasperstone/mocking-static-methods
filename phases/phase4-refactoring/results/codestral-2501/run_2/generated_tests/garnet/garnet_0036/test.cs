using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Garnet.common;
using Garnet.server;
using Tsavorite.core;

namespace Garnet.cluster.Tests
{
    public class ClusterManagerTests
    {
        [Fact]
        public void TryPrepareSlotForMigration_LogsTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ClusterManager>>();
            var clusterManager = new ClusterManager(mockLogger.Object);

            int slot = 1;
            string nodeid = "node123";
            ReadOnlySpan<byte> errorMessage;

            // Act
            bool result = clusterManager.TryPrepareSlotForMigration(slot, nodeid, out errorMessage);

            // Assert
            mockLogger.Verify(
                logger => logger.LogTrace(
                    "[Processed] SetSlot MIGRATING {slot} TO {nodeId}",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
