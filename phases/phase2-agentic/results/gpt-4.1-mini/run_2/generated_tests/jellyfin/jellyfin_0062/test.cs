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
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();
            var dotIgnoreMock = new Mock<DotIgnoreIgnoreRule>();

            // Setup appLifetime to allow Start and Stop registration without error
            appLifetimeMock.Setup(a => a.ApplicationStarted).Returns(new Microsoft.Extensions.Hosting.ApplicationStartedLifetime());
            appLifetimeMock.Setup(a => a.ApplicationStopping).Returns(new Microsoft.Extensions.Hosting.ApplicationStoppingLifetime());

            var libraryMonitor = new LibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnoreMock.Object);

            var watcher = new FileSystemWatcher
            {
                Path = "C:\\TestPath",
                EnableRaisingEvents = true
            };

            // Attach dummy event handlers to allow removal without error
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
            disposeWatcherMethod.Invoke(libraryMonitor, new object[] { watcher, true });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping directory watching for path C:\\TestPath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(watcher.EnableRaisingEvents);
        }
    }
}
