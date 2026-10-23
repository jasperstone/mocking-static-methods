using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class AofTaskStoreLoggerTests
    {
        [Fact]
        public void TryAddReplicationTasks_LogsError_WhenStartAddressBeforeTruncatedUntilAndAllowDataLossFalse()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<AofTaskStore>>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TryAddReplicationTasks") && v.ToString()!.Contains("failed to add tasks") && v.ToString()!.Contains("500") && v.ToString()!.Contains("1000")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            var mockClusterProvider = new Mock<ClusterProvider>();
            mockClusterProvider.Setup(cp => cp.AllowDataLoss).Returns(false);
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(new MockClusterManager());
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(new MockServerOptions());

            var store = new AofTaskStore(mockClusterProvider.Object, logger: mockLogger.Object);
            
            // Use reflection to set internal fields
            typeof(AofTaskStore).GetField("TruncatedUntil", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                                .SetValue(store, 1000L);
            typeof(AofTaskStore).GetField("_lock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                                .SetValue(store, new MockLock());

            // Act
            var result = store.TryAddReplicationTasks(Array.Empty<ReplicaSyncSession>(), 500L);

            // Assert
            mockLogger.VerifyAll();
            Assert.False(result);
        }

        [Fact]
        public void TryAddReplicationTasks_LogsErrorWithException_WhenTaskCreationFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<AofTaskStore>>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TryAddReplicationTasks") && v.ToString()!.Contains("creating AOF sync task") && v.ToString()!.Contains("replica1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            var mockClusterProvider = new Mock<ClusterProvider>();
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(new MockClusterManager { FailGetWorkerAddress = true });
            
            var store = new AofTaskStore(mockClusterProvider.Object, logger: mockLogger.Object);
            typeof(AofTaskStore).GetField("_lock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                                .SetValue(store, new MockLock());

            // Act
            var result = store.TryAddReplicationTasks(new[] { new MockReplicaSyncSession() }, 100L);

            // Assert
            mockLogger.VerifyAll();
            Assert.False(result);
        }
    }

    // Minimal test doubles
    public class MockClusterManager
    {
        public ClusterConfig clusterManager { get; } = new MockClusterConfig();
        public bool FailGetWorkerAddress { get; set; }
    }

    public class MockClusterConfig : ClusterConfig
    {
        public override (string, int) GetWorkerAddressFromNodeId(string nodeId) => ("127.0.0.1", 7001);
    }

    public class MockServerOptions
    {
        public TlsOptions TlsOptions => null;
    }

    public class MockLock : SingleWriterMultiReaderLock
    {
        public override void WriteLock() { }
        public override void WriteUnlock() { }
        public override void ReadLock() { }
        public override void ReadUnlock() { }
    }

    public class MockReplicaSyncSession : ReplicaSyncSession
    {
        public override string replicaNodeId => "replica1";
    }

    // Minimal stubs for referenced types
    public class ClusterProvider { public ClusterManager clusterManager { get; set; } = new(); public ServerOptions serverOptions { get; set; } = new(); public bool AllowDataLoss => false; }
    public class ClusterManager { public ClusterConfig CurrentConfig => new(); }
    public class ClusterConfig { public virtual (string, int) GetWorkerAddressFromNodeId(string nodeId) => (null, 0); public string LocalNodeId => "node1"; }
    public class ServerOptions { public TlsOptions TlsOptions { get; set; } }
    public class TlsOptions { public TlsClientOptions TlsClientOptions { get; set; } }
    public class TlsClientOptions { }
    public class SingleWriterMultiReaderLock { public virtual void WriteLock() { } public virtual void WriteUnlock() { } public virtual void ReadLock() { } public virtual void ReadUnlock() { } }
    public class ReplicaSyncSession { public virtual string replicaNodeId => null; }
    public class AofSyncTaskInfo : IDisposable { public string remoteNodeId => null; public GarnetClient garnetClient => null; public long previousAddress => 0; public AofSyncTask AofSyncTask { get; set; } public void Dispose() { } }
    public class GarnetClient { public bool IsConnected => true; }
    public class AofSyncTask { }
    public class GarnetClientSession { public GarnetClientSession(IPEndPoint ep, Func<NetworkBufferSettings> bufferSettings, NetworkPool pool, TlsClientOptions tlsOptions = null, string username = null, string password = null, ILogger logger = null) { } }
    public class NetworkBufferSettings { }
    public class NetworkPool { }
}
