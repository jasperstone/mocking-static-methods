using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Jellyfin.Tests.MediaEncoding
{
    public class TranscodingJobTests
    {
        [Fact]
        public void Stop_WhenProcessDoesNotExitWithinTimeout_LogsKillingProcess()
        {
            var logger = new TestLogger<TranscodingJob>();
            using var process = CreateLongRunningProcess();

            var job = new TranscodingJob(logger)
            {
                Path = "/test/path",
                HasExited = false,
                Process = process
            };

            try
            {
                job.Stop();

                Assert.True(process.WaitForExit(5000), "Process should have been terminated by the Stop method.");

                var logEntry = logger.Entries.SingleOrDefault(entry =>
                    entry.LogLevel == LogLevel.Information &&
                    string.Equals(entry.OriginalFormat, "Killing FFmpeg process for {Path}", StringComparison.Ordinal));

                Assert.NotNull(logEntry);
                Assert.Equal(job.Path, logEntry!.GetValue("Path"));
                Assert.Contains("Killing FFmpeg process", logEntry.Message, StringComparison.Ordinal);
            }
            finally
            {
                if (!process.HasExited)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(2000);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }

                job.Process = null;
                job.Dispose();
            }
        }

        private static Process CreateLongRunningProcess()
        {
            var startInfo = new ProcessStartInfo
            {
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (OperatingSystem.IsWindows())
            {
                startInfo.FileName = "powershell";
                startInfo.ArgumentList.Add("-NoProfile");
                startInfo.ArgumentList.Add("-Command");
                startInfo.ArgumentList.Add("Read-Host; Start-Sleep -Seconds 30");
            }
            else
            {
                startInfo.FileName = "/bin/sh";
                startInfo.ArgumentList.Add("-c");
                startInfo.ArgumentList.Add("read dummy; sleep 30");
            }

            var process = new Process { StartInfo = startInfo };
            process.Start();
            return process;
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            private readonly List<TestLogEntry> _entries = new();

            public IReadOnlyList<TestLogEntry> Entries => _entries;

            public IDisposable? BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (formatter is null)
                {
                    throw new ArgumentNullException(nameof(formatter));
                }

                var message = formatter(state, exception);
                var structuredState = ExtractState(state);
                _entries.Add(new TestLogEntry(logLevel, eventId, message, exception, structuredState));
            }

            private static IReadOnlyList<KeyValuePair<string, object?>> ExtractState<TState>(TState state)
            {
                if (state is IReadOnlyList<KeyValuePair<string, object?>> list)
                {
                    return list;
                }

                if (state is IEnumerable<KeyValuePair<string, object?>> enumerable)
                {
                    return enumerable.ToList();
                }

                return Array.Empty<KeyValuePair<string, object?>>();
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();

                public void Dispose()
                {
                }
            }
        }

        private sealed record TestLogEntry(
            LogLevel LogLevel,
            EventId EventId,
            string Message,
            Exception? Exception,
            IReadOnlyList<KeyValuePair<string, object?>> State)
        {
            public string? OriginalFormat =>
                State.FirstOrDefault(pair => pair.Key == "{OriginalFormat}").Value as string;

            public object? GetValue(string key) =>
                State.FirstOrDefault(pair => pair.Key == key).Value;
        }
    }
}
