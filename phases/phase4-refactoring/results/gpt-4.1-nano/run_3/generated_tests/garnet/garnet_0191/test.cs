using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.server;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsError_WhenSyncFromAofAddressLessThanBeginAddressAndPossibleAofDataLossIsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicaSyncSession>>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var appendOnlyFileMock = new Mock<AppendOnlyFile>();
            var serverOptions = new ServerOptions
            {
                ReplicaSyncTimeout = TimeSpan.FromSeconds(30),
                UseAofNullDevice = false,
                FastAofTruncate = false,
                OnDemandCheckpoint = false
            };

            // Setup storeWrapper and appendOnlyFile
            var beginAddress = 1000L;
            appendOnlyFileMock.Setup(a => a.BeginAddress).Returns(beginAddress);
            storeWrapperMock.Setup(s => s.appendOnlyFile).Returns(appendOnlyFileMock.Object);
            storeWrapperMock.Setup(s => s.serverOptions).Returns(serverOptions);

            // Setup clusterProvider
            var currentConfigMock = new Mock<ClusterConfig>();
            currentConfigMock.Setup(c => c.GetWorkerAddressFromNodeId(It.IsAny<string>()))
                .Returns(("127.0.0.1", 12345));
            var clusterManagerMock = new Mock<ClusterManager>();
            clusterManagerMock.Setup(c => c.CurrentConfig).Returns(currentConfigMock.Object);

            var replicationManagerMock = new Mock<ReplicationManager>();
            var clusterProvider = new Mock<ClusterProvider>();
            clusterProvider.Setup(c => c.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(Mock.Of<ReplicationLogCheckpointManager>());
            clusterProvider.Setup(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProvider.Setup(c => c.replicationManager).Returns(replicationManagerMock.Object);
            clusterProvider.Setup(c => c.serverOptions).Returns(serverOptions);
            clusterProvider.Setup(c => c.ClusterUsername).Returns("user");
            clusterProvider.Setup(c => c.ClusterPassword).Returns("pass");
            clusterProvider.Setup(c => c.GetNetworkPool).Returns(new object());
            clusterProvider.Setup(c => c.GetRSSNetworkBufferSettings).Returns(new object());

            // Instantiate the session with the mocks
            var session = new TestReplicaSyncSession(
                storeWrapperMock.Object,
                clusterProvider.Object,
                loggerMock.Object)
            {
                // Set the addresses and data loss flag
                SyncFromAofAddress = 900L,
                SyncToAofAddress = 1000L,
                PossibleAofDataLoss = false
            };

            // Act
            await session.SendCheckpointAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.Is<string>(msg => msg.Contains("syncFromAofAddress"))),
                Times.Once);
        }

        // Derived class to override internal behavior
        private class TestReplicaSyncSession : ReplicaSyncSession
        {
            public long SyncFromAofAddress { get; set; }
            public long SyncToAofAddress { get; set; }
            public bool PossibleAofDataLoss { get; set; }

            public TestReplicaSyncSession(
                StoreWrapper storeWrapper,
                ClusterProvider clusterProvider,
                ILogger logger)
                : base(storeWrapper, clusterProvider, logger: logger)
            {
            }

            public override async Task<bool> SendCheckpointAsync()
            {
                // Set the addresses and data loss flag
                this.SyncFromAofAddress = 900;
                this.SyncToAofAddress = 1000;
                this.PossibleAofDataLoss = false;

                // Call the base method
                return await base.SendCheckpointAsync();
            }
        }
    }
}
