using System.IO;
using System.Reflection;
using Emby.Server.Implementations.IO;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Configuration;
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
        private readonly DotIgnoreIgnoreRule _dotIgnore;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
            _dotIgnore = new DotIgnoreIgnoreRule();
        }

        [Fact]
        public void DisposeWatcher_LogsInformationMessage_WithCorrectPath()
        {
            // Arrange
            var testPath = "/test/path";
            var watcher = new FileSystemWatcher(testPath);
            var monitor = CreateLibraryMonitor();
            var disposeWatcherMethod = typeof(LibraryMonitor).GetMethod("DisposeWatcher", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            disposeWatcherMethod.Invoke(monitor, new object[] { watcher, true });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Stopping directory watching for path") && 
                        v.ToString()!.Contains(testPath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DisposeWatcher_CalledWithRemoveFromListTrue_RemovesWatcher()
        {
            // Arrange
            var testPath = "/test/path";
            var watcher = new FileSystemWatcher(testPath);
            var monitor = CreateLibraryMonitor();
            
            // Use reflection to add watcher to private dictionary
            var fileSystemWatchersField = typeof(LibraryMonitor)
                .GetField("_fileSystemWatchers", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var dictionary = (System.Collections.Concurrent.ConcurrentDictionary<string, FileSystemWatcher>)fileSystemWatchersField.GetValue(monitor)!;
            dictionary.TryAdd(testPath, watcher);
            
            var disposeWatcherMethod = typeof(LibraryMonitor).GetMethod("DisposeWatcher", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            disposeWatcherMethod.Invoke(monitor, new object[] { watcher, true });

            // Assert
            Assert.False(dictionary.ContainsKey(testPath));
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Stopping directory watching for path") && 
                        v.ToString()!.Contains(testPath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private LibraryMonitor CreateLibraryMonitor()
        {
            return new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                _dotIgnore);
        }
    }
}
