using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Server.Helpers;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Helpers;

public class StartupHelpersTests
{
    [Fact]
    public void LogEnvironmentInfo_LogsApplicationDirectoryFromAppPaths()
    {
        var logger = new TestLogger();

        const string programDataPath = "/var/lib/jellyfin/data";
        const string logDirectoryPath = "/var/log/jellyfin";
        const string configurationDirectoryPath = "/etc/jellyfin";
        const string cachePath = "/var/cache/jellyfin";
        const string tempDirectory = "/tmp/jellyfin";
        const string webPath = "/opt/jellyfin/web";
        const string expectedProgramSystemPath = "/opt/jellyfin/system";

        var appPaths = new Mock<IApplicationPaths>(MockBehavior.Strict);
        appPaths.SetupGet(x => x.ProgramDataPath).Returns(programDataPath);
        appPaths.SetupGet(x => x.LogDirectoryPath).Returns(logDirectoryPath);
        appPaths.SetupGet(x => x.ConfigurationDirectoryPath).Returns(configurationDirectoryPath);
        appPaths.SetupGet(x => x.CachePath).Returns(cachePath);
        appPaths.SetupGet(x => x.TempDirectory).Returns(tempDirectory);
        appPaths.SetupGet(x => x.WebPath).Returns(webPath);
        appPaths.SetupGet(x => x.ProgramSystemPath).Returns(expectedProgramSystemPath);

        StartupHelpers.LogEnvironmentInfo(logger, appPaths.Object);

        var applicationLog = Assert.Single(logger.Entries.Where(entry =>
            entry.Message.StartsWith("Application directory:", StringComparison.Ordinal)));

        Assert.Equal(LogLevel.Information, applicationLog.Level);
        Assert.Equal($"Application directory: {expectedProgramSystemPath}", applicationLog.Message);

        var structuredState = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(applicationLog.State);
        var applicationPath = Assert.Single(structuredState.Where(pair => pair.Key == "ApplicationPath"));
        Assert.Equal(expectedProgramSystemPath, applicationPath.Value);
    }

    private sealed class TestLogger : ILogger
    {
        public List<LogEntry> Entries { get; } = new();

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            Entries.Add(new LogEntry(logLevel, eventId, state!, exception, message));
        }

        internal sealed record LogEntry(LogLevel Level, EventId EventId, object State, Exception? Exception, string Message);

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();

            public void Dispose()
            {
            }
        }
    }
}
