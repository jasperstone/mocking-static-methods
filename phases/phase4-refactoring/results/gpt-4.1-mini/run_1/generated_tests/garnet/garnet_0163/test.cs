using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionLoggerTests
    {
        private class TestReplicaSyncSession
        {
            public ILogger logger;
            public SyncStatusInfo ssInfo;
            public SemaphoreSlim signalCompletion;
            public CancellationToken token;
            public Task<bool> flushTask;

            public TestReplicaSyncSession(ILogger logger)
            {
                this.logger = logger;
                ssInfo = new SyncStatusInfo();
                signalCompletion = new SemaphoreSlim(0, 1);
                token = CancellationToken.None;
            }

            public void SetStatus(SyncStatus status, string error = null)
            {
                if (ssInfo.error == null)
                    ssInfo.error = error;
                ssInfo.syncStatus = status;
                if (status == SyncStatus.SUCCESS || status == SyncStatus.FAILED)
                    signalCompletion.Release();
            }

            public async Task WaitForFlushAsync()
            {
                try
                {
                    if (flushTask != null) _ = await flushTask.ConfigureAwait(false);
                    flushTask = null;
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "{method}", nameof(WaitForFlushAsync));
                    SetStatus(SyncStatus.FAILED, "Flush task faulted");
                }
            }

            public async Task WaitForSyncCompletionAsync()
            {
                try
                {
                    await signalCompletion.WaitAsync(token).ConfigureAwait(false);
                    if (!(ssInfo.syncStatus == SyncStatus.SUCCESS || ssInfo.syncStatus == SyncStatus.FAILED))
                    {
                        throw new Exception("Invalid sync status");
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "{method} failed waiting for sync", nameof(WaitForSyncCompletionAsync));
                    SetStatus(SyncStatus.FAILED, "Wait for sync task faulted");
                }
            }
        }

        // We cannot access SyncStatus and SyncStatusInfo directly due to protection level,
        // so we define minimal copies here for testing purposes.
        private enum SyncStatus : byte
        {
            SUCCESS,
            FAILED,
            INPROGRESS,
            INITIALIZING
        }

        private struct SyncStatusInfo
        {
            public SyncStatus syncStatus;
            public string error;
        }

        [Fact]
        public async Task WaitForFlushAsync_LogsErrorOnException()
        {
            var loggerMock = new Mock<ILogger>();
            var session = new TestReplicaSyncSession(loggerMock.Object);

            session.flushTask = Task.FromException<bool>(new InvalidOperationException("flush failed"));

            await session.WaitForFlushAsync();

            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "{method}",
                    nameof(TestReplicaSyncSession.WaitForFlushAsync)),
                Times.Once);
        }

        [Fact]
        public async Task WaitForSyncCompletionAsync_LogsErrorOnException()
        {
            var loggerMock = new Mock<ILogger>();
            var session = new TestReplicaSyncSession(loggerMock.Object);

            var cts = new CancellationTokenSource();
            session.token = cts.Token;

            var waitTask = session.WaitForSyncCompletionAsync();

            cts.Cancel();

            await waitTask;

            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "{method} failed waiting for sync",
                    nameof(TestReplicaSyncSession.WaitForSyncCompletionAsync)),
                Times.Once);
        }
    }
}
