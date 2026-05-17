using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.cluster
{
    public class ReplicaSyncSessionTests
    {
        private class TestReplicaSyncSession : ReplicaSyncSession
        {
            public TestReplicaSyncSession(ILogger logger)
            {
                this.logger = logger;
                this.ssInfo = new SyncStatusInfo();
                this.signalCompletion = new SemaphoreSlim(0, 1);
                this.token = CancellationToken.None;
                this.clusterProvider = new ClusterProviderStub();
                this.replicaSyncMetadata = new ReplicaSyncMetadataStub();
                this.storeWrapper = new StoreWrapperStub();
            }

            public new ILogger logger;
            public new SyncStatusInfo ssInfo;
            public new SemaphoreSlim signalCompletion;
            public new CancellationToken token;
            public new ClusterProviderStub clusterProvider;
            public new ReplicaSyncMetadataStub replicaSyncMetadata;
            public new StoreWrapperStub storeWrapper;

            public void SetFlushTask(Task<bool> task)
            {
                flushTask = task;
            }
        }

        private class ClusterProviderStub
        {
            public ReplicationManagerStub replicationManager = new ReplicationManagerStub();
            public StoreWrapperStub storeWrapper = new StoreWrapperStub();
            public ServerOptionsStub serverOptions = new ServerOptionsStub();
            public ClusterManagerStub clusterManager = new ClusterManagerStub();
        }

        private class ReplicationManagerStub
        {
            public string PrimaryReplId = "primary";
        }

        private class StoreWrapperStub
        {
            public AppendOnlyFileStub appendOnlyFile = new AppendOnlyFileStub();
            public int loggingFrequency = 1;
            public ServerOptionsStub serverOptions = new ServerOptionsStub();
        }

        private class AppendOnlyFileStub
        {
            public long BeginAddress = 0;
            public long TailAddress = 100;
        }

        private class ServerOptionsStub
        {
            public TimeSpan ReplicaSyncTimeout => TimeSpan.FromSeconds(1);
            public long ReplicaDisklessSyncFullSyncAofThresholdValue() => 50;
        }

        private class ClusterManagerStub
        {
            public ClusterConfigStub CurrentConfig = new ClusterConfigStub();
        }

        private class ClusterConfigStub
        {
            public string LocalNodeId = "localNode";
        }

        private class ReplicaSyncMetadataStub
        {
            public string currentPrimaryReplId = "primary";
            public long currentStoreVersion = 1;
            public long currentObjectStoreVersion = 1;
            public long currentAofTailAddress = 50;
        }

        [Fact]
        public async Task WaitForFlushAsync_LogsErrorOnFlushTaskException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var session = new TestReplicaSyncSession(loggerMock.Object);

            var tcs = new TaskCompletionSource<bool>();
            tcs.SetException(new InvalidOperationException("flush failed"));
            session.SetFlushTask(tcs.Task);

            // Act
            await session.WaitForFlushAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "{method}",
                    nameof(session.WaitForFlushAsync)),
                Times.Once);

            Assert.Equal(SyncStatus.FAILED, session.ssInfo.syncStatus);
            Assert.Equal("Flush task faulted", session.ssInfo.error);
        }

        [Fact]
        public async Task WaitForSyncCompletionAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var session = new TestReplicaSyncSession(loggerMock.Object);

            var cts = new CancellationTokenSource();
            session.token = cts.Token;
            cts.Cancel();

            // Act
            await session.WaitForSyncCompletionAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "{method} failed waiting for sync",
                    nameof(session.WaitForSyncCompletionAsync)),
                Times.Once);

            Assert.Equal(SyncStatus.FAILED, session.ssInfo.syncStatus);
            Assert.Equal("Wait for sync task faulted", session.ssInfo.error);
        }
    }
}
