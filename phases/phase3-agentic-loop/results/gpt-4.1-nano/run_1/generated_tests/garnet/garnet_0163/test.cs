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
            public Exception LoggedException { get; private set; }
            public string LoggedMessage { get; private set; }
            public LogLevel? LastLogLevel { get; private set; }
            public string LastEventId { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogLevel = logLevel;
                LoggedException = exception;
                LoggedMessage = formatter(state, exception);
                LastEventId = eventId.ToString();
            }
        }

        [Fact]
        public async Task WaitForFlushAsync_LogsErrorAndSetsFailedOnException()
        {
            // Arrange
            var logger = new DummyLogger();
            var session = new ReplicaSyncSession();

            // Use reflection to set the private logger field
            var loggerField = typeof(ReplicaSyncSession).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(session, logger);

            // Create a TaskCompletionSource to simulate a flushTask that throws
            var tcs = new TaskCompletionSource<bool>();
            // Use reflection to set the private flushTask field
            var flushTaskField = typeof(ReplicaSyncSession).GetField("flushTask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            flushTaskField.SetValue(session, tcs.Task);

            // Act: set exception to simulate failure
            tcs.SetException(new InvalidOperationException("Test exception"));

            await session.WaitForFlushAsync();

            // Assert
            Assert.NotNull(logger.LoggedException);
            Assert.IsType<InvalidOperationException>(logger.LoggedException);
            Assert.Contains("Flush task faulted", logger.LoggedMessage);
            // Check that the status is set to FAILED
            var ssInfo = typeof(ReplicaSyncSession).GetField("ssInfo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(session);
            var syncStatus = ssInfo.GetType().GetProperty("syncStatus").GetValue(ssInfo);
            Assert.Equal(SyncStatus.FAILED, syncStatus);
        }
    }
}
