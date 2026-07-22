using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_ShouldLogError_WhenCannotStreamAOF()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationReplicaAofSync>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var replicationManagerMock = new Mock<IReplicationManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var currentConfig = new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node-1" };
            var serverOptions = new ServerOptions { ReplicationOffsetMaxLag = 0, FastAofTruncate = false };
            var activeReplayMock = new Mock<IReadLock>();

            // Setup clusterProvider mock
            clusterProviderMock.Setup(c => c.clusterManager).Returns(new Mock<IClusterManager>().Object);
            clusterProviderMock.Setup(c => c.CurrentConfig).Returns(currentConfig);
            clusterProviderMock.Setup(c => c.serverOptions).Returns(serverOptions);
            clusterProviderMock.Setup(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(c => c.activeReplay).Returns(activeReplayMock.Object);
            clusterProviderMock.Setup(c => c.storeWrapper).Returns(storeWrapperMock.Object);

            // Instantiate the class under test
            var replication = new ReplicationReplicaAofSync(
                loggerMock.Object,
                clusterProviderMock.Object,
                storeWrapperMock.Object,
                pageSizeBits: 12,
                ReplicationOffset: 0);

            // Act
            // Setup the mock to simulate CannotStreamAOF being true
            replicationManagerMock.Setup(r => r.CannotStreamAOF).Returns(true);

            unsafe
            {
                byte dummyByte = 0;
                fixed (byte* ptr = &dummyByte)
                {
                    // Call the method, expecting it to log error
                    replication.ProcessPrimaryStream(ptr, 10, 0, 100, 200);
                }
            }

            // Assert
            // Verify that LogError was called with the expected message
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

    // Dummy interfaces and classes to make the test compile
    public interface IClusterProvider
    {
        IClusterManager clusterManager { get; }
        IReplicationManager replicationManager { get; }
        IStoreWrapper storeWrapper { get; }
        ServerOptions serverOptions { get; }
        CurrentConfigHolder CurrentConfig { get; }
        IActiveReplay activeReplay { get; }
    }

    public interface IClusterManager
    {
        ClusterConfig CurrentConfig { get; }
    }

    public interface IReplicationManager
    {
        bool CannotStreamAOF { get; }
    }

    public interface IStoreWrapper
    {
        IAppendOnlyFile appendOnlyFile { get; }
        IDatabase DefaultDatabase { get; }
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

    public class ClusterConfig
    {
        public NodeRole LocalNodeRole { get; set; }
        public string LocalNodeId { get; set; }
    }

    public enum NodeRole
    {
        REPLICA,
        MASTER
    }

    public class ServerOptions
    {
        public int ReplicationOffsetMaxLag { get; set; }
        public bool FastAofTruncate { get; set; }
    }

    public class CurrentConfigHolder
    {
        public ClusterConfig CurrentConfig { get; set; }
    }

    public interface IActiveReplay
    {
        bool TryReadLock();
    }

    public class GarnetException : Exception
    {
        public GarnetException(string message, LogLevel level, bool clientResponse) : base(message) { }
    }

    // Dummy class under test
    public class ReplicationReplicaAofSync
    {
        private readonly ILogger<ReplicationReplicaAofSync> logger;
        private readonly IClusterProvider clusterProvider;
        private readonly IStoreWrapper storeWrapper;
        private readonly int pageSizeBits;
        public long ReplicationOffset { get; set; }

        public ReplicationReplicaAofSync(
            ILogger<ReplicationReplicaAofSync> logger,
            IClusterProvider clusterProvider,
            IStoreWrapper storeWrapper,
            int pageSizeBits,
            long ReplicationOffset)
        {
            this.logger = logger;
            this.clusterProvider = clusterProvider;
            this.storeWrapper = storeWrapper;
            this.pageSizeBits = pageSizeBits;
            this.ReplicationOffset = ReplicationOffset;
        }

        public unsafe void ProcessPrimaryStream(byte* record, int recordLength, long previousAddress, long currentAddress, long nextAddress)
        {
            var currentConfig = clusterProvider.CurrentConfig;
            var syncReplay = clusterProvider.serverOptions.ReplicationOffsetMaxLag == 0;

            var failReplay = syncReplay && !clusterProvider.activeReplay.TryReadLock();
            try
            {
                if (failReplay)
                    throw new GarnetException($"Failed to acquire activeReplay lock!", LogLevel.Warning, false);

                if (clusterProvider.replicationManager.CannotStreamAOF)
                {
                    logger?.LogError("Replica is recovering cannot sync AOF");
                    throw new GarnetException("Replica is recovering cannot sync AOF", LogLevel.Warning, false);
                }

                if (currentConfig.LocalNodeRole != NodeRole.REPLICA)
                {
                    logger?.LogWarning("This node {nodeId} is not a replica", currentConfig.LocalNodeId);
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
                            logger?.LogWarning("MainMemoryReplication: Skipping from {ReplicaReplicationOffset} to {currentAddress}", 0, currentAddress);
                            storeWrapper.appendOnlyFile.SafeInitialize(currentAddress, currentAddress);
                            storeWrapper.DefaultDatabase?.VectorManager.WaitForVectorOperationsToComplete();
                            this.ReplicationOffset = currentAddress;
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
