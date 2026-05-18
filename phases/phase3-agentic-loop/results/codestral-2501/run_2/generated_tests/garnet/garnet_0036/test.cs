using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using Garnet.common;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ClusterManagerTests
    {
        [Fact]
        public void TryPrepareSlotForMigration_LogsTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ClusterManager>>();
            var clusterManager = (ClusterManager)Activator.CreateInstance(typeof(ClusterManager), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { mockLogger.Object }, null);

            int slot = 1;
            string nodeid = "node1";
            ReadOnlySpan<byte> errorMessage;

            // Act
            var method = typeof(ClusterManager).GetMethod("TryPrepareSlotForMigration", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (bool)method.Invoke(clusterManager, new object[] { slot, nodeid, errorMessage });

            // Assert
            mockLogger.Verify(
                logger => logger.LogTrace(
                    "[Processed] SetSlot MIGRATING {slot} TO {nodeId}",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
