using System;
using System.IO;
using System.Threading.Tasks;
using Emby.Server.Implementations.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Tests.IO
{
    public class LibraryMonitorLoggingTests
    {
        [Fact]
        public void DisposeWatcher_LogsStoppingDirectoryWatching()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appLifetimeMock = new Mock<IHostApplicationLifetime>();
            var dotIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();

            // Create a subclass to expose the private method
            var monitor = new TestableLibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnoreRuleMock.Object);

            var watcher = new FileSystemWatcher
            {
                Path = "/some/path"
            };

            // Act
            monitor.InvokeDisposeWatcher(watcher, true);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping directory watching for path /some/path")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper subclass to expose the private method
        private class TestableLibraryMonitor : LibraryMonitor
        {
            public TestableLibraryMonitor(
                ILogger<LibraryMonitor> logger,
                ILibraryManager libraryManager,
                IServerConfigurationManager configurationManager,
                IFileSystem fileSystem,
                IHostApplicationLifetime appLifetime,
                DotIgnoreIgnoreRule dotIgnoreIgnoreRule)
                : base(logger, libraryManager, configurationManager, fileSystem, appLifetime, dotIgnoreIgnoreRule)
            {
            }

            public void InvokeDisposeWatcher(FileSystemWatcher watcher, bool removeFromList)
            {
                // Call the private method via reflection
                var method = typeof(LibraryMonitor).GetMethod("DisposeWatcher", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method.Invoke(this, new object[] { watcher, removeFromList });
            }
        }
    }
}
