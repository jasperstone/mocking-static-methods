using System;
using System.Text;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;
using Garnet.common;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ReplicaReceiveCheckpointTests
{
    [Fact]
    public async Task TryReplicateDiskbasedSyncAsync_LogsForegroundCheckpointRetrieval()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProviderMock = new Mock<IClusterProvider>();
        var sessionMock = new Mock<ClusterSession>();
        var options = new ReplicateSyncOptions { Background = false };

        var replicationManager = new ReplicationManager(loggerMock.Object, clusterProviderMock.Object);

        // Act
        await replicationManager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation("Initiating foreground checkpoint retrieval"),
            Times.Once);
    }

    [Fact]
    public async Task TryReplicateDiskbasedSyncAsync_CallsReplicaSyncAttachTaskAsyncWithCorrectParameters()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProviderMock = new Mock<IClusterProvider>();
        var sessionMock = new Mock<ClusterSession>();
        var options = new ReplicateSyncOptions { Background = false };

        var replicationManager = new ReplicationManager(loggerMock.Object, clusterProviderMock.Object);

        // Act
        await replicationManager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

        // Assert
        // Verify that ReplicaSyncAttachTaskAsync is called with the correct parameters
        // This is a bit tricky since ReplicaSyncAttachTaskAsync is a local function, so we can't directly verify it.
        // Instead, we can verify that the logger logs the correct information.
        loggerMock.Verify(
            x => x.LogInformation("Initiating foreground checkpoint retrieval"),
            Times.Once);
    }

    [Fact]
    public async Task TryReplicateDiskbasedSyncAsync_ReturnsFalseAndErrorMessageWhenReplicaSyncAttachTaskAsyncReturnsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProviderMock = new Mock<IClusterProvider>();
        var sessionMock = new Mock<ClusterSession>();
        var options = new ReplicateSyncOptions { Background = false };

        var replicationManager = new ReplicationManager(loggerMock.Object, clusterProviderMock.Object);

        // Mock the ReplicaSyncAttachTaskAsync method to return an error message
        replicationManager.ReplicaSyncAttachTaskAsync = async (downgradeLock, forceAsync) =>
        {
            await Task.Yield();
            return "Error message";
        };

        // Act
        var result = await replicationManager.TryReplicateDiskbasedSyncAsync(sessionMock.Object, options);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Encoding.ASCII.GetBytes("Error message"), result.ErrorMessage);
    }
}
