using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Volo.Abp.Core.Tests.Logging
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWithLevel_CriticalWithoutException_LogsCriticalMessage()
        {
            var logger = new CollectingLogger();

            logger.LogWithLevel(LogLevel.Critical, "critical message");

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Critical, entry.LogLevel);
            Assert.Equal("critical message", entry.Message);
            Assert.Null(entry.Exception);
        }

        [Fact]
        public void LogWithLevel_CriticalWithException_LogsCriticalException()
        {
            var logger = new CollectingLogger();
            var exception = new InvalidOperationException("boom");

            logger.LogWithLevel(LogLevel.Critical, "critical message", exception);

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Critical, entry.LogLevel);
            Assert.Equal("critical message", entry.Message);
            Assert.Same(exception, entry.Exception);
        }

        private sealed class CollectingLogger : ILogger
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString();
                Entries.Add(new LogEntry(logLevel, eventId, state, exception, message));
            }

            public sealed class LogEntry
            {
                public LogEntry(LogLevel logLevel, EventId eventId, object state, Exception exception, string message)
                {
                    LogLevel = logLevel;
                    EventId = eventId;
                    State = state;
                    Exception = exception;
                    Message = message;
                }

                public LogLevel LogLevel { get; }
                public EventId EventId { get; }
                public object State { get; }
                public Exception Exception { get; }
                public string Message { get; }
            }

            private sealed class NullScope : IDisposable
            {
                public static NullScope Instance { get; } = new NullScope();

                public void Dispose()
                {
                }
            }
        }
    }
}
