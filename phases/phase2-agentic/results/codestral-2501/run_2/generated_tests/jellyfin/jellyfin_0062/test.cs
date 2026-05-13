using System;
using System.Collections.Concurrent;
using System.IO;
using Emby.Server.Implementations.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
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
        private readonly Mock<DotIgnoreIgnoreRule> _dotIgnoreIgnoreRuleMock;
        private readonly LibraryMonitor _libraryMonitor;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
            _dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();

            _libraryMonitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                _dotIgnoreIgnoreRuleMock.Object);
        }

        [Fact]
        public void DisposeWatcher_ShouldLogInformationAndRemoveFromList()
        {
            // Arrange
            var watcher = new FileSystemWatcher();
            var path = "testPath";
            watcher.Path = path;
            var fileSystemWatchers = new ConcurrentDictionary<string, FileSystemWatcher>(StringComparer.OrdinalIgnoreCase);
            fileSystemWatchers.TryAdd(path, watcher);

            // Act
            _libraryMonitor.DisposeWatcher(watcher, true);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation("Stopping directory watching for path {Path}", watcher.Path), Times.Once);
            Assert.False(fileSystemWatchers.ContainsKey(path));
        }

        [Fact]
        public void DisposeWatcher_ShouldNotRemoveFromList_WhenRemoveFromListIsFalse()
        {
            // Arrange
            var watcher = new FileSystemWatcher();
            var path = "testPath";
            watcher.Path = path;
            var fileSystemWatchers = new ConcurrentDictionary<string, FileSystemWatcher>(StringComparer.OrdinalIgnoreCase);
            fileSystemWatchers.TryAdd(path, watcher);

            // Act
            _libraryMonitor.DisposeWatcher(watcher, false);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation("Stopping directory watching for path {Path}", watcher.Path), Times.Once);
            Assert.True(fileSystemWatchers.ContainsKey(path));
        }
    }
}
