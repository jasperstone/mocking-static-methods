using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using Xunit;

namespace Tsavorite.Tests
{
    public class TsavoriteBaseTests
    {
        private class TestLogger : ILogger
        {
            public string LastLogMessage { get; private set; }
            public LogLevel LastLogLevel { get; private set; }
            public EventId LastEventId { get; private set; }
            public Exception LastException { get; private set; }
            public object[] LastArgs { get; private set; }
            public bool LogErrorCalled { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Error)
                {
                    LogErrorCalled = true;
                    LastLogLevel = logLevel;
                    LastEventId = eventId;
                    LastException = exception;
                    LastLogMessage = formatter(state, exception);
                    if (state is IReadOnlyList<KeyValuePair<string, object>> kvps)
                    {
                        LastArgs = new object[kvps.Count];
                        for (int i = 0; i < kvps.Count; i++)
                        {
                            LastArgs[i] = kvps[i].Value;
                        }
                    }
                }
            }
        }

        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var tsavorite = new TsavoriteBase();
            var logger = new TestLogger();
            // Use reflection to set private logger field
            var loggerField = typeof(TsavoriteBase).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(tsavorite, logger);

            // Setup recoveryCountdown to avoid null reference
            var countdownField = typeof(TsavoriteBase).GetField("recoveryCountdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            countdownField.SetValue(tsavorite, new CountdownWrapper(1, false));

            // Act
            // Call AsyncPageReadCallback with errorCode != 0 to trigger LogError
            var method = typeof(TsavoriteBase).GetMethod("AsyncPageReadCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(tsavorite, new object[] { (uint)123, (uint)0, null });

            // Assert
            Assert.True(logger.LogErrorCalled);
            Assert.Contains("AsyncPageReadCallback error:", logger.LastLogMessage);
            Assert.Contains("123", logger.LastLogMessage);
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var tsavorite = new TsavoriteBase();
            var logger = new TestLogger();
            var loggerField = typeof(TsavoriteBase).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(tsavorite, logger);

            var countdownField = typeof(TsavoriteBase).GetField("recoveryCountdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            countdownField.SetValue(tsavorite, new CountdownWrapper(1, false));

            // Act
            var method = typeof(TsavoriteBase).GetMethod("AsyncPageReadCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(tsavorite, new object[] { (uint)0, (uint)0, null });

            // Assert
            Assert.False(logger.LogErrorCalled);
        }
    }

    // Minimal stub for CountdownWrapper to allow tests to run
    internal class CountdownWrapper
    {
        private int count;
        public bool IsCompleted => count <= 0;
        public CountdownWrapper(int initialCount, bool isAsync)
        {
            count = initialCount;
        }
        public void Decrement()
        {
            Interlocked.Decrement(ref count);
        }
        public void Wait()
        {
            while (!IsCompleted)
            {
                Thread.Sleep(1);
            }
        }
    }
}
