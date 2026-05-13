using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jellyfin.LiveTv.IO;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Jellyfin.Tests.LiveTv.IO
{
    public class EncodedRecorderTests
    {
        [Fact]
        public void Stop_LogsWaitForExitInformation()
        {
            var logger = new TestLogger();
            var recorder = new EncodedRecorder(logger, mediaEncoder: null, appPaths: null, serverConfigurationManager: null);

            SetPrivateField(recorder, "_targetPath", "/path/to/file.ts");

            var stopMethod = typeof(EncodedRecorder).GetMethod("Stop", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(stopMethod);

            stopMethod!.Invoke(recorder, Array.Empty<object>());

            var matchingEntry = Assert.Single(
                logger.Entries.Where(entry =>
                    entry.LogLevel == LogLevel.Information &&
                    entry.Message.StartsWith("Calling recording process.WaitForExit for", StringComparison.Ordinal)));

            Assert.Contains("/path/to/file.ts", matchingEntry.Message);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field!.SetValue(instance, value);
        }

        private sealed class TestLogger : ILogger
        {
            private static readonly NullScope ScopeInstance = new NullScope();

            public List<LogEntry> Entries { get; } = new List<LogEntry>();

            public IDisposable BeginScope<TState>(TState state) => ScopeInstance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
                Entries.Add(new LogEntry(logLevel, message, eventId, exception));
            }
        }

        private sealed class NullScope : IDisposable
        {
            public void Dispose()
            {
            }
        }

        private record LogEntry(LogLevel LogLevel, string Message, EventId EventId, Exception Exception);
    }
}
