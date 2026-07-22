using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace ReplicaSyncSessionTests
{
    public class ReplicaSyncSessionTest
    {
        private class DummyLogger : ILogger<ReplicaSyncSession>
        {
            public string LastLogMessage { get; private set; }
            public LogLevel LastLogLevel { get; private set; }
            public Exception LastException { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogMessage = formatter(state, exception);
                LastLogLevel = logLevel;
                LastException = exception;
            }
        }

        [Fact]
        public async Task WaitForFlushAsync_CatchesException_LogsErrorAndSetsFailed()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
            var session = new ReplicaSyncSession
            {
                logger = mockLogger.Object,
                flushTask = Task.FromException(new InvalidOperationException("Test exception"))
            };

            // Act
            await session.WaitForFlushAsync();

            // Assert
            mockLogger.Verify(l => l.LogError(It.IsAny<Exception>(), "{method}", "WaitForFlushAsync"), Times.Once);
            Assert.Equal(SyncStatus.FAILED, session.GetSyncStatusInfo.syncStatus);
        }

        [Fact]
        public async Task WaitForSyncCompletionAsync_WaitsAndHandlesException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
            var semaphore = new SemaphoreSlim(0);
            var session = new ReplicaSyncSession
            {
                logger = mockLogger.Object,
                signalCompletion = semaphore,
                ssInfo = new SyncStatusInfo { syncStatus = SyncStatus.INPROGRESS }
            };
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            // Act
            var task = session.WaitForSyncCompletionAsync();

            // Simulate cancellation to cause exception
            cts.Cancel();

            await task;

            // Assert
            mockLogger.Verify(l => l.LogError(It.IsAny<Exception>(), "{method} failed waiting for sync", "WaitForSyncCompletionAsync"), Times.Once);
            Assert.Equal(SyncStatus.FAILED, session.GetSyncStatusInfo.syncStatus);
        }
    }
}
