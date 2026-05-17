using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.IO
{
    public class LibraryMonitorTests
    {
        private readonly Mock<ILogger<LibraryMonitor>> _loggerMock;
        private readonly Mock<IHostApplicationLifetime> _appLifetimeMock;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
        }

        [Fact]
        public void DisposeWatcher_LogsInformationMessage_WithCorrectPath()
        {
            // Arrange
            var watcher = new FileSystemWatcher("C:\\TestPath");
            var monitor = CreateLibraryMonitor();

            // Act
            monitor.GetType()
                .GetMethod("DisposeWatcher", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(monitor, new object[] { watcher, true });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t?.ToString()?.Contains("Stopping directory watching for path C:\\TestPath") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void StopWatchingPath_LogsInformationMessage_WhenWatcherExists()
        {
            // Arrange
            var path = "C:\\TestPath";
            var watcher = new FileSystemWatcher(path);
            var monitor = CreateLibraryMonitor();
            
            // Use reflection to populate the private dictionary
            var field = typeof(LibraryMonitor).GetField("_fileSystemWatchers", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            field.SetValue(monitor, new ConcurrentDictionary<string, FileSystemWatcher>(StringComparer.OrdinalIgnoreCase())
            {
                [path] = watcher
            });

            // Act
            monitor.GetType()
                .GetMethod("StopWatchingPath", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(monitor, new object[] { path });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t?.ToString()?.Contains("Stopping directory watching for path " + path) == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private LibraryMonitor CreateLibraryMonitor()
        {
            // Create minimal mocks - these won't be used in our tests but are required for constructor
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var dotIgnoreMock = new Mock<DotIgnoreIgnoreRule>();

            return new LibraryMonitor(
                _loggerMock.Object,
                libraryManagerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                _appLifetimeMock.Object,
                dotIgnoreMock.Object);
        }
    }
}
