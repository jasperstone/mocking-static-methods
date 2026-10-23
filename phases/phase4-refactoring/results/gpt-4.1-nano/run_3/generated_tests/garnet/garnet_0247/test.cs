using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void ProcessPrimaryStream_ShouldLogWarningOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var appendOnlyFileMock = new Mock<AppendOnlyFile>();
            var defaultDatabaseMock = new Mock<Database>();
            var vectorManagerMock = new Mock<VectorManager>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var replicationManagerMock = new Mock<ReplicationManager>();

            // Setup clusterProvider
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(new ClusterManager { CurrentConfig = new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" } });
            clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions { EnableFastCommit = false, FastAofTruncate = false, ReplOffsetMaxLag = 0 });
            clusterProviderMock.Setup(cp => cp.activeReplay).Returns(new ActiveReplay());
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(new ClusterManager { CurrentConfig = new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" } });
            clusterProviderMock.Setup(cp => cp.DefaultDatabase).Returns(defaultDatabaseMock.Object);
            clusterProviderMock.Setup(cp => cp.replayIterator).Returns((object)null);
            clusterProviderMock.Setup(cp => cp.storeWrapper.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            clusterProviderMock.Setup(cp => cp.storeWrapper.DefaultDatabase).Returns(defaultDatabaseMock.Object);
            clusterProviderMock.Setup(cp => cp.storeWrapper.appendOnlyFile.TailAddress).Returns(100);
            clusterProviderMock.Setup(cp => cp.storeWrapper.appendOnlyFile.ScanSingle(It.IsAny<long>(), It.IsAny<long>(), true, false, false))
                .Returns((object)null);

            // Setup appendOnlyFile mock
            appendOnlyFileMock.Setup(ao => ao.TailAddress).Returns(100);
            appendOnlyFileMock.Setup(ao => ao.SafeInitialize(It.IsAny<long>(), It.IsAny<long>()));
            appendOnlyFileMock.Setup(ao => ao.UnsafeEnqueueRaw(It.IsAny<Span<byte>>(), It.IsAny<bool>())).Returns(true);

            // Setup activeReplay
            var activeReplay = new ActiveReplay();
            var activeReplayMock = new Mock<ActiveReplay>();
            activeReplayMock.Setup(ar => ar.TryReadLock()).Returns(true);
            clusterProviderMock.Setup(cp => cp.activeReplay).Returns(activeReplay);

            // Setup ExceptionInjectionHelper to throw
            ExceptionInjectionHelper.TriggerException(ExceptionInjectionType.Divergent_AOF_Stream);

            var manager = new ReplicationManager
            {
                clusterProvider = clusterProviderMock.Object,
                logger = loggerMock.Object,
                pageSizeBits = 12,
                ReplicationOffset = 50,
                replayIterator = null,
                storeWrapper = storeWrapperMock.Object
            };

            // Prepare record data
            byte[] recordData = new byte[10];
            GCHandle handle = GCHandle.Alloc(recordData, GCHandleType.Pinned);
            try
            {
                // Act
                var ex = Record.Exception(() =>
                {
                    unsafe
                    {
                        fixed (byte* recordPtr = recordData)
                        {
                            manager.ProcessPrimaryStream(recordPtr, recordData.Length, 40, 50, 60);
                        }
                    }
                });

                // Assert
                Assert.NotNull(ex);
                loggerMock.Verify(
                    x => x.LogWarning(It.IsAny<Exception>(), "An exception occurred at ReplicationManager.ProcessPrimaryStream"),
                    Times.AtLeastOnce);
            }
            finally
            {
                handle.Free();
            }
        }
    }

    // Dummy classes and enums to support the test
    public class ClusterProvider
    {
        public ClusterManager clusterManager { get; set; }
        public StoreWrapper storeWrapper { get; set; }
        public ActiveReplay activeReplay { get; set; }
        public ServerOptions serverOptions { get; set; }
        public string LocalNodeId { get; set; }
        public ClusterConfig CurrentConfig { get; set; }
        public object DefaultDatabase { get; set; }
        public object replayIterator { get; set; }
        public ReplicationManager replicationManager { get; set; }
    }

    public class StoreWrapper
    {
        public AppendOnlyFile appendOnlyFile { get; set; }
        public Database DefaultDatabase { get; set; }
    }

    public class AppendOnlyFile
    {
        public long TailAddress { get; set; }
        public bool SafeInitialize(long address, long currentAddress) => true;
        public bool UnsafeEnqueueRaw(Span<byte> data, bool noCommit) => true;
        public object ScanSingle(long previousAddress, long maxAddress, bool scanUncommitted, bool recover, bool someFlag) => null;
    }

    public class Database
    {
        public VectorManager VectorManager { get; set; } = new VectorManager();
    }

    public class VectorManager
    {
        public void WaitForVectorOperationsToComplete() { }
    }

    public class ActiveReplay
    {
        public bool TryReadLock() => true;
    }

    public class ClusterManager
    {
        public ClusterConfig CurrentConfig { get; set; }
    }

    public class ClusterConfig
    {
        public NodeRole LocalNodeRole { get; set; }
        public string LocalNodeId { get; set; }
    }

    public enum NodeRole
    {
        REPLICA,
        PRIMARY
    }

    public class ServerOptions
    {
        public bool EnableFastCommit { get; set; }
        public bool FastAofTruncate { get; set; }
        public int ReplOffsetMaxLag { get; set; }
        public bool ReplOffsetMaxLagEnabled => ReplOffsetMaxLag != -1;
    }

    public static class ExceptionInjectionHelper
    {
        public static void TriggerException(ExceptionInjectionType type) { }
    }

    public enum ExceptionInjectionType
    {
        Divergent_AOF_Stream
    }
}
