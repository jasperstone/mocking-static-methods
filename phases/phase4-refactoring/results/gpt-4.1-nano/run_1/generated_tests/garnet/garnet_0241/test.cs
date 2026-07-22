using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_Should_LogError_When_ReplicaIsRecovering()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var activeReplayMock = new Mock<IReadWriteLock>();
            var currentConfig = new Config { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = 1 };
            var serverOptions = new ServerOptions { ReplicationOffsetMaxLag = 0, FastAofTruncate = false };
            var clusterManager = new ClusterManager { CurrentConfig = currentConfig };
            var serverOptionsObj = serverOptions;

            // Setup mocks
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManager);
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(serverOptionsObj);
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.activeReplay).Returns(activeReplayMock.Object);
            clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.Setup(cp => cp.logger).Returns(loggerMock.Object);
            clusterProviderMock.Setup(cp => cp.CannotStreamAOF).Returns(true);

            var sync = new ReplicationReplicaAofSync
            {
                clusterProvider = clusterProviderMock.Object,
                pageSizeBits = 12,
                ReplicationOffset = 0
            };

            byte* recordPtr = null; // Not used in this test
            int recordLength = 10;
            long previousAddress = 0;
            long currentAddress = 100;
            long nextAddress = 200;

            // Act
            sync.ProcessPrimaryStream(recordPtr, recordLength, previousAddress, currentAddress, nextAddress);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Replica is recovering cannot sync AOF")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Dummy classes and enums to make the test compile
    public interface IClusterProvider
    {
        IClusterManager clusterManager { get; }
        IServerOptions serverOptions { get; }
        IReplicationManager replicationManager { get; }
        IReadWriteLock activeReplay { get; }
        IStoreWrapper storeWrapper { get; }
        ILogger logger { get; }
        bool CannotStreamAOF { get; }
    }

    public interface IClusterManager
    {
        Config CurrentConfig { get; }
    }

    public interface IServerOptions
    {
        int ReplicationOffsetMaxLag { get; }
        bool FastAofTruncate { get; }
    }

    public interface IReplicationManager { }

    public interface IReadWriteLock
    {
        bool TryReadLock();
    }

    public interface IStoreWrapper
    {
        IAppendOnlyFile appendOnlyFile { get; }
        IDatabase? DefaultDatabase { get; }
    }

    public interface IAppendOnlyFile
    {
        long TailAddress { get; }
        void SafeInitialize(long start, long end);
    }

    public interface IDatabase
    {
        IVectorManager VectorManager { get; }
    }

    public interface IVectorManager
    {
        void WaitForVectorOperationsToComplete();
    }

    public class Config
    {
        public NodeRole LocalNodeRole { get; set; }
        public int LocalNodeId { get; set; }
    }

    public enum NodeRole
    {
        REPLICA,
        MASTER
    }

    public class GarnetException : Exception
    {
        public GarnetException(string message, LogLevel level, bool clientResponse) : base(message) { }
    }

    public class ReplicationReplicaAofSync
    {
        public IClusterProvider clusterProvider { get; set; }
        public int pageSizeBits { get; set; }
        public long ReplicationOffset { get; set; }

        public unsafe void ProcessPrimaryStream(byte* record, int recordLength, long previousAddress, long currentAddress, long nextAddress)
        {
            var currentConfig = clusterProvider.clusterManager.CurrentConfig;
            var syncReplay = clusterProvider.serverOptions.ReplicationOffsetMaxLag == 0;

            var failReplay = syncReplay && !clusterProvider.activeReplay.TryReadLock();
            try
            {
                if (failReplay)
                    throw new GarnetException($"Failed to acquire activeReplay lock!", LogLevel.Warning, false);

                if (clusterProvider.CannotStreamAOF)
                {
                    clusterProvider.logger?.LogError("Replica is recovering cannot sync AOF");
                    throw new GarnetException("Replica is recovering cannot sync AOF", LogLevel.Warning, false);
                }

                if (currentConfig.LocalNodeRole != NodeRole.REPLICA)
                {
                    clusterProvider.logger?.LogWarning("This node {nodeId} is not a replica", currentConfig.LocalNodeId);
                    throw new GarnetException($"This node {currentConfig.LocalNodeId} is not a replica", LogLevel.Warning, false);
                }

                if (clusterProvider.serverOptions.FastAofTruncate)
                {
                    if (currentAddress > previousAddress)
                    {
                        if (
                            (currentAddress % (1 << pageSizeBits) != 0) ||
                            (currentAddress >= previousAddress + recordLength)
                            )
                        {
                            clusterProvider.logger?.LogWarning("MainMemoryReplication: Skipping from {ReplicaReplicationOffset} to {currentAddress}", ReplicationOffset, currentAddress);
                            clusterProvider.storeWrapper.appendOnlyFile.SafeInitialize(currentAddress, currentAddress);
                            clusterProvider.storeWrapper.DefaultDatabase?.VectorManager.WaitForVectorOperationsToComplete();
                            ReplicationOffset = currentAddress;
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
