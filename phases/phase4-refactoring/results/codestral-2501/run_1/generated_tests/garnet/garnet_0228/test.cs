using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.cluster.Server.Replication;
using Garnet.common;
using Garnet.server;

public class ReplicaReceiveCheckpointTests
{
    [Fact]
    public async Task TryReplicateDiskbasedSyncAsync_LogsError_WhenPrimaryAddressIsNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProviderMock = new Mock<IClusterProvider>();
        var clusterManagerMock = new Mock<IClusterManager>();
        var replicationManagerMock = new Mock<IReplicationManager>();
        var serverOptionsMock = new Mock<IServerOptions>();
        var storeWrapperMock = new Mock<IStoreWrapper>();
        var appendOnlyFileMock = new Mock<IAppendOnlyFile>();

        clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
        clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
        clusterProviderMock.Setup(cp => cp.serverOptions).Returns(serverOptionsMock.Object);
        clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);

        clusterManagerMock.Setup(cm => cm.CurrentConfig.GetLocalNodePrimaryAddress()).Returns((null, -1));
        storeWrapperMock.Setup(sw => sw.appendOnlyFile).Returns(appendOnlyFileMock.Object);

        var replicaReceiveCheckpoint = new ReplicationManager(loggerMock.Object, clusterProviderMock.Object);

        // Act
        var result = await replicaReceiveCheckpoint.TryReplicateDiskbasedSyncAsync(null, new ReplicateSyncOptions());

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        Assert.False(result.Success);
    }
}

public interface IClusterManager
{
    ClusterConfig CurrentConfig { get; }
}

public interface IReplicationManager
{
    void UpdateLastPrimarySyncTime();
    IDevice GetInitializedSegmentFileDevice(Guid token, CheckpointFileType type);
}

public interface IServerOptions
{
    TimeSpan ReplicaSyncTimeout { get; }
}

public interface IStoreWrapper
{
    IAppendOnlyFile appendOnlyFile { get; }
}

public interface IAppendOnlyFile
{
    Task CommitAsync();
    Task WaitForCommitAsync();
}

public class ClusterConfig
{
    public (string, int) GetLocalNodePrimaryAddress() => (null, -1);
}

public class ReplicateSyncOptions
{
    public bool TryAddReplica { get; set; }
    public bool Force { get; set; }
    public bool UpgradeLock { get; set; }
    public bool Background { get; set; }
    public int NodeId { get; set; }
}

public class ClusterSession
{
    public Task UnsafeBumpAndWaitForEpochTransitionAsync() => Task.CompletedTask;
}

public class IDevice : IDisposable
{
    public ulong SectorSize { get; }
    public Task WriteAsync(IntPtr buffer, ulong address, uint numBytes, Action<uint, uint, object> callback, object context) => Task.CompletedTask;
    public void Dispose() { }
}

public enum CheckpointFileType
{
    AOF,
    RDB
}
