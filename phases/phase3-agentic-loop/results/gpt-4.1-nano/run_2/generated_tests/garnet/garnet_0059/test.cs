using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_Should_LogCritical_On_Exception()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var replicationManagerMock = new Mock<ReplicationManager>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var currentConfig = new ClusterConfig();

            // Setup clusterProvider to return currentConfig
            clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(currentConfig);
            // Setup clusterProvider to return clusterManager
            clusterProviderMock.Setup(cp => cp.clusterManager).Returns(clusterManagerMock.Object);
            // Setup clusterProvider to return replicationManager
            clusterProviderMock.Setup(cp => cp.replicationManager).Returns(replicationManagerMock.Object);
            // Setup clusterProvider to return storeWrapper
            clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);

            // Create an instance of FailoverSession with mocked dependencies
            var failoverSession = new FailoverSession
            {
                logger = loggerMock.Object,
                clusterProvider = clusterProviderMock.Object,
                oldConfig = new OldConfig
                {
                    LocalNodePrimaryId = "primary",
                    LocalNodeId = "node1"
                },
                cts = new CancellationTokenSource(),
                failoverTimeout = TimeSpan.FromSeconds(5),
                epoch = 1,
                status = FailoverStatus.Idle
            };

            // Mock primaryClient to be null initially
            failoverSession.primaryClient = null;

            // Create a mock GarnetClient that throws on GossipAsync
            var mockClient = new Mock<GarnetClient>();
            mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                .Returns<byte[]>(async _ =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("Gossip failed");
                });

            // Setup GetConnectionAsync to return the mock client
            // Since GetConnectionAsync is private, we can simulate the call by setting primaryClient directly
            failoverSession.primaryClient = mockClient.Object;

            // Prepare configByteArray
            var configByteArray = new byte[] { 1, 2, 3 };

            // Act
            await failoverSession.BroadcastConfigAndRequestAttachAsync("some-replica", configByteArray);

            // Assert
            // Verify that LogCritical was called with the exception
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IssueAttachReplicas faulted")),
                    It.Is<Exception>(ex => ex.Message == "Gossip failed"),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Dummy classes to satisfy dependencies
    public class OldConfig
    {
        public string LocalNodePrimaryId { get; set; }
        public string LocalNodeId { get; set; }
    }

    public class ClusterConfig
    {
        public string LocalNodeId => "node1";
        public string LocalNodeIp => "127.0.0.1";

        public static ClusterConfig FromByteArray(byte[] array) => new ClusterConfig();
        public bool IsKnown(string nodeId) => true;
    }

    public class ClusterProvider
    {
        public ClusterManager clusterManager { get; set; }
        public ReplicationManager replicationManager { get; set; }
        public StoreWrapper storeWrapper { get; set; }
        public ServerOptions serverOptions { get; set; } = new ServerOptions();
        public string ClusterUsername { get; set; } = "user";
        public string ClusterPassword { get; set; } = "pass";
    }

    public class ClusterManager
    {
        public ClusterConfig CurrentConfig => new ClusterConfig();
        public bool TryTakeOverForPrimary() => true;
    }

    public class ReplicationManager
    {
        public bool BeginRecovery(RecoveryStatus status, bool upgradeLock) => true;
        public void TryUpdateForFailover() { }
        public void ResetReplayIterator() { }
        public bool InitializeCheckpointStore() => true;
        public void EndRecovery(RecoveryStatus status, bool downgradeLock) { }
    }

    public class StoreWrapper
    {
        public void StartPrimaryTasks() { }
    }

    public class ServerOptions
    {
        public TlsOptions TlsOptions { get; set; } = new TlsOptions();
    }

    public class TlsOptions
    {
        public object TlsClientOptions { get; set; }
    }

    public enum RecoveryStatus
    {
        ClusterFailover,
        NoRecovery
    }

    public enum FailoverStatus
    {
        Idle,
        ISSUING_PAUSE_WRITES,
        WAITING_FOR_SYNC,
        TAKING_OVER_AS_PRIMARY
    }
}
