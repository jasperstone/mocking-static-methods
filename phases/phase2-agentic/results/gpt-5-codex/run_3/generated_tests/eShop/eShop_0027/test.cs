using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Ordering.API.UnitTests.Application.Validations
{
    public class IdentifiedCommandValidatorTests
    {
        [Fact]
        public void Constructor_LogsTrace_WhenTraceEnabled()
        {
            var logger = new CapturingLogger<IdentifiedCommandValidator>(isTraceEnabled: true);

            _ = new IdentifiedCommandValidator(logger);

            var logEntry = Assert.NotNull(logger.LastLogEntry);
            Assert.Equal(LogLevel.Trace, logEntry.LogLevel);
            Assert.Equal("INSTANCE CREATED - IdentifiedCommandValidator", logEntry.Message);
            Assert.Null(logEntry.Exception);

            var state = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object>>>(logEntry.State);
            Assert.Contains(state, kvp => kvp.Key == "ClassName" && string.Equals(kvp.Value?.ToString(), nameof(IdentifiedCommandValidator), StringComparison.Ordinal));
            Assert.Contains(state, kvp => kvp.Key == "{OriginalFormat}" && string.Equals(kvp.Value?.ToString(), "INSTANCE CREATED - {ClassName}", StringComparison.Ordinal));
        }

        [Fact]
        public void Constructor_DoesNotLogTrace_WhenTraceDisabled()
        {
            var logger = new CapturingLogger<IdentifiedCommandValidator>(isTraceEnabled: false);

            _ = new IdentifiedCommandValidator(logger);

            Assert.Null(logger.LastLogEntry);
        }

        private sealed class CapturingLogger<T> : ILogger<T>
        {
            private readonly bool _isTraceEnabled;

            public CapturingLogger(bool isTraceEnabled)
            {
                _isTraceEnabled = isTraceEnabled;
            }

            public LogEntry? LastLogEntry { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => NoopDisposable.Instance;

            public bool IsEnabled(LogLevel logLevel) =>
                logLevel == LogLevel.Trace && _isTraceEnabled;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                LastLogEntry = new LogEntry(
                    logLevel,
                    eventId,
                    state!,
                    exception,
                    formatter(state, exception));
            }

            public sealed record LogEntry(LogLevel LogLevel, EventId EventId, object State, Exception? Exception, string Message);

            private sealed class NoopDisposable : IDisposable
            {
                public static NoopDisposable Instance { get; } = new();

                public void Dispose()
                {
                }
            }
        }
    }
}
