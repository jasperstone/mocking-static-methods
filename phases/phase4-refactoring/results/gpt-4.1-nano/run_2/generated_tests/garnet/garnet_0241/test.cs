using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void ProcessPrimaryStream_Should_LogError_When_CannotStreamAOF()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var appendOnlyFileMock = new Mock<AppendOnlyFile>();
            var defaultDatabaseMock = new Mock<Database>();
            var vectorManagerMock = new Mock<VectorManager>();
            var replicationManagerMock = new Mock<ReplicationManager>();

            // Setup clusterManager.CurrentConfig
            var currentConfig = new ClusterConfig { LocalNodeRole = NodeRole.REPLICA, LocalNodeId = "node1" };
            clusterManagerMock.Setup(c => c.CurrentConfig).Returns(currentConfig);

            // Setup clusterProvider
            clusterProviderMock.Setup(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.Setup(c => c.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 0, FastAofTruncate = false, EnableFastCommit = false });
            clusterProviderMock.Setup(c => c.replicationManager).Returns(new ReplicationManager());
            clusterProviderMock.Setup(c => c.storeWrapper).Returns(storeWrapperMock.Object);
            clusterProviderMock.Setup(c => c.storeWrapper.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            clusterProviderMock.Setup(c => c.storeWrapper.DefaultDatabase).Returns(defaultDatabaseMock.Object);
            clusterProviderMock.Setup(c => c.storeWrapper.DefaultDatabase.VectorManager).Returns(vectorManagerMock.Object);
            clusterProviderMock.Setup(c => c.storeWrapper.appendOnlyFile.TailAddress).Returns(1000L);
            clusterProviderMock.Setup(c => c.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 0, FastAofTruncate = false, EnableFastCommit = false });
            clusterProviderMock.Setup(c => c.replicationManager.CannotStreamAOF).Returns(true);

            // Setup storeWrapper
            storeWrapperMock.Setup(s => s.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            storeWrapperMock.Setup(s => s.DefaultDatabase).Returns(defaultDatabaseMock.Object);
            storeWrapperMock.Setup(s => s.DefaultDatabase.VectorManager).Returns(vectorManagerMock.Object);
            storeWrapperMock.Setup(s => s.serverOptions).Returns(new ServerOptions { ReplicationOffsetMaxLag = 0, FastAofTruncate = false, EnableFastCommit = false });

            // Create an instance of ReplicationManager
            var replicationManager = new ReplicationManager
            {
                // Inject dependencies
                clusterProvider = clusterProviderMock.Object,
                logger = loggerMock.Object,
                activeReplay = new ActiveReplay(),
                storeWrapper = storeWrapperMock.Object,
                pageSizeBits = 12,
                ReplicationOffset = 0,
                replayIterator = null
            };

            // Prepare dummy data for ProcessPrimaryStream
            byte[] recordData = new byte[] { 1, 2, 3, 4 };
            var recordPtr = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(recordData, 0);

            // Act
            // Call with parameters that will trigger the logError branch
            var exception = Record.Exception(() =>
                replicationManager.ProcessPrimaryStream(recordPtr, recordData.Length, 900, 1000, 1100));

            // Assert
            // Verify that LogError was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Replica is recovering cannot sync AOF")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }

    // Dummy classes to satisfy dependencies
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

    public class ClusterProvider
    {
        public ClusterManager clusterManager { get; set; }
        public ServerOptions serverOptions { get; set; }
        public ReplicationManager replicationManager { get; set; }
        public StoreWrapper storeWrapper { get; set; }
    }

    public class ClusterManager
    {
        public ClusterConfig CurrentConfig { get; set; }
    }

    public class StoreWrapper
    {
        public AppendOnlyFile appendOnlyFile { get; set; }
        public Database DefaultDatabase { get; set; }
        public ServerOptions serverOptions { get; set; }
    }

    public class AppendOnlyFile
    {
        public long TailAddress { get; set; }
        public void SafeInitialize(long start, long end) { }
        public void UnsafeEnqueueRaw(Span<byte> data, bool noCommit) { }
        public ScanIterator ScanSingle(long address, bool lo) => null;
    }

    public class Database
    {
        public VectorManager VectorManager { get; set; }
    }

    public class VectorManager
    {
        public void WaitForVectorOperationsToComplete() { }
    }

    public class ServerOptions
    {
        public int ReplicationOffsetMaxLag { get; set; }
        public bool FastAofTruncate { get; set; }
        public bool EnableFastCommit { get; set; }
    }

    public class ActiveReplay
    {
        public bool TryReadLock() => true;
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
