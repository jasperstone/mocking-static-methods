using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        private Mock<ILogger<ReplicationManager>> _loggerMock;
        private Mock<IClusterProvider> _clusterProviderMock;
        private Mock<IStoreWrapper> _storeWrapperMock;
        private Mock<IAppendOnlyFile> _appendOnlyFileMock;
        private Mock<IReplicationManager> _replicationManagerMock;
        private Mock<IClusterManager> _clusterManagerMock;
        private Mock<IClientConfig> _clientConfigMock;
        private Mock<IVectorManager> _vectorManagerMock;
        private Mock<IActiveReplay> _activeReplayMock;
        private Mock<IStoreWrapper> _storeWrapper;
        private Mock<IClusterProvider> _clusterProvider;
        private Mock<IClusterManager> _clusterManager;
        private Mock<IAppendOnlyFile> _appendOnlyFile;
        private Mock<IReplicationManager> _replicationManager;
        private Mock<IClientConfig> _currentConfig;
        private Mock<IVectorManager> _vectorManager;
        private Mock<IActiveReplay> _activeReplay;

        public ReplicationReplicaAofSyncTests()
        {
            _loggerMock = new Mock<ILogger<ReplicationManager>>();
            _clusterProviderMock = new Mock<IClusterProvider>();
            _storeWrapperMock = new Mock<IStoreWrapper>();
            _appendOnlyFileMock = new Mock<IAppendOnlyFile>();
            _replicationManagerMock = new Mock<IReplicationManager>();
            _clusterManagerMock = new Mock<IClusterManager>();
            _clientConfigMock = new Mock<IClientConfig>();
            _vectorManagerMock = new Mock<IVectorManager>();
            _activeReplayMock = new Mock<IActiveReplay>();

            // Setup default behaviors
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 0, EnableFastCommit = false, FastAofTruncate = false });
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _clusterProviderMock.Setup(cp => cp.replayIterator).Returns((IReplayIterator)null);
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.DefaultDatabase).Returns(new Database { VectorManager = _vectorManagerMock.Object });
            _clusterProviderMock.Setup(cp => cp.TailAddress).Returns(0L);
            _clusterProviderMock.Setup(cp => cp.TailAddress).Returns(0L);
            _clusterProviderMock.Setup(cp => cp.storeWrapper.appendOnlyFile).Returns(_appendOnlyFileMock.Object);
            _clusterProviderMock.Setup(cp => cp.storeWrapper.DefaultDatabase).Returns(new Database { VectorManager = _vectorManagerMock.Object });
            _clusterProviderMock.Setup(cp => cp.storeWrapper.appendOnlyFile.TailAddress).Returns(0L);
            _clusterProviderMock.Setup(cp => cp.storeWrapper.appendOnlyFile).Returns(_appendOnlyFileMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" });
            _clusterProviderMock.Setup(cp => cp.replayIterator).Returns((IReplayIterator)null);
            _clusterProviderMock.Setup(cp => cp.CannotStreamAOF).Returns(false);
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 0, EnableFastCommit = false, FastAofTruncate = false });
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _clusterProviderMock.Setup(cp => cp.DefaultDatabase).Returns(new Database { VectorManager = _vectorManagerMock.Object });
            _clusterProviderMock.Setup(cp => cp.TailAddress).Returns(0L);
            _clusterProviderMock.Setup(cp => cp.replayIterator).Returns((IReplayIterator)null);
            _clusterProviderMock.Setup(cp => cp.CannotStreamAOF).Returns(false);
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 0, EnableFastCommit = false, FastAofTruncate = false });
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _clusterProviderMock.Setup(cp => cp.DefaultDatabase).Returns(new Database { VectorManager = _vectorManagerMock.Object });
            _clusterProviderMock.Setup(cp => cp.TailAddress).Returns(0L);
            _clusterProviderMock.Setup(cp => cp.replayIterator).Returns((IReplayIterator)null);
            _clusterProviderMock.Setup(cp => cp.CannotStreamAOF).Returns(false);
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 0, EnableFastCommit = false, FastAofTruncate = false });
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _clusterProviderMock.Setup(cp => cp.DefaultDatabase).Returns(new Database { VectorManager = _vectorManagerMock.Object });
            _clusterProviderMock.Setup(cp => cp.TailAddress).Returns(0L);
            _clusterProviderMock.Setup(cp => cp.replayIterator).Returns((IReplayIterator)null);
            _clusterProviderMock.Setup(cp => cp.CannotStreamAOF).Returns(false);
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 0, EnableFastCommit = false, FastAofTruncate = false });
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);
            _clusterProviderMock.Setup(cp => cp.DefaultDatabase).Returns(new Database { VectorManager = _vectorManagerMock.Object });
            _clusterProviderMock.Setup(cp => cp.TailAddress).Returns(0L);
            _clusterProviderMock.Setup(cp => cp.replayIterator).Returns((IReplayIterator)null);
            _clusterProviderMock.Setup(cp => cp.CannotStreamAOF).Returns(false);
        }

        [Fact]
        public void ProcessPrimaryStream_Should_LogError_When_ReplicaIsRecovering()
        {
            // Arrange
            var manager = new ReplicationManager
            {
                logger = _loggerMock.Object,
                clusterProvider = _clusterProviderMock.Object,
                storeWrapper = _storeWrapperMock.Object,
                pageSizeBits = 12,
                ReplicationOffset = 0,
                activeReplay = _activeReplayMock.Object
            };

            // Setup to simulate CannotStreamAOF true
            _clusterProviderMock.Setup(cp => cp.CannotStreamAOF).Returns(true);

            byte[] recordBytes = new byte[] { 1, 2, 3, 4 };
            var recordPtr = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(recordBytes, 0);
            int recordLength = recordBytes.Length;
            long previousAddress = 0;
            long currentAddress = 0;
            long nextAddress = 0;

            // Act & Assert
            var ex = Assert.Throws<GarnetException>(() =>
                manager.ProcessPrimaryStream(recordPtr, recordLength, previousAddress, currentAddress, nextAddress));

            _loggerMock.Verify(
                x => x.LogError("Replica is recovering cannot sync AOF"),
                Times.Once);
        }

        [Fact]
        public void ProcessPrimaryStream_Should_LogWarning_When_NodeIsNotReplica()
        {
            // Arrange
            var manager = new ReplicationManager
            {
                logger = _loggerMock.Object,
                clusterProvider = _clusterProviderMock.Object,
                storeWrapper = _storeWrapperMock.Object,
                pageSizeBits = 12,
                ReplicationOffset = 0,
                activeReplay = _activeReplayMock.Object
            };

            // Setup to simulate current config role not being REPLICA
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new ClusterConfig
            {
                LocalNodeRole = NodeRole.MASTER,
                LocalNodeId = "node1"
            });

            byte[] recordBytes = new byte[] { 1, 2, 3, 4 };
            var recordPtr = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(recordBytes, 0);
            int recordLength = recordBytes.Length;
            long previousAddress = 0;
            long currentAddress = 0;
            long nextAddress = 0;

            // Act & Assert
            var ex = Assert.Throws<GarnetException>(() =>
                manager.ProcessPrimaryStream(recordPtr, recordLength, previousAddress, currentAddress, nextAddress));

            _loggerMock.Verify(
                x => x.LogWarning("This node {nodeId} is not a replica", "node1"),
                Times.Once);
        }

        [Fact]
        public void ProcessPrimaryStream_Should_LogWarning_When_FastAofTruncateAndOffsetMismatch()
        {
            // Arrange
            var manager = new ReplicationManager
            {
                logger = _loggerMock.Object,
                clusterProvider = _clusterProviderMock.Object,
                storeWrapper = _storeWrapperMock.Object,
                pageSizeBits = 12,
                ReplicationOffset = 0,
                activeReplay = _activeReplayMock.Object
            };

            // Setup to simulate FastAofTruncate enabled
            _clusterProviderMock.Setup(cp => cp.serverOptions.FastAofTruncate).Returns(true);
            // Setup currentAddress > previousAddress to trigger the condition
            long previousAddress = 0;
            long currentAddress = 4096; // 1 << 12
            long nextAddress = 0;

            byte[] recordBytes = new byte[] { 1, 2, 3, 4 };
            var recordPtr = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(recordBytes, 0);
            int recordLength = recordBytes.Length;

            // Act
            manager.ProcessPrimaryStream(recordPtr, recordLength, previousAddress, currentAddress, nextAddress);

            // Verify that SafeInitialize was called
            _storeWrapperMock.Verify(s => s.appendOnlyFile.SafeInitialize(currentAddress, currentAddress), Times.Once);
            _vectorManagerMock.Verify(v => v.WaitForVectorOperationsToComplete(), Times.Once);
        }
    }

    // Dummy classes and enums to support the test
    public class ServerOptions
    {
        public int ReplicationOffsetMaxLag { get; set; }
        public bool EnableFastCommit { get; set; }
        public bool FastAofTruncate { get; set; }
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

    public class Database
    {
        public IVectorManager VectorManager { get; set; }
    }

    public interface IVectorManager
    {
        void WaitForVectorOperationsToComplete();
    }

    public interface IActiveReplay { }

    public interface IClusterProvider
    {
        IClusterManager clusterManager { get; }
        ServerOptions serverOptions { get; }
        IStoreWrapper storeWrapper { get; }
        IReplayIterator replayIterator { get; }
        IReplicationManager replicationManager { get; }
        IClientConfig DefaultDatabase { get; }
        long TailAddress { get; }
        bool CannotStreamAOF { get; }
    }

    public interface IClusterManager
    {
        ClusterConfig CurrentConfig { get; }
    }

    public interface IStoreWrapper
    {
        IAppendOnlyFile appendOnlyFile { get; }
        IDefaultDatabase DefaultDatabase { get; }
        long TailAddress { get; }
    }

    public interface IAppendOnlyFile
    {
        long TailAddress { get; }
        void SafeInitialize(long start, long end);
        void UnsafeEnqueueRaw(Span<byte> data, bool noCommit);
        IScanResult ScanSingle(long previousAddress, bool lo);
    }

    public interface IScanResult { }

    public interface IReplayIterator { }

    public interface IReplicationManager { }

    public interface IClientConfig { }

    public class ReplicationManager
    {
        public ILogger<ReplicationManager> logger;
        public IClusterProvider clusterProvider;
        public IStoreWrapper storeWrapper;
        public int pageSizeBits;
        public long ReplicationOffset;
        public IActiveReplay activeReplay;

        public void ProcessPrimaryStream(byte* record, int recordLength, long previousAddress, long currentAddress, long nextAddress)
        {
            // Implementation as in the original code
        }

        public void ThrottlePrimary()
        {
            // Implementation as in the original code
        }
    }
}
