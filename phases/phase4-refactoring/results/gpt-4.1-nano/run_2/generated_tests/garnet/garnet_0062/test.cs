using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        private class DummyConfig
        {
            public string LocalNodeId => "localNode";
            public string LocalNodePrimaryId => "primaryId";
            public string GetEndpointFromNodeId(string nodeId) => "endpoint";
        }

        private class DummyClusterProvider
        {
            public class DummyReplicationManager
            {
                public bool BeginRecovery(RecoveryStatus status, bool upgradeLock) => true;
                public void TryUpdateForFailover() { }
                public void ResetReplayIterator() { }
                public bool InitializeCheckpointStore() => true;
                public void EndRecovery(RecoveryStatus status, bool downgradeLock) { }
                public long ReplicationOffset => 0;
            }

            public DummyReplicationManager replicationManager = new DummyReplicationManager();
            public DummyClusterManager clusterManager = new DummyClusterManager();
            public DummyStoreWrapper storeWrapper = new DummyStoreWrapper();
            public DummyClusterConfig CurrentConfig => new DummyClusterConfig();
        }

        private class DummyClusterManager
        {
            public bool TryTakeOverForPrimary() => true;
            public DummyClusterConfig CurrentConfig => new DummyClusterConfig();
        }

        private class DummyClusterConfig
        {
            public string LocalNodeId => "localNode";
            public string LocalNodePrimaryId => "primaryId";
        }

        private class DummyStoreWrapper
        {
            public void StartPrimaryTasks() { }
        }

        private class DummyClient : GarnetClient
        {
            public DummyClient() : base("endpoint", null, sendPageSize: 1, maxOutstandingTasks: 1, null, null, 0, null) { }
            public override Task ReconnectAsync() => Task.CompletedTask;
            public override Task<long> FailStopWritesAsync(byte[] localNodeIdBytes) => Task.FromResult(0L);
            public override Task<GossipResponse> GossipAsync(byte[] config) => Task.FromResult(new GossipResponse());
        }

        [Fact]
        public async Task LogWarning_IsCalled_OnExceptionDuringWaitAsync()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();
            var session = new FailoverSession
            {
                logger = loggerMock.Object,
                oldConfig = new DummyConfig(),
                clusterProvider = new DummyClusterProvider(),
                cts = new CancellationTokenSource(),
                failoverTimeout = TimeSpan.FromSeconds(1),
                status = FailoverStatus.WAITING_FOR_SYNC
            };

            // Mock GetConnectionAsync to return a client that throws on WaitAsync
            var mockClient = new Mock<GarnetClient>();
            mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                      .ReturnsAsync(new GossipResponse());
            mockClient.Setup(c => c.WaitAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new TimeoutException());

            session.primaryClient = mockClient.Object;

            // Act
            await session.BroadcastConfigAndRequestAttachAsync("replicaId", new byte[0]);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.IsAny<TimeoutException>(), "WaitingForAttachToComplete Error"),
                Times.Once);
        }
    }
}
