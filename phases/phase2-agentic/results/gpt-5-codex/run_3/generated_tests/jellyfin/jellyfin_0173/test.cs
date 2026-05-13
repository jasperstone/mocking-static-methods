using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.Plugins
{
    public sealed class PluginManagerSaveManifestTests
    {
        [Fact]
        public void SaveManifest_WhenArgumentExceptionThrown_LogsWarningAndReturnsFalse()
        {
            var logger = new TestLogger<PluginManager>();
            var appHost = new Mock<IServerApplicationHost>(MockBehavior.Strict);
            var config = new ServerConfiguration();
            var pluginsPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

            var pluginManager = new PluginManager(
                logger,
                appHost.Object,
                config,
                pluginsPath,
                new Version(10, 0));

            var manifest = new PluginManifest();

            var result = pluginManager.SaveManifest(manifest, null);

            Assert.False(result);

            var warning = Assert.Single(logger.Entries.Where(entry => entry.LogLevel == LogLevel.Warning));
            Assert.IsType<ArgumentNullException>(warning.Exception);

            var message = warning.FormatMessage();
            Assert.Contains("Unable to save plugin manifest due to invalid value.", message);

            var stateProperties = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(warning.State);
            Assert.Contains(stateProperties, kvp => kvp.Key == "Path" && kvp.Value is null);
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                Entries.Add(new LogEntry(
                    logLevel,
                    eventId,
                    state,
                    exception,
                    (s, e) => formatter((TState)s!, e)));
            }
        }

        private sealed class LogEntry
        {
            public LogEntry(
                LogLevel logLevel,
                EventId eventId,
                object? state,
                Exception? exception,
                Func<object?, Exception?, string> formatter)
            {
                LogLevel = logLevel;
                EventId = eventId;
                State = state;
                Exception = exception;
                Formatter = formatter;
            }

            public LogLevel LogLevel { get; }

            public EventId EventId { get; }

            public object? State { get; }

            public Exception? Exception { get; }

            private Func<object?, Exception?, string> Formatter { get; }

            public string FormatMessage() => Formatter(State, Exception);
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
