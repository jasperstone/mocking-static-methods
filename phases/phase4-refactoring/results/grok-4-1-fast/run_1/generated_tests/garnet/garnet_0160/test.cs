using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class AofTaskStoreLoggerTests
    {
        [Fact]
        public void LogErrorExtension_IsCalled_WhenTruncationPreventsTaskAddition()
        {
            // Arrange - Mock ILogger to verify LogError extension call
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

            // Create minimal mock dependencies that satisfy constructor
            var mockClusterProvider = new Mock<ClusterProvider>();
            mockClusterProvider.Setup(cp => cp.AllowDataLoss).Returns(false);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(new Mock<ClusterManager>().Object);
            mockClusterProvider.SetupGet(cp => cp.serverOptions).Returns(new Mock<ServerOptions>().Object);
            mockClusterProvider.SetupGet(cp => cp.replicationManager).Returns(new Mock<ReplicationManager>().Object);
            mockClusterProvider.SetupGet(cp => cp.ClusterUsername).Returns("test");
            mockClusterProvider.SetupGet(cp => cp.ClusterPassword).Returns("test");
            
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockAof = new Mock<AppendOnlyFile>();
            mockAof.Setup(x => x.UnsafeGetLogPageSizeBits()).Returns(12);
            mockAof.Setup(x => x.UnsafeGetReadOnlyAddressLagOffset()).Returns(8192L);
            mockStoreWrapper.SetupGet(x => x.appendOnlyFile).Returns(mockAof.Object);
            mockClusterProvider.SetupGet(cp => cp.storeWrapper).Returns(mockStoreWrapper.Object);

            // Create AofTaskStore instance
            var store = new AofTaskStore(mockClusterProvider.Object, logger: mockLogger.Object);

            // Use reflection to set up the exact condition for line 271 LogError call
            // _lock.WriteLock() acquired, !_disposed, startAddress < TruncatedUntil && !AllowDataLoss
            var truncatedField = typeof(AofTaskStore).GetField("TruncatedUntil", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            truncatedField?.SetValue(store, 1000L);

            var disposedField = typeof(AofTaskStore).GetField("_disposed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            disposedField?.SetValue(store, false);

            // Get the private TryAddReplicationTasks method via reflection
            var method = typeof(AofTaskStore).GetMethod("TryAddReplicationTasks", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            // Create empty replicaSyncSessions array to pass to the method
            var replicaSyncSessions = Array.Empty<ReplicaSyncSession>();

            // Act - Invoke the method to hit the LogError path at line 271
            object? result = method.Invoke(store, new object[] { replicaSyncSessions, 500L });

            // Assert - Verify the exact LogError extension call from line 271
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, t) => 
                        state?.ToString()?.Contains("TryAddReplicationTasks failed to add tasks for AOF sync 500 1000") == true),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Verify the method returns false as expected
            Assert.False((bool)result!);
        }
    }

    // Minimal test doubles for constructor satisfaction
    public class ClusterProvider 
    {
        public virtual bool AllowDataLoss => false;
        public virtual ClusterManager clusterManager { get; set; } = null!;
        public virtual ServerOptions serverOptions { get; set; } = null!;
        public virtual ReplicationManager replicationManager { get; set; } = null!;
        public virtual string ClusterUsername => "";
        public virtual string ClusterPassword => "";
        public virtual StoreWrapper storeWrapper { get; set; } = null!;
    }

    public class ClusterManager 
    {
        public virtual ClusterConfig CurrentConfig => new();
    }

    public class ClusterConfig 
    {
        public virtual string LocalNodeId => "local";
        public virtual (string, int) GetWorkerAddressFromNodeId(string nodeId) => ("127.0.0.1", 7001);
    }

    public class ServerOptions 
    {
        public virtual TlsOptions TlsOptions => null;
    }

    public class TlsOptions 
    {
        public virtual TlsClientOptions TlsClientOptions => null;
    }

    public class TlsClientOptions { }

    public class ReplicationManager 
    {
        public virtual NetworkBufferSettings GetAofSyncNetworkBufferSettings() => null!;
        public virtual NetworkPool GetNetworkPool() => null!;
    }

    public class NetworkBufferSettings { }
    public class NetworkPool { }

    public class StoreWrapper 
    {
        public virtual AppendOnlyFile appendOnlyFile { get; set; } = null!;
    }

    public class AppendOnlyFile 
    {
        public virtual int UnsafeGetLogPageSizeBits() => 12;
        public virtual long UnsafeGetReadOnlyAddressLagOffset() => 8192L;
        public virtual Action<long, long> SafeTailShiftCallback { get; set; } = null!;
    }

    public class ReplicaSyncSession { }

    public class AofSyncTaskInfo : IDisposable 
    {
        public virtual string remoteNodeId => "";
        public virtual GarnetClientSession garnetClient => null!;
        public virtual long previousAddress => 0;
        public virtual AofSyncTask AofSyncTask => null!;
        public void Dispose() { }
    }

    public class AofSyncTask : IDisposable 
    {
        public void Dispose() { }
    }

    public class GarnetClientSession : IDisposable 
    {
        public virtual bool IsConnected => true;
        public void Dispose() { }
    }
}
