using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text;
using Garnet.cluster;

public class ClusterManagerSlotStateTests
{
    [Fact]
    public void TryPrepareSlotForMigration_LogsTrace()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ClusterManager>>();
        var clusterManager = new ClusterManager(new ClusterProvider(), mockLogger.Object);

        var slot = 1;
        var nodeId = "node1";
        var errorMessage = default(ReadOnlySpan<byte>);

        // Act
        var result = clusterManager.TryPrepareSlotForMigration(slot, nodeId, out errorMessage);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("[Processed] SetSlot MIGRATING")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void TryPrepareSlotsForMigration_LogsTrace()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ClusterManager>>();
        var clusterManager = new ClusterManager(new ClusterProvider(), mockLogger.Object);

        var slots = new HashSet<int> { 1, 2, 3 };
        var nodeId = "node1";
        var errorMessage = default(ReadOnlySpan<byte>);

        // Act
        var result = clusterManager.TryPrepareSlotsForMigration(slots, nodeId, out errorMessage);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("[Processed] SetSlot")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void TryPrepareSlotsForOwnershipChange_LogsDebug()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ClusterManager>>();
        var clusterManager = new ClusterManager(new ClusterProvider(), mockLogger.Object);

        var slots = new HashSet<int> { 1, 2, 3 };
        var nodeId = "node1";
        var errorMessage = default(ReadOnlySpan<byte>);

        // Act
        var result = clusterManager.TryPrepareSlotsForOwnershipChange(slots, nodeId, out errorMessage);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Bumped Epoch")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
