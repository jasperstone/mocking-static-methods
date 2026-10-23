using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Xunit;
using Emby.Server.Implementations.IO;

namespace Emby.Server.Implementations.Tests.IO
{
    public class LibraryMonitorTests
    {
        private class TestLogger<T> : ILogger<T>
        {
            public LogLevel? LastLogLevel { get; private set; }
            public string? LastMessage { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null!;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                LastLogLevel = logLevel;
                LastMessage = formatter(state, exception);
            }
        }

        [Fact]
        public void DisposeWatcher_LogsStoppingDirectoryWatching()
        {
            // Arrange
            var logger = new TestLogger<LibraryMonitor>();
            var monitor = new LibraryMonitor(
                logger,
                null!, // ILibraryManager not needed for this test
                null!, // IServerConfigurationManager not needed
                null!, // IFileSystem not needed
                new DummyHostApplicationLifetime(),
                null!  // DotIgnoreIgnoreRule not needed
            );

            var watcher = new FileSystemWatcher
            {
                Path = "C:\\TestPath"
            };

            // Act
            var method = typeof(LibraryMonitor).GetMethod("DisposeWatcher", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);
            method!.Invoke(monitor, new object[] { watcher, true });

            // Assert
            Assert.Equal(LogLevel.Information, logger.LastLogLevel);
            Assert.Contains("Stopping directory watching for path", logger.LastMessage);
            Assert.Contains(watcher.Path, logger.LastMessage);
        }

        private class DummyHostApplicationLifetime : Microsoft.Extensions.Hosting.IHostApplicationLifetime
        {
            public System.Threading.CancellationToken ApplicationStarted => default;
            public System.Threading.CancellationToken ApplicationStopping => default;
            public System.Threading.CancellationToken ApplicationStopped => default;

            public void Register(Action callback) { }

            public void StopApplication() { }
        }
    }
}
