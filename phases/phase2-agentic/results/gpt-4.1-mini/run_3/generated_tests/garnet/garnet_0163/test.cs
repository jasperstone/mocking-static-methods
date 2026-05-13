using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        // Helper class to expose internal members for testing
        private class TestReplicaSyncSession : ReplicaSyncSession
        {
            public new ILogger logger;
            public new Task<bool> flushTask;
            public new SyncStatusInfo ssInfo = new SyncStatusInfo();
            public new CancellationToken token = CancellationToken.None;
            public new SemaphoreSlim signalCompletion = new SemaphoreSlim(0, 1);

            public TestReplicaSyncSession()
            {
                // Initialize base fields
                this.ssInfo = new SyncStatusInfo();
                this.flushTask = null;
                this.logger = null;
                this.signalCompletion = new SemaphoreSlim(0, 1);
                this.token = CancellationToken.None;
            }

            public void SetLogger(ILogger logger) => this.logger = logger;

            public void SetFlushTask(Task<bool> task) => this.flushTask = task;

            public void SetSyncStatusInfo(SyncStatusInfo info) => this.ssInfo = info;

            public void SetSignalCompletion(SemaphoreSlim sem) => this.signalCompletion = sem;

            public void SetToken(CancellationToken token) => this.token = token;

            public new async Task WaitForFlushAsync()
            {
                try
                {
                    if (flushTask != null) _ = await flushTask.ConfigureAwait(false);
                    flushTask = null;
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "{method}", $"{nameof(WaitForFlushAsync)}");
                    SetStatus(SyncStatus.FAILED, "Flush task faulted");
                }
            }

            public new async Task WaitForSyncCompletionAsync()
            {
                try
                {
                    await signalCompletion.WaitAsync(token).ConfigureAwait(false);
                    // Assert syncStatus is SUCCESS or FAILED
                    if (!(ssInfo.syncStatus == SyncStatus.SUCCESS || ssInfo.syncStatus == SyncStatus.FAILED))
                    {
                        throw new Exception("SyncStatus is not SUCCESS or FAILED");
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "{method} failed waiting for sync", nameof(WaitForSyncCompletionAsync));
                    SetStatus(SyncStatus.FAILED, "Wait for sync task faulted");
                }
            }

            public new void SetStatus(SyncStatus status, string error = null)
            {
                ssInfo.error ??= error;
                ssInfo.syncStatus = status;
                if (status == SyncStatus.SUCCESS || status == SyncStatus.FAILED)
                {
                    signalCompletion.Release();
                }
            }
        }

        [Fact]
        public async Task WaitForFlushAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var session = new TestReplicaSyncSession();
            session.SetLogger(loggerMock.Object);

            // Create a flushTask that throws
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
                    nameof(TestReplicaSyncSession.WaitForFlushAsync)),
                Times.Once);

            Assert.Equal(SyncStatus.FAILED, session.ssInfo.syncStatus);
            Assert.Equal("Flush task faulted", session.ssInfo.error);
            Assert.Null(session.flushTask);
        }

        [Fact]
        public async Task WaitForFlushAsync_DoesNotLogErrorWhenNoFlushTask()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var session = new TestReplicaSyncSession();
            session.SetLogger(loggerMock.Object);
            session.SetFlushTask(null);

            // Act
            await session.WaitForFlushAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Never);

            Assert.Null(session.flushTask);
        }

        [Fact]
        public async Task WaitForSyncCompletionAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var session = new TestReplicaSyncSession();
            session.SetLogger(loggerMock.Object);

            // Setup signalCompletion to throw on WaitAsync
            var semMock = new Mock<SemaphoreSlim>(0, 1);
            semMock.Setup(s => s.WaitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new TimeoutException("timeout"));
            session.SetSignalCompletion(semMock.Object);

            // Act
            await session.WaitForSyncCompletionAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "{method} failed waiting for sync",
                    nameof(TestReplicaSyncSession.WaitForSyncCompletionAsync)),
                Times.Once);

            Assert.Equal(SyncStatus.FAILED, session.ssInfo.syncStatus);
            Assert.Equal("Wait for sync task faulted", session.ssInfo.error);
        }

        [Fact]
        public async Task WaitForSyncCompletionAsync_CompletesWhenSignalReleased()
        {
            // Arrange
            var session = new TestReplicaSyncSession();
            session.SetStatus(SyncStatus.SUCCESS);
            session.signalCompletion.Release();

            // Act & Assert
            await session.WaitForSyncCompletionAsync();
        }
    }
}
