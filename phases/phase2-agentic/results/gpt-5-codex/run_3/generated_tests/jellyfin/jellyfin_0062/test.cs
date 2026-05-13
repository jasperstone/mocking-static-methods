using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Emby.Server.Implementations.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void DisposeWatcher_LogsInformationAndRemovesWatcherFromDictionary()
        {
            var logger = new TestLogger();
            var monitor = new LibraryMonitor(logger, null!, null!, null!, new NoopHostApplicationLifetime(), null!);

            var watchersField = typeof(LibraryMonitor).GetField("_fileSystemWatchers", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var watchers = (ConcurrentDictionary<string, FileSystemWatcher>)watchersField.GetValue(monitor)!;

            var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDirectory);

            try
            {
                var watcher = new FileSystemWatcher(tempDirectory);
                var watcherPath = watcher.Path;

                Assert.True(watchers.TryAdd(watcherPath, watcher));

                var disposeWatcherMethod = typeof(LibraryMonitor).GetMethod("DisposeWatcher", BindingFlags.Instance | BindingFlags.NonPublic)!;
                disposeWatcherMethod.Invoke(monitor, new object[] { watcher, true });

                Assert.False(watchers.ContainsKey(watcherPath));

                var entry = Assert.Single(logger.Entries);
                Assert.Equal(LogLevel.Information, entry.LogLevel);
                Assert.Equal("Stopping directory watching for path {Path}", entry.OriginalFormat);
                Assert.Equal(watcherPath, entry.GetValue("Path"));
                Assert.Null(entry.Exception);
            }
            finally
            {
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, recursive: true);
                }
            }
        }

        private sealed class TestLogger : ILogger<LibraryMonitor>
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NoopDisposable.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
                Entries.Add(new LogEntry(logLevel, state, exception, message));
            }
        }

        private sealed class LogEntry
        {
            public LogEntry(LogLevel logLevel, object? state, Exception? exception, string message)
            {
                LogLevel = logLevel;
                State = state;
                Exception = exception;
                Message = message;
            }

            public LogLevel LogLevel { get; }

            public object? State { get; }

            public Exception? Exception { get; }

            public string Message { get; }

            public string? OriginalFormat => GetStateValue("{OriginalFormat}") as string;

            public object? GetValue(string name) => GetStateValue(name);

            private object? GetStateValue(string name)
            {
                if (State is IEnumerable<KeyValuePair<string, object?>> kvps)
                {
                    foreach (var kvp in kvps)
                    {
                        if (kvp.Key == name)
                        {
                            return kvp.Value;
                        }
                    }
                }

                return null;
            }
        }

        private sealed class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new();

            public void Dispose()
            {
            }
        }

        private sealed class NoopHostApplicationLifetime : IHostApplicationLifetime
        {
            public CancellationToken ApplicationStarted => CancellationToken.None;

            public CancellationToken ApplicationStopping => CancellationToken.None;

            public CancellationToken ApplicationStopped => CancellationToken.None;

            public void StopApplication()
            {
            }
        }
    }
}
