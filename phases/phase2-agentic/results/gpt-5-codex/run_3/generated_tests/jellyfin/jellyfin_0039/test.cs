using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Devices;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Server.Implementations.Tests.Devices
{
    public class DeviceIdTests
    {
        [Fact]
        public void Value_LogsErrorWhenCacheReadThrowsUnexpectedException()
        {
            // Arrange
            var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempRoot);
            var devicePath = Path.Combine(tempRoot, "device.txt");
            Directory.CreateDirectory(devicePath); // Force UnauthorizedAccessException on read

            var appPaths = new TestApplicationPaths(tempRoot);
            var logger = new TestLogger<DeviceId>();

            var deviceId = new DeviceId(appPaths, logger);

            try
            {
                // Act
                var value = deviceId.Value;

                // Assert
                Assert.False(string.IsNullOrWhiteSpace(value));
                var readErrors = logger.Entries.FindAll(entry =>
                    entry.LogLevel == LogLevel.Error &&
                    entry.Message == "Error reading file");

                Assert.Single(readErrors);
                Assert.IsType<UnauthorizedAccessException>(readErrors[0].Exception);
            }
            finally
            {
                Directory.Delete(tempRoot, true);
            }
        }

        private sealed class TestApplicationPaths : IApplicationPaths
        {
            public TestApplicationPaths(string dataPath)
            {
                DataPath = dataPath;
            }

            public string ProgramDataPath => throw new NotImplementedException();
            public string WebPath => throw new NotImplementedException();
            public string ProgramSystemPath => throw new NotImplementedException();
            public string DataPath { get; }
            public string ImageCachePath => throw new NotImplementedException();
            public string PluginsPath => throw new NotImplementedException();
            public string PluginConfigurationsPath => throw new NotImplementedException();
            public string LogDirectoryPath => throw new NotImplementedException();
            public string ConfigurationDirectoryPath => throw new NotImplementedException();
            public string SystemConfigurationFilePath => throw new NotImplementedException();
            public string CachePath => throw new NotImplementedException();
            public string TempDirectory => throw new NotImplementedException();
            public string VirtualDataPath => throw new NotImplementedException();
            public string TrickplayPath => throw new NotImplementedException();
            public string BackupPath => throw new NotImplementedException();
            public void MakeSanityCheckOrThrow() => throw new NotImplementedException();
            public void CreateAndCheckMarker(string path, string markerName, bool recursive = false) => throw new NotImplementedException();
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullDisposable.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Entries.Add(new LogEntry
                {
                    LogLevel = logLevel,
                    EventId = eventId,
                    State = state,
                    Exception = exception,
                    Message = formatter?.Invoke(state, exception) ?? state?.ToString() ?? string.Empty
                });
            }

            public sealed class LogEntry
            {
                public LogLevel LogLevel { get; init; }
                public EventId EventId { get; init; }
                public object State { get; init; }
                public Exception Exception { get; init; }
                public string Message { get; init; }
            }

            private sealed class NullDisposable : IDisposable
            {
                public static readonly IDisposable Instance = new NullDisposable();
                public void Dispose()
                {
                }
            }
        }
    }
}
