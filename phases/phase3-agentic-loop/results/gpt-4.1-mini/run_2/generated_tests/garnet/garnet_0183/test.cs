using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsInformationAtLine134()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<IClusterManager>();
            var mockCurrentConfig = new Mock<ICurrentConfig>();
            var mockReplicationManager = new Mock<IReplicationManager>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockServerOptions = new Mock<IServerOptions>();

            string replicaNodeId = "replica1";

            // Setup clusterProvider and related mocks
            mockClusterProvider.SetupGet(cp => cp.clusterManager).Returns(mockClusterManager.Object);
            mockClusterProvider.SetupGet(cp => cp.replicationManager).Returns(mockReplicationManager.Object);
            mockClusterProvider.SetupGet(cp => cp.serverOptions).Returns(mockServerOptions.Object);
            mockClusterProvider.SetupGet(cp => cp.ClusterUsername).Returns("user");
            mockClusterProvider.SetupGet(cp => cp.ClusterPassword).Returns("pass");

            mockClusterManager.SetupGet(cm => cm.CurrentConfig).Returns(mockCurrentConfig.Object);
            mockCurrentConfig.Setup(c => c.GetWorkerAddressFromNodeId(replicaNodeId)).Returns(("127.0.0.1", 12345));

            // Setup StoreWrapper and ServerOptions
            var serverOptions = new ServerOptionsStub();
            mockStoreWrapper.SetupGet(sw => sw.serverOptions).Returns(serverOptions);

            // Setup replicaCheckpointEntry with metadata
            var metadata = new CheckpointMetadataStub
            {
                storeVersion = 1,
                objectStoreVersion = 2
            };
            var replicaCheckpointEntry = new CheckpointEntryStub
            {
                metadata = metadata
            };

            // Create the ReplicaSyncSession instance
            var session = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                replicaCheckpointEntry: replicaCheckpointEntry,
                replicaNodeId: replicaNodeId,
                logger: mockLogger.Object);

            // Setup AcquireCheckpointEntryAsync to return a dummy localEntry and null AofSyncTaskInfo
            session.OverrideAcquireCheckpointEntryAsync(() =>
                Task.FromResult((new CheckpointEntryStub { metadata = metadata }, (AofSyncTaskInfo)null)));

            // Setup ValidateMetadata to return true immediately
            session.OverrideValidateMetadata((localEntry, out long index_size, out LogFileInfo hlog_size, out long obj_index_size, out LogFileInfo obj_hlog_size, out bool skipLocalMainStoreCheckpoint, out bool skipLocalObjectStoreCheckpoint) =>
            {
                index_size = 0;
                hlog_size = default;
                obj_index_size = 0;
                obj_hlog_size = default;
                skipLocalMainStoreCheckpoint = true;
                skipLocalObjectStoreCheckpoint = true;
                return true;
            });

            // Setup GarnetClientSession.ConnectAsync to complete immediately
            session.OverrideGarnetClientSessionConnectAsync(() => Task.CompletedTask);

            // Act
            await session.SendCheckpointAsync();

            // Assert
            // Verify that LogInformation was called with the message "Checkpoint search completed"
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Checkpoint search completed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Stubs and helpers for the test

        private class ServerOptionsStub : IServerOptions
        {
            public TimeSpan ReplicaSyncTimeout => TimeSpan.FromSeconds(1);
            public ITlsOptions TlsOptions => null;
            public bool EnableStorageTier => false;
            public bool DisableObjects => false;
        }

        private class CheckpointMetadataStub
        {
            public int storeVersion { get; set; }
            public int objectStoreVersion { get; set; }
            public string storePrimaryReplId { get; set; } = "primary1";
            public string objectStorePrimaryReplId { get; set; } = "primary1";
            public object storeHlogToken { get; set; } = new object();
            public object storeIndexToken { get; set; } = new object();
            public object objectStoreHlogToken { get; set; } = new object();
            public object objectStoreIndexToken { get; set; } = new object();
        }

        private class CheckpointEntryStub
        {
            public CheckpointMetadataStub metadata { get; set; }
        }

        // Extension methods to override internal methods for testing
        // These would require the actual class to be partial and have virtual or protected methods or internal hooks
        // For the sake of this test, assume these methods exist or use reflection or other means in real tests

    }
}
