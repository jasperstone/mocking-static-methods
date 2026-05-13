using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.server;
using Garnet.common;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        private Mock<ILogger> _loggerMock;
        private Mock<ClusterProvider> _clusterProviderMock;
        private Mock<StoreWrapper> _storeWrapperMock;
        private Mock<ReplicationManager> _replicationManagerMock;
        private Mock<ClusterManager> _clusterManagerMock;
        private Mock<ReplicationLogCheckpointManager> _checkpointManagerMock;

        public ReplicaSyncSessionTests()
        {
            _loggerMock = new Mock<ILogger>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _checkpointManagerMock = new Mock<ReplicationLogCheckpointManager>();

            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(_checkpointManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new ClusterConfig());
        }

        [Fact]
        public async Task SendCheckpointAsync_Should_LogInformation_And_ReturnTrue_When_Successful()
        {
            // Arrange
            var session = new ReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object);

            // Setup mocks
            var localEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storePrimaryReplId = "id",
                    objectStorePrimaryReplId = "id"
                }
            };

            var acquireCheckpointCalled = false;
            session.GetType().GetMethod("AcquireCheckpointEntryAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .CreateDelegate<Func<Task<(CheckpointEntry, AofSyncTaskInfo)>>>(session)
                = async () =>
                {
                    acquireCheckpointCalled = true;
                    return (localEntry, null);
                };

            _checkpointManagerMock.Setup(m => m.TryGetLatestCheckpointEntryFromMemory(out It.Ref<CheckpointEntry>.IsAny))
                .Returns(true);

            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig)
                .Returns(new ClusterConfig());

            // Act
            var result = await session.SendCheckpointAsync();

            // Assert
            Assert.True(result);
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SendCheckpointAsync_Should_LogError_And_ReturnFalse_When_AddressIsNull()
        {
            // Arrange
            var session = new ReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object);

            // Setup mock to return null address
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig)
                .Returns(new ClusterConfig());
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns((null, -1));

            // Act
            var result = await session.SendCheckpointAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task SendCheckpointAsync_Should_LogInformation_When_LogInformationCalled()
        {
            // Arrange
            var session = new ReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object);

            // Setup mocks
            var localEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storeVersion = 1,
                    objectStoreVersion = 1,
                    storePrimaryReplId = "id",
                    objectStorePrimaryReplId = "id"
                }
            };

            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig)
                .Returns(new ClusterConfig());

            // Setup AcquireCheckpointEntryAsync to return localEntry
            var methodInfo = typeof(ReplicaSyncSession).GetMethod("AcquireCheckpointEntryAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var del = (Func<Task<(CheckpointEntry, AofSyncTaskInfo)>>)Delegate.CreateDelegate(typeof(Func<Task<(CheckpointEntry, AofSyncTaskInfo)>>), session, methodInfo);
            var mockMethod = new Func<Task<(CheckpointEntry, AofSyncTaskInfo)>>(() => Task.FromResult((localEntry, null)));
            methodInfo.CreateDelegate(typeof(Func<Task<(CheckpointEntry, AofSyncTaskInfo)>>), session) = mockMethod;

            // Act
            await session.SendCheckpointAsync();

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("requesting checkpoint")), It.IsAny<object[]>()), Times.Once);
        }
    }

    // Dummy classes to compile the test
    public class CheckpointEntry
    {
        public CheckpointMetadata metadata;
    }

    public class CheckpointMetadata
    {
        public int storeVersion;
        public int objectStoreVersion;
        public string storePrimaryReplId;
        public string objectStorePrimaryReplId;
    }

    public class LogFileInfo { }

    public class AofSyncTaskInfo { }

    public class ClusterConfig
    {
        public (string, int) GetWorkerAddressFromNodeId(string nodeId) => ("127.0.0.1", 1234);
    }

    public enum StoreType { Main, Object }

    public class GarnetException : Exception
    {
        public GarnetException(string message) : base(message) { }
    }
}
