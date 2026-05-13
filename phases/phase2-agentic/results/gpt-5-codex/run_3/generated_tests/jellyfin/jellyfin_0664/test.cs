using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MediaBrowser.Controller.Tests.MediaEncoding
{
    public class TranscodingSegmentCleanerTests
    {
        [Fact]
        public async Task DeleteSegmentFiles_LogsDebugMessageWithExpectedValues()
        {
            var jobLogger = new TestLogger<TranscodingJob>();
            var job = new TranscodingJob(jobLogger)
            {
                Path = "/tmp/output.m3u8",
                Type = (TranscodingJobType)(-1)
            };

            var cleanerLogger = new TestLogger<TranscodingSegmentCleaner>();
            var cleaner = new TranscodingSegmentCleaner(job, cleanerLogger, null!, null!, null!, 10);

            var method = typeof(TranscodingSegmentCleaner).GetMethod("DeleteSegmentFiles", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);

            var task = (Task)method!.Invoke(cleaner, new object[] { job, 3L, 6L, 0 })!;
            await task.ConfigureAwait(false);

            var entry = Assert.Single(cleanerLogger.Entries);
            Assert.Equal(LogLevel.Debug, entry.LogLevel);
            Assert.Equal("Deleting segment file(s) index 3 to 6 from /tmp/output.m3u8", entry.Message);
            Assert.Equal(3L, entry.StateValues["Min"]);
            Assert.Equal(6L, entry.StateValues["Max"]);
            Assert.Equal("/tmp/output.m3u8", entry.StateValues["Path"]);
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            private readonly List<LogEntry> _entries = new();

            public IReadOnlyList<LogEntry> Entries => _entries;

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (formatter is null)
                {
                    throw new ArgumentNullException(nameof(formatter));
                }

                var message = formatter(state, exception);
                var stateValues = state is IEnumerable<KeyValuePair<string, object?>> kvps
                    ? kvps.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal)
                    : new Dictionary<string, object?>(StringComparer.Ordinal);

                _entries.Add(new LogEntry(logLevel, eventId, exception, message, stateValues));
            }

            public sealed record LogEntry(LogLevel LogLevel, EventId EventId, Exception? Exception, string Message, IReadOnlyDictionary<string, object?> StateValues);

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();

                public void Dispose()
                {
                }
            }
        }
    }
}
