using Moq;
using Moq.Language.Flow;
using Xunit;
using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;

namespace Garnet.Cluster.Tests;

public class ReplicationManagerLoggerTests
{
    [Fact]
    public void BeginRecovery_LogsError_WhenCurrentRecoveryStatusIsNotNoRecovery()
    {
        // Arrange - test the specific LogError call on line 368
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

        // Create real dependencies with minimal mocks
        var mockClusterProvider = new Mock<Garnet.cluster.ClusterProvider>();
        var mockStoreWrapper = new Mock<Garnet.cluster.StoreWrapper>();
        mockClusterProvider.Setup(x => x.storeWrapper).Returns(mockStoreWrapper.Object);

        var replicationManager = new Garnet.cluster.ReplicationManager(mockClusterProvider.Object, mockLogger.Object);
        
        // Use reflection to set the private field to trigger the exact LogError on line 368
        var currentStatusField = typeof(Garnet.cluster.ReplicationManager)
            .GetField("currentRecoveryStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        currentStatusField?.SetValue(replicationManager, Garnet.cluster.RecoveryStatus.InitializeRecover);

        // Act
        var result = (bool)typeof(Garnet.cluster.ReplicationManager)
            .GetMethod("BeginRecovery", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(replicationManager, [Garnet.cluster.RecoveryStatus.CheckpointRecoveredAtReplica, false]);

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify the exact message using the logger's Log call with message inspection
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => 
                    ((string)state.ToString()).Contains("Error background recovering task has not completed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void BeginRecovery_LogsError_WhenTryPauseCheckpointsFails()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

        var mockStoreWrapper = new Mock<Garnet.cluster.StoreWrapper>();
        mockStoreWrapper.Setup(x => x.TryPauseCheckpoints()).Returns(false);

        var mockClusterProvider = new Mock<Garnet.cluster.ClusterProvider>();
        mockClusterProvider.Setup(x => x.storeWrapper).Returns(mockStoreWrapper.Object);

        var replicationManager = new Garnet.cluster.ReplicationManager(mockClusterProvider.Object, mockLogger.Object);
        
        // Set to NoRecovery so it passes the first check
        var currentStatusField = typeof(Garnet.cluster.ReplicationManager)
            .GetField("currentRecoveryStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        currentStatusField?.SetValue(replicationManager, Garnet.cluster.RecoveryStatus.NoRecovery);

        // Act
        var result = (bool)typeof(Garnet.cluster.ReplicationManager)
            .GetMethod("BeginRecovery", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(replicationManager, [Garnet.cluster.RecoveryStatus.CheckpointRecoveredAtReplica, false]);

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => 
                    ((string)state.ToString()).Contains("Error could not acquire checkpoint lock")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
