using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jellyfin.LiveTv.IO;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderStopTests
    {
        [Fact]
        public void Stop_WhenProcessHasNotExited_LogsWaitForExitInformation()
        {
            var testLogger = new TestLogger();
            var mediaEncoder = new Mock<IMediaEncoder>();
            var appPaths = new Mock<IServerApplicationPaths>();
            var serverConfigurationManager = new Mock<IServerConfigurationManager>();

            var recorder = new EncodedRecorder(
                testLogger,
                mediaEncoder.Object,
                appPaths.Object,
                serverConfigurationManager.Object);

            var targetPathField = typeof(EncodedRecorder).GetField("_targetPath", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(targetPathField);
            targetPathField!.SetValue(recorder, "testPath");

            var stopMethod = typeof(EncodedRecorder).GetMethod("Stop", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(stopMethod);
            stopMethod!.Invoke(recorder, null);

            var waitForExitLogs = testLogger.Entries
                .Where(entry => entry.Level == LogLevel.Information
                    && entry.Message.Contains("Calling recording process.WaitForExit", StringComparison.Ordinal))
                .ToList();

            var waitForExitLog = Assert.Single(waitForExitLogs);
            Assert.Contains("testPath", waitForExitLog.Message);
        }

        private sealed class TestLogger : ILogger
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
                Entries.Add(new LogEntry(logLevel, eventId, message, state, exception));
            }

            public sealed record LogEntry(LogLevel Level, EventId EventId, string Message, object? State, Exception? Exception);

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
