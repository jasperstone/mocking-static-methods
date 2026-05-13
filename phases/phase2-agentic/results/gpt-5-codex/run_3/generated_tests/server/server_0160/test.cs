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
        public void LogInformation_PassesEventIdAndFormattedMessageToUnderlyingLogger()
        {
            var captureLogger = new CaptureLogger();
            var sut = new DbUpLogger(captureLogger);

            sut.LogInformation("Migrating step {0} of {1}", 3, 5);

            Assert.Equal(LogLevel.Information, captureLogger.LastLogLevel);
            Assert.Equal(Constants.BypassFiltersEventId, captureLogger.LastEventId);
            Assert.Equal("Migrating step 3 of 5", captureLogger.LastFormattedMessage);
            Assert.Null(captureLogger.LastException);

            var state = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(captureLogger.LastState!);
            var infoMessagePair = Assert.Single(state.Where(kvp => kvp.Key == "InfoMessage"));
            Assert.Equal("Migrating step 3 of 5", infoMessagePair.Value);
            var originalFormatPair = Assert.Single(state.Where(kvp => kvp.Key == "{OriginalFormat}"));
            Assert.Equal("{InfoMessage}", originalFormatPair.Value);
        }

        private sealed class CaptureLogger : ILogger
        {
            public LogLevel? LastLogLevel { get; private set; }
            public EventId LastEventId { get; private set; }
            public object? LastState { get; private set; }
            public Exception? LastException { get; private set; }
            public string? LastFormattedMessage { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                LastLogLevel = logLevel;
                LastEventId = eventId;
                LastState = state;
                LastException = exception;
                LastFormattedMessage = formatter(state, exception);
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
