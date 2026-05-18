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
        public async Task WaitForFlushAsync_Should_LogError_When_FlushTaskThrows()
        {
            // Arrange
            var mockLogger = new DummyLogger();
            var session = new ReplicaSyncSession();

            // Use reflection to set the private logger field
            var loggerField = typeof(ReplicaSyncSession).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(session, mockLogger);

            // Create a Task that throws
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetException(new InvalidOperationException("Test exception"));

            // Use reflection to set the private flushTask field
            var flushTaskField = typeof(ReplicaSyncSession).GetField("flushTask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            flushTaskField.SetValue(session, tcs.Task);

            // Act
            await session.WaitForFlushAsync();

            // Assert
            Assert.Contains("Flush task faulted", mockLogger.LastLogMessage);
            Assert.Equal(LogLevel.Error, mockLogger.LastLogLevel);
            Assert.IsType<InvalidOperationException>(mockLogger.LastException);
        }
    }
}
