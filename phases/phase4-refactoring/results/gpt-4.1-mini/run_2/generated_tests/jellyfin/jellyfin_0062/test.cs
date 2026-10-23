using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Logging;
using Xunit;
using Emby.Server.Implementations.IO;
using Moq;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Hosting;
using Emby.Server.Implementations.Library;

namespace Emby.Server.Implementations.Tests.IO
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void DisposeWatcher_LogsInformationAndDisposesWatcher()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appLifetimeMock = new Mock<IHostApplicationLifetime>();
            var dotIgnoreMock = new Mock<DotIgnoreIgnoreRule>();

            var libraryMonitor = new LibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnoreMock.Object);

            var watcher = new FileSystemWatcher
            {
                Path = "C:\\TestPath"
            };

            // Add watcher to internal dictionary to simulate real usage
            var watchersField = typeof(LibraryMonitor).GetField("_fileSystemWatchers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var watchersDict = (ConcurrentDictionary<string, FileSystemWatcher>)watchersField.GetValue(libraryMonitor);
            watchersDict.TryAdd(watcher.Path, watcher);

            // Act
            // Call DisposeWatcher via reflection since it is private
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

            // The watcher should be removed from dictionary
            Assert.False(watchersDict.ContainsKey(watcher.Path));
        }
    }
}
