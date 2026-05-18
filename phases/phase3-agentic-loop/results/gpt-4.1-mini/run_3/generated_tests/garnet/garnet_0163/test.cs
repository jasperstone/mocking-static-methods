using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class ReplicaSyncSessionTests
    {
        // Helper subclass to expose protected/internal members and inject dependencies
        private class TestReplicaSyncSession : ReplicaSyncSession
        {
            public TestReplicaSyncSession(ILogger logger)
            {
                this.logger = logger;
                this.ssInfo = new SyncStatusInfo();
                this.signalCompletion = new SemaphoreSlim(0, 1);
                this.token = CancellationToken.None;
                this.clusterProvider = new TestClusterProvider();
                this.replicaSyncMetadata = new TestReplicaSyncMetadata();
                this.storeWrapper = new TestStoreWrapper();
            }

            public new ILogger logger;
            public new SyncStatusInfo ssInfo;
            public new SemaphoreSlim signalCompletion;
            public new CancellationToken token;
            public new TestClusterProvider clusterProvider;
            public new TestReplicaSyncMetadata replicaSyncMetadata;
            public new TestStoreWrapper storeWrapper;

            public void SetFlushTask(Task<string> task)
            {
                base.SetFlushTask(task);
            }

            public Task WaitForFlushAsyncPublic() => base.WaitForFlushAsync();

            public Task WaitForSyncCompletionAsyncPublic() => base.WaitForSyncCompletionAsync();

            public void SetStatusPublic(SyncStatus status, string error = null) => base.SetStatus(status, error);
        }

        // Minimal mocks for dependencies to allow compilation and test
        private class TestClusterProvider
        {
            public TestReplicationManager replicationManager = new TestReplicationManager();
            public TestStoreWrapper storeWrapper = new TestStoreWrapper();
            public TestServerOptions serverOptions = new TestServerOptions();
            public TestClusterManager clusterManager = new TestClusterManager();
        }

        private class TestReplicationManager
        {
            public string PrimaryReplId => "primaryReplId";
        }

        private class TestStoreWrapper
        {
            public TestAppendOnlyFile appendOnlyFile = new TestAppendOnlyFile();
            public int loggingFrequency = 1;
            public TestServerOptions serverOptions = new TestServerOptions();
        }

        private class TestAppendOnlyFile
        {
            public long BeginAddress => 0;
            public long TailAddress => 100;
        }

        private class TestServerOptions
        {
            public TimeSpan ReplicaSyncTimeout => TimeSpan.FromSeconds(1);
            public long ReplicaDisklessSyncFullSyncAofThresholdValue() => 50;
        }

        private class TestClusterManager
        {
            public TestClusterConfig CurrentConfig = new TestClusterConfig();
        }

        private class TestClusterConfig
        {
            public string LocalNodeId => "localNodeId";
        }

        private class TestReplicaSyncMetadata
        {
            public string currentPrimaryReplId = "primaryReplId";
            public long currentStoreVersion = 1;
            public long currentObjectStoreVersion = 1;
            public long currentAofTailAddress = 50;
        }

        private class SyncStatusInfo
        {
            public SyncStatus syncStatus;
            public string error;
        }

        private enum SyncStatus
        {
            SUCCESS,
            FAILED,
            INPROGRESS
        }

        [Fact]
        public async Task WaitForFlushAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var session = new TestReplicaSyncSession(loggerMock.Object);

            // Setup flushTask to throw on await
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetException(new InvalidOperationException("flush failed"));
            session.flushTask = tcs.Task;

            // Act
            await session.WaitForFlushAsyncPublic();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "{method}",
                    nameof(session.WaitForFlushAsyncPublic).Replace("Public", "")),
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

            // Setup signalCompletion to throw on WaitAsync
            var semaphoreMock = new Mock<SemaphoreSlim>(0, 1);
            semaphoreMock.Setup(s => s.WaitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("wait failed"));
            session.signalCompletion = semaphoreMock.Object;

            // Act
            await session.WaitForSyncCompletionAsyncPublic();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "{method} failed waiting for sync",
                    nameof(session.WaitForSyncCompletionAsyncPublic).Replace("Public", "")),
                Times.Once);

            Assert.Equal(SyncStatus.FAILED, session.ssInfo.syncStatus);
            Assert.Equal("Wait for sync task faulted", session.ssInfo.error);
        }
    }
}
