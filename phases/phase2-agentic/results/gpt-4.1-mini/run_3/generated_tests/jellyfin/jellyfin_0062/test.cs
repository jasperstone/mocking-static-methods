using System;
using System.IO;
using System.Threading.Tasks;
using Emby.Server.Implementations.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void DisposeWatcher_LogsStoppingDirectoryWatching()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var configManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var appLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();
            var dotIgnoreMock = new Mock<DotIgnoreIgnoreRule>();

            // Setup appLifetime to not call Start or Stop automatically
            appLifetimeMock.Setup(a => a.ApplicationStarted).Returns(new Microsoft.Extensions.Hosting.CancellationChangeToken(new System.Threading.CancellationToken(false)));
            appLifetimeMock.Setup(a => a.ApplicationStopping).Returns(new Microsoft.Extensions.Hosting.CancellationChangeToken(new System.Threading.CancellationToken(false)));

            var libraryMonitor = new LibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnoreMock.Object);

            // Create a FileSystemWatcher with a test path
            var watcher = new FileSystemWatcher
            {
                Path = "C:\\TestPath"
            };

            // Attach dummy event handlers to avoid null reference on -=
            FileSystemEventHandler dummyHandler = (s, e) => { };
            RenamedEventHandler dummyRenamedHandler = (s, e) => { };
            ErrorEventHandler dummyErrorHandler = (s, e) => { };

            watcher.Created += dummyHandler;
            watcher.Deleted += dummyHandler;
            watcher.Changed += dummyHandler;
            watcher.Renamed += dummyRenamedHandler;
            watcher.Error += dummyErrorHandler;

            // Act
            // Use reflection to call private DisposeWatcher method
            var disposeWatcherMethod = typeof(LibraryMonitor).GetMethod("DisposeWatcher", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            disposeWatcherMethod.Invoke(libraryMonitor, new object[] { watcher, false });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping directory watching for path C:\\TestPath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
