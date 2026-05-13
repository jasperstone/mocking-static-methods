using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_ShouldLogInformation_WhenCalled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockClusterManager = new Mock<ClusterManager>();
            var mockReplicationManager = new Mock<ReplicationManager>();
            var mockServerOptions = new Mock<ServerOptions>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockGarnetClientSession = new Mock<GarnetClientSession>();
            var mockGetReplicationLogCheckpointManager = new Mock<Func<StoreType, object>>();

            // Setup cluster provider to return mock checkpoint managers
            mockClusterProvider.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(() => new object());

            // Setup cluster provider to return current config with a valid address
            var currentConfig = new Mock<ClusterConfig>();
            currentConfig.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 1234));
            mockClusterProvider.Setup(cp => cp.clusterManager.CurrentConfig)
                .Returns(currentConfig.Object);

            // Setup cluster provider to return clusterManager
            mockClusterProvider.Setup(cp => cp.clusterManager)
                .Returns(mockClusterManager.Object);

            // Setup cluster provider to return replicationManager
            mockClusterProvider.Setup(cp => cp.replicationManager)
                .Returns(mockReplicationManager.Object);

            // Setup server options
            var mockTlsOptions = new Mock<TlsOptions>();
            var mockTlsClientOptions = new Mock<TlsClientOptions>();
            mockTlsOptions.Setup(t => t.TlsClientOptions).Returns(mockTlsClientOptions.Object);
            mockServerOptions.Setup(s => s.TlsOptions).Returns(mockTlsOptions.Object);
            mockServerOptions.Setup(s => s.EnableStorageTier).Returns(true);
            mockServerOptions.Setup(s => s.ReplicaSyncTimeout).Returns(TimeSpan.FromSeconds(10));
            mockClusterProvider.Setup(cp => cp.serverOptions).Returns(mockServerOptions.Object);

            // Setup cluster provider to return dummy checkpoint entries
            var dummyCheckpointEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storeHlogToken = "token",
                    storeIndexToken = "indexToken",
                    storePrimaryReplId = "id",
                    objectStorePrimaryReplId = "objId"
                }
            };

            // Setup AcquireCheckpointEntryAsync to return dummy checkpoint
            var dummyLocalEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storeHlogToken = "token",
                    storeIndexToken = "indexToken",
                    storePrimaryReplId = "id",
                    objectStorePrimaryReplId = "objId"
                }
            };

            var session = new ReplicaSyncSession(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                replicaCheckpointEntry: dummyCheckpointEntry,
                logger: mockLogger.Object);

            // Mock AcquireCheckpointEntryAsync to return dummy checkpoint
            var sessionType = typeof(ReplicaSyncSession);
            var method = sessionType.GetMethod("AcquireCheckpointEntryAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var taskCompletionSource = new TaskCompletionSource<(CheckpointEntry, AofSyncTaskInfo)>();
            taskCompletionSource.SetResult((dummyLocalEntry, null));
            var mockSession = new Mock<ReplicaSyncSession>(
                mockStoreWrapper.Object,
                mockClusterProvider.Object,
                replicaCheckpointEntry: dummyCheckpointEntry,
                logger: mockLogger.Object);
            mockSession.Setup(s => s.AcquireCheckpointEntryAsync()).Returns(taskCompletionSource.Task);

            // Act
            await mockSession.Object.SendCheckpointAsync();

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Replica replicaId")), It.IsAny<object[]>()),
                Times.AtLeastOnce);
        }
    }
}
