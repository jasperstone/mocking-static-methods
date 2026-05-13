using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Server.Helpers;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsWebResourcesPath()
        {
            // Arrange
            var logger = new CollectingLogger();
            const string expectedWebPath = "/custom/web";
            var applicationPaths = new TestApplicationPaths
            {
                ProgramDataPath = "/data",
                LogDirectoryPath = "/logs",
                ConfigurationDirectoryPath = "/config",
                CachePath = "/cache",
                TempDirectory = "/temp",
                WebPath = expectedWebPath,
                ProgramSystemPath = "/system"
            };

            // Act
            StartupHelpers.LogEnvironmentInfo(logger, applicationPaths);

            // Assert
            var webPathLog = Assert.Single(
                logger.Entries,
                entry => entry.LogLevel == LogLevel.Information &&
                         entry.Message == $"Web resources path: {expectedWebPath}");

            if (webPathLog.State is IReadOnlyList<KeyValuePair<string, object>> kvps)
            {
                var webPathPair = Assert.Single(kvps.Where(kvp => kvp.Key == "WebPath"));
                Assert.Equal(expectedWebPath, webPathPair.Value);

                var originalFormatPair = Assert.Single(kvps.Where(kvp => kvp.Key == "{OriginalFormat}"));
                Assert.Equal("Web resources path: {WebPath}", originalFormatPair.Value);
            }
            else
            {
                Assert.True(false, "Logger state was not captured as structured values.");
            }
        }

        private sealed class CollectingLogger : ILogger
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var message = formatter?.Invoke(state, exception) ?? state?.ToString() ?? string.Empty;
                Entries.Add(new LogEntry(logLevel, eventId, state!, exception, message));
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();

                public void Dispose()
                {
                }
            }
        }

        private sealed record LogEntry(LogLevel LogLevel, EventId EventId, object State, Exception? Exception, string Message);

        private sealed class TestApplicationPaths : IApplicationPaths
        {
            public string ProgramDataPath { get; set; } = string.Empty;
            public string WebPath { get; set; } = string.Empty;
            public string ProgramSystemPath { get; set; } = string.Empty;
            public string DataPath { get; set; } = string.Empty;
            public string ImageCachePath { get; set; } = string.Empty;
            public string PluginsPath { get; set; } = string.Empty;
            public string PluginConfigurationsPath { get; set; } = string.Empty;
            public string LogDirectoryPath { get; set; } = string.Empty;
            public string ConfigurationDirectoryPath { get; set; } = string.Empty;
            public string SystemConfigurationFilePath { get; set; } = string.Empty;
            public string CachePath { get; set; } = string.Empty;
            public string TempDirectory { get; set; } = string.Empty;
            public string VirtualDataPath { get; set; } = string.Empty;
            public string TrickplayPath { get; set; } = string.Empty;
            public string BackupPath { get; set; } = string.Empty;

            public void MakeSanityCheckOrThrow()
            {
            }

            public void CreateAndCheckMarker(string path, string markerName, bool recursive = false)
            {
            }
        }
    }
}
