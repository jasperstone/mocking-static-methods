using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Emby.Server.Implementations.IO;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorTests
    {
        private readonly Mock<ILogger<LibraryMonitor>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerConfigurationManager> _configurationManagerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IHostApplicationLifetime> _appLifetimeMock;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
        }

        [Fact]
        public void DisposeWatcher_LogsInformationMessage_WithCorrectPath()
        {
            // Arrange
            var testPath = "/test/path";
            var watcher = new FileSystemWatcher(testPath);
            var monitor = CreateLibraryMonitor();

            // Act
            InvokePrivateMethod(monitor, "DisposeWatcher", watcher, true);

            // Assert - Verify the LogInformation call on line 292
            _loggerMock.Verify(
                x => x.LogInformation("Stopping directory watching for path {Path}", watcher.Path),
                Times.Once);
        }

        [Fact]
        public void DisposeWatcher_RemovesWatcherFromDictionary_WhenRemoveFromListIsTrue()
        {
            // Arrange
            var testPath = "/test/path";
            var watcher = new FileSystemWatcher(testPath);
            var monitor = CreateLibraryMonitor();

            // Set up internal dictionary state using reflection
            var watchersField = typeof(LibraryMonitor)
                .GetField("_fileSystemWatchers", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var watchersDict = new ConcurrentDictionary<string, FileSystemWatcher>
            {
                [testPath] = watcher
            };
            watchersField.SetValue(monitor, watchersDict);

            // Act
            InvokePrivateMethod(monitor, "DisposeWatcher", watcher, true);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Stopping directory watching for path {Path}", watcher.Path),
                Times.Once);

            Assert.False(watchersDict.ContainsKey(testPath));
        }

        private LibraryMonitor CreateLibraryMonitor()
        {
            return new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                null!);
        }

        private static void InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            var method = typeof(LibraryMonitor)
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!;
            method.Invoke(target, args);
        }
    }
}
