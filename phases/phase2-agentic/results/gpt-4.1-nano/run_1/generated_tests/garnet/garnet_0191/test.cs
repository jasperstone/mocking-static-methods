using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
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
        private Mock<GarnetClientSession> _gcsMock;

        public ReplicaSyncSessionTests()
        {
            _loggerMock = new Mock<ILogger>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _storeWrapperMock = new Mock<StoreWrapper>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _checkpointManagerMock = new Mock<ReplicationLogCheckpointManager>();
            _gcsMock = new Mock<GarnetClientSession>();

            // Setup default behaviors
            _clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(_checkpointManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.ClusterUsername).Returns("user");
            _clusterProviderMock.Setup(cp => cp.ClusterPassword).Returns("pass");
            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions
            {
                TlsOptions = null,
                ReplicaSyncTimeout = TimeSpan.FromSeconds(10),
                UseAofNullDevice = false,
                FastAofTruncate = false,
                OnDemandCheckpoint = false
            });
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new ClusterConfig());
            _clusterProviderMock.Setup(cp => cp.GetNetworkPool()).Returns(new object());
            _clusterProviderMock.Setup(cp => cp.GetRSSNetworkBufferSettings).Returns(new object());
        }

        [Fact]
        public async Task LogError_IsCalled_When_LogErrorCalledInCode()
        {
            // Arrange
            var session = new ReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                logger: _loggerMock.Object);

            // Setup localEntry with dummy data
            var localEntry = new CheckpointEntry
            {
                metadata = new CheckpointMetadata
                {
                    storePrimaryReplId = "id",
                    storeVersion = 1,
                    objectStorePrimaryReplId = "objId",
                    objectStoreVersion = 1
                }
            };

            // Setup the ValidateMetadata to return false to trigger LogError
            bool validateResult = false;
            var validateMethod = typeof(ReplicaSyncSession).GetMethod("ValidateMetadata");
            // Use reflection to invoke private method
            var parameters = new object[] { localEntry, null, null, null, null, out bool skipMain, out bool skipObject };
            // We will call SendCheckpointAsync and simulate the part where LogError is called
            // For that, we need to mock AcquireCheckpointEntryAsync to return localEntry
            // and simulate the code path that leads to LogError

            // Since the code is complex, we will focus on the part that calls LogError
            // We can invoke the method that contains the LogError call directly if accessible
            // But since it's inside SendCheckpointAsync, we can test that method

            // For simplicity, we will test that when ValidateMetadata returns false, LogError is called

            // Act
            // We need to call SendCheckpointAsync and force ValidateMetadata to return false
            // To do this, we can create a derived class that overrides ValidateMetadata to return false
            var testSession = new TestReplicaSyncSession(
                _storeWrapperMock.Object,
                _clusterProviderMock.Object,
                _loggerMock.Object,
                validateMetadataReturns: false);

            await testSession.SendCheckpointAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to validate metadata")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        // Helper class to override ValidateMetadata for testing
        private class TestReplicaSyncSession : ReplicaSyncSession
        {
            private readonly bool _validateMetadataReturns;

            public TestReplicaSyncSession(
                StoreWrapper storeWrapper,
                ClusterProvider clusterProvider,
                ILogger logger,
                bool validateMetadataReturns)
                : base(storeWrapper, clusterProvider, logger: logger)
            {
                _validateMetadataReturns = validateMetadataReturns;
            }

            public override bool ValidateMetadata(
                CheckpointEntry localEntry,
                out long index_size,
                out LogFileInfo hlog_size,
                out long obj_index_size,
                out LogFileInfo obj_hlog_size,
                out bool skipLocalMainStoreCheckpoint,
                out bool skipLocalObjectStoreCheckpoint)
            {
                index_size = 0;
                hlog_size = default;
                obj_index_size = 0;
                obj_hlog_size = default;
                skipLocalMainStoreCheckpoint = false;
                skipLocalObjectStoreCheckpoint = false;
                return _validateMetadataReturns;
            }
        }
    }
}
