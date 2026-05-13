using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionTests
    {
        private class DummyLogger : ILogger
        {
            public string LastLogMessage { get; private set; }
            public Exception LastException { get; private set; }
            public LogLevel LastLogLevel { get; private set; }
            public string LastEventId { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogMessage = formatter(state, exception);
                LastException = exception;
                LastLogLevel = logLevel;
                LastEventId = eventId.ToString();
            }
        }

        private class DummyGarnetClient
        {
            public bool IsConnected { get; set; } = false;
            public string ExecuteAsync(params string[] commands) => "OK";
            public void InitializeIterationBuffer(int frequency) { }
            public bool NeedsInitialization => true;
            public void SetClusterSyncHeader(string nodeId, bool isMainStore) { }
            public bool TryWriteKeyValueSpanByte(ref SpanByte key, ref SpanByte value, out Task<string> task)
            {
                task = Task.FromResult("OK");
                return true;
            }
            public bool TryWriteKeyValueByteArray(byte[] key, byte[] value, long expiration, out Task<string> task)
            {
                task = Task.FromResult("OK");
                return true;
            }
            public Task<string> SendAndResetIterationBuffer() => Task.FromResult("OK");
            public void Connect() { IsConnected = true; }
        }

        private class DummyAofSyncTaskInfo
        {
            public DummyGarnetClient garnetClient = new DummyGarnetClient();
            public bool IsConnected => garnetClient.IsConnected;
        }

        [Fact]
        public async Task WaitForFlushAsync_ShouldLogErrorAndSetFailed_WhenFlushTaskThrows()
        {
            // Arrange
            var logger = new DummyLogger();
            var session = new ReplicaSyncSession();
            session.GetType().GetProperty("logger").SetValue(session, logger);
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetException(new InvalidOperationException("Test exception"));
            session.GetType().GetProperty("flushTask").SetValue(session, tcs.Task);

            // Act
            await session.WaitForFlushAsync();

            // Assert
            Assert.Equal(SyncStatus.FAILED, session.GetSyncStatusInfo.syncStatus);
            Assert.Contains("Flush task faulted", session.GetSyncStatusInfo.error);
            Assert.Contains("Test exception", logger.LastLogMessage);
        }

        [Fact]
        public async Task WaitForSyncCompletionAsync_ShouldLogErrorAndSetFailed_WhenExceptionThrown()
        {
            // Arrange
            var logger = new DummyLogger();
            var session = new ReplicaSyncSession();
            session.GetType().GetProperty("logger").SetValue(session, logger);
            var cts = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<bool>();
            session.GetType().GetProperty("signalCompletion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(session, tcs);
            session.GetType().GetProperty("token", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(session, cts.Token);
            tcs.SetException(new InvalidOperationException("Test exception"));

            // Act
            await session.WaitForSyncCompletionAsync();

            // Assert
            Assert.Equal(SyncStatus.FAILED, session.GetSyncStatusInfo.syncStatus);
            Assert.Contains("Wait for sync task faulted", session.GetSyncStatusInfo.error);
            Assert.Contains("Test exception", logger.LastLogMessage);
        }

        [Fact]
        public void LogError_ShouldBeCalled_WhenSetFlushTaskReceivesNonOkResponse()
        {
            // Arrange
            var logger = new DummyLogger();
            var session = new ReplicaSyncSession();
            session.GetType().GetProperty("logger").SetValue(session, logger);
            var mockClient = new DummyGarnetClient();
            var aofSyncTask = new DummyAofSyncTaskInfo { garnetClient = mockClient };
            session.GetType().GetProperty("AofSyncTask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(session, aofSyncTask);
            var responseTask = Task.FromResult("ErrorResponse");

            // Act
            session.SetFlushTask(responseTask);

            // Wait briefly for async continuation
            Task.Delay(50).Wait();

            // Assert
            Assert.Contains("ReplicaSyncSession: {errorMsg}", logger.LastLogMessage);
            Assert.Equal(SyncStatus.FAILED, session.GetSyncStatusInfo.syncStatus);
            Assert.Contains("ErrorResponse", session.GetSyncStatusInfo.error);
        }
    }
}
