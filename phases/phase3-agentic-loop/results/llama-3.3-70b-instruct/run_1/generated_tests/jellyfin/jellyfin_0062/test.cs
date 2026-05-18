using Emby.Server.Implementations.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Concurrent;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void StopWatchingPath_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var configurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var libraryMonitor = new LibraryMonitor(loggerMock.Object, libraryManagerMock.Object, configurationManagerMock.Object, fileSystemMock.Object, null, null);
            var fileSystemWatcher = new FileSystemWatcher();
            var field = typeof(LibraryMonitor).GetField("_fileSystemWatchers", BindingFlags.NonPublic | BindingFlags.Instance);
            ((ConcurrentDictionary<string, FileSystemWatcher>)field.GetValue(libraryMonitor)).TryAdd(fileSystemWatcher.Path, fileSystemWatcher);

            // Act
            libraryMonitor.StopWatchingPath(fileSystemWatcher.Path);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Stopping directory watching for path {Path}", fileSystemWatcher.Path), Times.Once);
        }

        [Fact]
        public void StopWatchingPath_DisposesWatcher()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var configurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var libraryMonitor = new LibraryMonitor(loggerMock.Object, libraryManagerMock.Object, configurationManagerMock.Object, fileSystemMock.Object, null, null);
            var fileSystemWatcher = new FileSystemWatcher();
            var field = typeof(LibraryMonitor).GetField("_fileSystemWatchers", BindingFlags.NonPublic | BindingFlags.Instance);
            ((ConcurrentDictionary<string, FileSystemWatcher>)field.GetValue(libraryMonitor)).TryAdd(fileSystemWatcher.Path, fileSystemWatcher);

            // Act
            libraryMonitor.StopWatchingPath(fileSystemWatcher.Path);

            // Assert
            Assert.Throws<ObjectDisposedException>(() => fileSystemWatcher.EnableRaisingEvents = true);
        }

        [Fact]
        public void StopWatchingPath_RemovesFromList()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var configurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var libraryMonitor = new LibraryMonitor(loggerMock.Object, libraryManagerMock.Object, configurationManagerMock.Object, fileSystemMock.Object, null, null);
            var fileSystemWatcher = new FileSystemWatcher();
            var field = typeof(LibraryMonitor).GetField("_fileSystemWatchers", BindingFlags.NonPublic | BindingFlags.Instance);
            ((ConcurrentDictionary<string, FileSystemWatcher>)field.GetValue(libraryMonitor)).TryAdd(fileSystemWatcher.Path, fileSystemWatcher);

            // Act
            libraryMonitor.StopWatchingPath(fileSystemWatcher.Path);

            // Assert
            Assert.False(((ConcurrentDictionary<string, FileSystemWatcher>)field.GetValue(libraryMonitor)).TryGetValue(fileSystemWatcher.Path, out _));
        }
    }
}
