using System;
using System.Collections.Generic;
using System.Linq;
using Bit.Core;
using Bit.Migrator;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Bit.Migrator.Tests
{
    public class DbUpLoggerTests
    {
        [Fact]
        public void LogInformation_UsesBypassEventAndFormatsMessage()
        {
            var testLogger = new TestLogger();
            var sut = new DbUpLogger(testLogger);

            sut.LogInformation("Hello {0}", "World");

            var entry = Assert.Single(testLogger.Entries);
            Assert.Equal(LogLevel.Information, entry.LogLevel);
            Assert.Equal(Constants.BypassFiltersEventId, entry.EventId);
            Assert.Null(entry.Exception);
            Assert.Equal("Hello World", entry.Message);

            var state = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(entry.State);
            var infoPair = Assert.Single(state, kvp => kvp.Key == "InfoMessage");
            Assert.Equal("Hello World", infoPair.Value);
            var originalFormatPair = Assert.Single(state, kvp => kvp.Key == "{OriginalFormat}");
            Assert.Equal("{InfoMessage}", originalFormatPair.Value);
        }

        private sealed class TestLogger : ILogger
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;

                Entries.Add(new LogEntry
                {
                    LogLevel = logLevel,
                    EventId = eventId,
                    State = state,
                    Exception = exception,
                    Message = message
                });
            }

            private sealed class NullScope : IDisposable
            {
                public static NullScope Instance { get; } = new NullScope();

                public void Dispose()
                {
                }
            }
        }

        private sealed class LogEntry
        {
            public LogLevel LogLevel { get; init; }
            public EventId EventId { get; init; }
            public object? State { get; init; }
            public Exception? Exception { get; init; }
            public string Message { get; init; } = string.Empty;
        }
    }
}
